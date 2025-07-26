using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_InventorySlot : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private Button _slotButton;
    [SerializeField] private GameObject _emptyIndicator;

    private int _slotIndex;
    private InventoryItem _currentItem;

    public int SlotIndex => _slotIndex;
    public InventoryItem CurrentItem => _currentItem;
    public bool IsEmpty => _currentItem == null;

    private void Awake()
    {
        if (_slotButton != null)
            _slotButton.onClick.AddListener(OnSlotClicked);
    }

    public void Initialize(int slotIndex)
    {
        _slotIndex = slotIndex;
        ClearSlot();
    }

    public void SetItem(InventoryItem item)
    {
        _currentItem = item;

        if (item != null)
        {
            // 아이템 아이콘 설정
            if (_itemIcon != null)
            {
                _itemIcon.gameObject.SetActive(true);
                Sprite itemSprite = item.GetIcon();
                if (itemSprite != null)
                {
                    _itemIcon.sprite = itemSprite;
                    _itemIcon.color = Color.white;
                }
                else
                {
                    _itemIcon.sprite = null;
                    _itemIcon.color = Color.gray;
                }
            }

            // 수량 텍스트 설정
            if (_countText != null)
            {
                _countText.gameObject.SetActive(true);
                _countText.text = item.Quantity > 1 ? item.Quantity.ToString() : "";
            }

            // 빈 슬롯 표시기 숨기기
            if (_emptyIndicator != null)
                _emptyIndicator.SetActive(false);
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        _currentItem = null;

        if (_itemIcon != null)
            _itemIcon.gameObject.SetActive(false);

        if (_countText != null)
            _countText.gameObject.SetActive(false);

        if (_emptyIndicator != null)
            _emptyIndicator.SetActive(true);
    }

    private void OnSlotClicked()
    {
        if (_currentItem != null)
        {
            Debug.Log($"클릭된 아이템: {_currentItem.ItemType} x{_currentItem.Quantity}");
            // TODO: 아이템 사용/드롭 등의 기능 구현
            ShowItemDetails();
        }
    }

    private void ShowItemDetails()
    {
        Debug.Log($"아이템 상세 정보: {_currentItem.GetName()}");
        Debug.Log($"설명: {_currentItem.GetDescription()}");
        Debug.Log($"수량: {_currentItem.Quantity}");
        Debug.Log($"획득 시간: {_currentItem.AcquiredTime}");
        Debug.Log($"최대 스택 크기: {_currentItem.GetMaxStackSize()}");
    }

    private void OnDestroy()
    {
        if (_slotButton != null)
            _slotButton.onClick.RemoveListener(OnSlotClicked);
    }
}