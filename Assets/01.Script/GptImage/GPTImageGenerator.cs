using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class ImageGenerationRequest
{
    public string model = "gpt-image-1";//"dall-e-3";//"gpt-image-1";
    //�޸�3 �� ����
    //1024��1024: �� $0.04 / ��
    public string prompt;
    public int n = 1;
    public string size = "1024x1024";
    public string quality = "low";
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

public class ImageGenerationManager : MonoBehaviour
{
    public static ImageGenerationManager Instance { get; private set; }
    
    [Header("����")]
    [SerializeField] private SO_GPTConfig config;

    private const string API_URL = "https://api.openai.com/v1/images/generations";
    
    public event Action<Texture2D> OnImageGenerationComplete;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

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
                onSuccess?.Invoke(null);
                yield break;
            }

            var response = JsonUtility.FromJson<ImageGenerationResponse>(webRequest.downloadHandler.text);
            if (response?.data == null || response.data.Length == 0)
            {
                onSuccess?.Invoke(null);
                yield break;
            }

            if (!string.IsNullOrEmpty(response.data[0].url))
            {
                yield return StartCoroutine(DownloadImage(response.data[0].url, onSuccess));
            }
            else
            {
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
                OnImageGenerationComplete?.Invoke(texture);
            }
        }
    }
}
