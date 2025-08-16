using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImageGeneratorTest : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private TMP_InputField promptInput;
    [SerializeField] private Button generateButton;
    [SerializeField] private RawImage displayImage;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("매니저")]
    [SerializeField] private GPTImageGenerator imageGenerator;

    private void Start()
    {
        generateButton.onClick.AddListener(OnGenerateButtonClicked);
        SetStatus("프롬프트를 입력하고 이미지를 생성하세요.");
    }

    private void OnGenerateButtonClicked()
    {
        string prompt = promptInput.text.Trim();

        if (string.IsNullOrEmpty(prompt))
        {
            SetStatus("프롬프트를 입력해주세요.");
            return;
        }

        generateButton.interactable = false;
        SetStatus("이미지 생성 중...");

        imageGenerator.GenerateImage(prompt, OnImageGenerated);
    }

    private void OnImageGenerated(Texture2D texture)
    {
        if (texture == null)
        {
            SetStatus("이미지 생성 실패: 프롬프트/설정/키를 확인해주세요.");
            generateButton.interactable = true;
            return;
        }

        displayImage.texture = texture;
        SetStatus("이미지 생성 완료!");
        generateButton.interactable = true;
    }
    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
