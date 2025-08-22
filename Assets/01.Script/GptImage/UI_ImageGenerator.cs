using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ImageGenerator : UI_Popup
{
    [Header("UI 컴포넌트")]
    [SerializeField] private TMP_InputField _promptInput;
    [SerializeField] private Button _generateButton;
    [SerializeField] private Button _closeButton;
    private void Start()
    {
        _generateButton.onClick.AddListener(OnGenerateButtonClicked);
        _closeButton.onClick.AddListener(()=> { PopupManager.Instance.PopUpClose(); _promptInput.text = string.Empty; });
    }

    private void OnGenerateButtonClicked()
    {
        string prompt = _promptInput.text.Trim();

        if (string.IsNullOrEmpty(prompt))
        {
            return;
        }

        ImageGenerationManager.Instance.GenerateImage(prompt, OnImageGenerated);
        _promptInput.text = string.Empty;
        PopupManager.Instance.PopUpClose();
    }

    private void OnImageGenerated(Texture2D texture)
    {
        if (texture == null)
        {
            return;
        }

        Debug.Log("이미지 생성 완료!");
    }
}
