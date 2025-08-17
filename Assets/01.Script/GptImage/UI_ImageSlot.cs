using UnityEngine;
using UnityEngine.UI;

public class UI_ImageSlot : MonoBehaviour
{
    [SerializeField] private RawImage imageDisplay;
    [SerializeField] private Button selectButton;
    [SerializeField] private GameObject selectedBorder;

    private int slotIndex;
    private System.Action<int> onClickCallback;

    public void Setup(Texture2D texture, int index, System.Action<int> clickCallback)
    {
        if (imageDisplay != null)
            imageDisplay.texture = texture;

        slotIndex = index;
        onClickCallback = clickCallback;

        if (selectButton != null)
            selectButton.onClick.AddListener(() => onClickCallback?.Invoke(slotIndex));

        SetSelected(false);

    }
    public void SetSelected(bool selected)
    {
        if (selectedBorder != null)
            selectedBorder.SetActive(selected);
    }

}
