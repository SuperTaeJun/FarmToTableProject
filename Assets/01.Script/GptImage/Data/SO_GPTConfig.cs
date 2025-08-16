using UnityEngine;

[CreateAssetMenu(fileName = "SO_GPTConfig", menuName = "Scriptable Objects/SO_GPTConfig")]
public class SO_GPTConfig : ScriptableObject
{
    [Header("API 설정")]
    [SerializeField] private string _apiKey;
    [Header("기본 프롬프트")]
    [TextArea(3, 5)]
    [SerializeField] private string basePrompt;

    public string ApiKey => _apiKey;
    public string BasePrompt => basePrompt;

    public string CombinePrompts(string userPrompt)
    {
        if (string.IsNullOrEmpty(basePrompt))
            return userPrompt;

        // 구분자 추가 (개행 두 줄 권장)
        return $"{basePrompt}\n\n{userPrompt}";
    }
}
