using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ImageGenerationManager : MonoBehaviour
{
    public static ImageGenerationManager Instance { get; private set; }

    [Header("설정")]
    [SerializeField] private SO_GPTConfig config;

    // OpenAI 이미지 생성 API 엔드포인트
    private const string API_URL = "https://api.openai.com/v1/images/generations";

    // 생성된 이미지들을 저장하는 리스트 (최대 3개)
    private List<Texture2D> _generatedImages = new List<Texture2D>();
    private const int MAX_IMAGES = 3;
    private const string PREFS_KEY_COUNT = "GeneratedImageCount";
    private const string PREFS_KEY_IMAGE_PREFIX = "GeneratedImage_";

    // 대기중인 이미지 (확인 전 임시 저장)
    private Texture2D _pendingImage;

    // 선택된 이미지 인덱스 (PaintingFunction용)
    private int _selectedImageIndex = -1;

    // 개별 PaintingFunction용 콜백
    private System.Action<Texture2D> _selectedImageCallback;

    // 이미지 생성 완료 시 전달되는 이벤트
    public event Action<Texture2D> OnImageGenerationComplete;
    
    // LetterFunction 상태 초기화 이벤트
    public event Action OnImageConfirmed;

    public List<Texture2D> GeneratedImages => _generatedImages;

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 게임 시작시 저장된 이미지 로드
            LoadImagesFromPlayerPrefs();
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
            // 설정이 없는 경우 중단
            return;
        }

        if (string.IsNullOrEmpty(prompt))
        {
            // 프롬프트가 비어 있으면 중단
            return;
        }

        StartCoroutine(GenerateImageCoroutine(prompt, onSuccess));
    }

    /// <summary>
    /// 이미지 생성 코루틴. API 호출 및 응답 처리.
    /// </summary>
    private IEnumerator GenerateImageCoroutine(string prompt, Action<Texture2D> onSuccess)
    {
        Debug.Log($"이미지 생성 요청: {prompt}");

        // SO_GPTConfig에 정의된 시스템/스타일 프롬프트와 합치기
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

            // 요청 전송
            yield return webRequest.SendWebRequest();

            // 에러 처리
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(null);
                yield break;
            }

            // 응답 파싱
            var response = JsonUtility.FromJson<ImageGenerationResponse>(webRequest.downloadHandler.text);
            if (response?.data == null || response.data.Length == 0)
            {
                onSuccess?.Invoke(null);
                yield break;
            }

            // URL로 이미지 다운로드
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

    /// <summary>
    /// 생성된 이미지 URL을 받아 텍스처 다운로드.
    /// </summary>
    private IEnumerator DownloadImage(string imageUrl, Action<Texture2D> onSuccess)
    {
        Debug.Log($"이미지 다운로드 시작: {imageUrl}");

        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                onSuccess?.Invoke(texture);
                OnImageGenerationComplete?.Invoke(texture);
            }
            else
            {
                onSuccess?.Invoke(null);
            }
        }
    }

    /// <summary>
    /// 생성된 이미지를 리스트에 추가 (수동 저장용)
    /// </summary>
    public void AddGeneratedImage(Texture2D texture)
    {
        // 최대 개수 초과시 가장 오래된 이미지 제거
        if (_generatedImages.Count >= MAX_IMAGES)
        {
            // 가장 오래된 이미지(첫 번째) 제거
            if (_generatedImages[0] != null)
            {
                DestroyImmediate(_generatedImages[0]);
            }
            _generatedImages.RemoveAt(0);
        }

        // 새 이미지 추가
        _generatedImages.Add(texture);
    }

    /// <summary>
    /// 이미지들을 PlayerPrefs에 저장 (LetterFunction에서 Execute시 호출용)
    /// </summary>
    public void SaveImagesToPlayerPrefs()
    {
        // 이미지 개수 저장
        PlayerPrefs.SetInt(PREFS_KEY_COUNT, _generatedImages.Count);

        // 각 이미지를 Base64로 인코딩하여 저장
        for (int i = 0; i < _generatedImages.Count; i++)
        {
            if (_generatedImages[i] != null)
            {
                byte[] textureBytes = _generatedImages[i].EncodeToPNG();
                string base64String = Convert.ToBase64String(textureBytes);
                PlayerPrefs.SetString(PREFS_KEY_IMAGE_PREFIX + i, base64String);
            }
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// PlayerPrefs에서 이미지들을 로드
    /// </summary>
    private void LoadImagesFromPlayerPrefs()
    {
        int imageCount = PlayerPrefs.GetInt(PREFS_KEY_COUNT, 0);

        for (int i = 0; i < imageCount; i++)
        {
            string base64String = PlayerPrefs.GetString(PREFS_KEY_IMAGE_PREFIX + i, "");
            
            if (!string.IsNullOrEmpty(base64String))
            {
                try
                {
                    byte[] textureBytes = Convert.FromBase64String(base64String);
                    Texture2D texture = new Texture2D(2, 2);
                    
                    if (texture.LoadImage(textureBytes))
                    {
                        _generatedImages.Add(texture);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"이미지 로드 실패: {e.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 특정 인덱스의 이미지 삭제
    /// </summary>
    public void RemoveImageAt(int index)
    {
        if (index >= 0 && index < _generatedImages.Count)
        {
            if (_generatedImages[index] != null)
            {
                DestroyImmediate(_generatedImages[index]);
            }
            _generatedImages.RemoveAt(index);
            SaveImagesToPlayerPrefs();
        }
    }

    /// 모든 저장된 이미지 삭제
    public void ClearAllImages()
    {
        // 메모리에서 텍스처 해제
        foreach (var texture in _generatedImages)
        {
            if (texture != null)
            {
                DestroyImmediate(texture);
            }
        }
        
        _generatedImages.Clear();
        
        // PlayerPrefs에서도 삭제
        PlayerPrefs.SetInt(PREFS_KEY_COUNT, 0);
        for (int i = 0; i < MAX_IMAGES; i++)
        {
            PlayerPrefs.DeleteKey(PREFS_KEY_IMAGE_PREFIX + i);
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 대기중인 이미지 설정 (확인 팝업용)
    /// </summary>
    public void SetPendingImage(Texture2D texture)
    {
        _pendingImage = texture;
    }

    /// <summary>
    /// 대기중인 이미지 가져오기
    /// </summary>
    public Texture2D GetPendingImage()
    {
        return _pendingImage;
    }

    /// <summary>
    /// 대기중인 이미지를 확정하여 저장
    /// </summary>
    public void ConfirmPendingImage()
    {
        if (_pendingImage != null)
        {
            // 생성된 이미지를 매니저에 추가
            AddGeneratedImage(_pendingImage);
            
            // PlayerPrefs에 저장
            SaveImagesToPlayerPrefs();
            
            // 대기중인 이미지 초기화
            _pendingImage = null;
            
            // LetterFunction 상태 초기화 이벤트 발생
            OnImageConfirmed?.Invoke();
            
            Debug.Log("이미지가 성공적으로 저장되었습니다!");
        }
    }

    /// <summary>
    /// 이미지 선택 콜백 설정 (PaintingFunction용)
    /// </summary>
    public void SetImageSelectedCallback(System.Action<Texture2D> callback)
    {
        _selectedImageCallback = callback;
    }

    /// <summary>
    /// 이미지 선택 (PaintingFunction용)
    /// </summary>
    public void SelectImage(int index)
    {
        if (index >= 0 && index < _generatedImages.Count)
        {
            _selectedImageIndex = index;
            
            // 등록된 콜백이 있으면 호출 (특정 PaintingFunction만 대상)
            _selectedImageCallback?.Invoke(_generatedImages[index]);
            
            // 콜백 사용 후 초기화 (한 번만 사용)
            _selectedImageCallback = null;
        }
    }

    /// <summary>
    /// 현재 선택된 이미지 가져오기
    /// </summary>
    public Texture2D GetSelectedImage()
    {
        if (_selectedImageIndex >= 0 && _selectedImageIndex < _generatedImages.Count)
        {
            return _generatedImages[_selectedImageIndex];
        }
        return null;
    }

    /// <summary>
    /// 선택된 이미지 인덱스 가져오기
    /// </summary>
    public int GetSelectedImageIndex()
    {
        return _selectedImageIndex;
    }
}

[Serializable]
public class ImageGenerationRequest
{
    // 사용할 모델 이름
    public string model = "dall-e-3";//"gpt-image-1";
    // 프롬프트 텍스트
    public string prompt;
    // 생성할 이미지 개수
    public int n = 1;
    // 이미지 해상도
    public string size = "1024x1024";

}

[Serializable]
public class ImageGenerationResponse
{
    // 반환된 이미지 데이터 배열
    public ImageData[] data;
}

[Serializable]
public class ImageData
{
    // 이미지 URL (서버에 호스팅된 경우)
    public string url;
    // Base64로 인코딩된 이미지 데이터 (옵션)
    public string b64_json;
}
