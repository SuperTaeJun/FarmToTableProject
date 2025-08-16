using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ImageGenerator : UI_Popup
{
    [Header("UI ������Ʈ")]
    [SerializeField] private TMP_InputField promptInput;
    [SerializeField] private Button generateButton;
    [SerializeField] private RawImage displayImage;
    [SerializeField] private TextMeshProUGUI statusText;


    private void Start()
    {
        generateButton.onClick.AddListener(OnGenerateButtonClicked);
        SetStatus("������Ʈ�� �Է��ϰ� �̹����� �����ϼ���.");
    }

    private void OnGenerateButtonClicked()
    {
        string prompt = promptInput.text.Trim();

        if (string.IsNullOrEmpty(prompt))
        {
            return;
        }

        // 백그라운드에서 이미지 생성 시작
        ImageGenerationManager.Instance.GenerateImage(prompt, OnImageGenerated);
        
        // 팝업 즉시 닫기
        Close();
    }

    private void OnImageGenerated(Texture2D texture)
    {
        if (texture == null)
        {
            return;
        }

        Debug.Log("이미지 생성 완료!");
    }
    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
