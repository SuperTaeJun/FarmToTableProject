using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ImagePreview : UI_Popup
{
    [Header("UI 컴포넌트")]
    [SerializeField] private RawImage previewImage;
    [SerializeField] private Button confirmButton;

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        
    }

    public override void Open(System.Action callback = null)
    {
        base.Open(callback);
        
        // 대기중인 이미지를 가져와서 표시
        Texture2D pendingImage = ImageGenerationManager.Instance.GetPendingImage();
        if (pendingImage != null && previewImage != null)
        {
            previewImage.texture = pendingImage;
        }
    }

    private void OnConfirmButtonClicked()
    {
        // 이미지 확정 및 저장
        ImageGenerationManager.Instance.ConfirmPendingImage();
        
        // 팝업 닫기
        Close();
    }
}