using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class ImageGenerationRequest
{
    public string model = "dall-e-3";//"gpt-image-1";
    //달리3 모델 가격
    //1024×1024: 약 $0.04 / 장
    //512×512: 약 $0.018 / 장
    //256×256: 약 $0.016 / 장
    public string prompt;
    public int n = 1;
    public string size = "512×512";
}

[Serializable]
public class ImageGenerationResponse
{
    public ImageData[] data;
}

[Serializable]
public class ImageData
{
    public string url;      
    public string b64_json; 
}

public class GPTImageGenerator : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private SO_GPTConfig config;

    private const string API_URL = "https://api.openai.com/v1/images/generations";

    public void GenerateImage(string prompt, Action<Texture2D> onSuccess)
    {
        if (config == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(prompt))
        {
            return;
        }

        StartCoroutine(GenerateImageCoroutine(prompt, onSuccess));
    }

    private IEnumerator GenerateImageCoroutine(string prompt, Action<Texture2D> onSuccess)
    {
        string finalPrompt = config.CombinePrompts(prompt);
        Debug.Log($"[ImageGen] finalPrompt: {finalPrompt}");

        var requestObj = new ImageGenerationRequest { prompt = finalPrompt };
        string jsonData = JsonUtility.ToJson(requestObj);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", "Bearer " + config.ApiKey);

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[ImageGen] HTTP Error: {webRequest.responseCode} - {webRequest.error}\n{webRequest.downloadHandler.text}");
                onSuccess?.Invoke(null);
                yield break;
            }

            var response = JsonUtility.FromJson<ImageGenerationResponse>(webRequest.downloadHandler.text);
            if (response?.data == null || response.data.Length == 0)
            {
                Debug.LogError("[ImageGen] Empty data in response");
                onSuccess?.Invoke(null);
                yield break;
            }

            if (!string.IsNullOrEmpty(response.data[0].url))
            {
                yield return StartCoroutine(DownloadImage(response.data[0].url, onSuccess));
            }
            else
            {
                Debug.LogError("[ImageGen] No url in response");
                onSuccess?.Invoke(null);
            }
        }
    }

    private IEnumerator DownloadImage(string imageUrl, Action<Texture2D> onSuccess)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                onSuccess?.Invoke(texture);
            }
        }
    }
}
