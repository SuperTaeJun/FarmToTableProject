using UnityEngine;
using UnityEngine.UI;

public class UI_ChunkPopup : UI_Popup
{
    [SerializeField] private Button _purchaseButton;

    private void Start()
    {
        _purchaseButton.onClick.AddListener(()=> OnClickedPurchaseButton());
        Close();
    }
    private void OnClickedPurchaseButton()
    {
        WorldManager.Instance.TryGenerateChunk();
        Close();
    }
}
