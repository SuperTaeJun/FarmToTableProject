using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UI_ShopItemSlot : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private TextMeshProUGUI _quantityText;
    [SerializeField] private Button _actionButton;
    [SerializeField] private TextMeshProUGUI _actionButtonText;
    
    [Header("수량 조절")]
    [SerializeField] private Button _decreaseButton;
    [SerializeField] private Button _increaseButton;
    [SerializeField] private TextMeshProUGUI _selectedQuantityText;
    
    private EItemType _itemType;
    private int _selectedQuantity = 1;
    private int _maxQuantity = 1;
    private int _unitPrice;
    private bool _isBuyMode;
    private Action<EItemType, int> _onActionClicked;
    
    private void Awake()
    {
        SetupButtons();
    }
    
    private void SetupButtons()
    {
        if (_actionButton != null)
            _actionButton.onClick.AddListener(OnActionButtonClicked);
            
        if (_decreaseButton != null)
            _decreaseButton.onClick.AddListener(DecreaseQuantity);
            
        if (_increaseButton != null)
            _increaseButton.onClick.AddListener(IncreaseQuantity);
    }
    
    public void SetupBuySlot(ShopItem shopItem, Action<EItemType, int> onBuyClicked)
    {
        _itemType = shopItem.ItemType;
        _unitPrice = shopItem.BuyPrice;
        _isBuyMode = true;
        _onActionClicked = onBuyClicked;
        _maxQuantity = shopItem.HasUnlimitedStock ? 99 : shopItem.Stock;
        
        SetupSlotUI();
        
        if (_actionButtonText != null)
            _actionButtonText.text = "구매";
    }
    
    public void SetupSellSlot(InventoryItem inventoryItem, int sellPrice, Action<EItemType, int> onSellClicked)
    {
        _itemType = inventoryItem.ItemType;
        _unitPrice = sellPrice;
        _isBuyMode = false;
        _onActionClicked = onSellClicked;
        _maxQuantity = inventoryItem.Quantity;
        
        SetupSlotUI();
        
        if (_actionButtonText != null)
            _actionButtonText.text = "판매";
            
        if (_quantityText != null)
            _quantityText.text = $"보유: {inventoryItem.Quantity}";
    }
    
    private void SetupSlotUI()
    {
        _selectedQuantity = 1;
        
        // 아이템 아이콘 설정
        if (_itemIcon != null)
        {
            var itemData = InventoryManager.Instance?.GetComponent<ItemDatabase>()?.GetItemData(_itemType);
            if (itemData?.icon != null)
            {
                _itemIcon.sprite = itemData.icon;
                _itemIcon.color = Color.white;
            }
            else
            {
                _itemIcon.sprite = null;
                _itemIcon.color = Color.gray;
            }
        }
        
        // 아이템 이름 설정
        if (_itemNameText != null)
        {
            _itemNameText.text = GetItemDisplayName();
        }
        
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        // 가격 표시
        if (_priceText != null)
        {
            int totalPrice = _unitPrice * _selectedQuantity;
            string pricePrefix = _isBuyMode ? "구매가: " : "판매가: ";
            _priceText.text = $"{pricePrefix}{totalPrice:N0}원";
        }
        
        // 선택된 수량 표시
        if (_selectedQuantityText != null)
        {
            _selectedQuantityText.text = _selectedQuantity.ToString();
        }
        
        // 버튼 활성화/비활성화
        if (_decreaseButton != null)
            _decreaseButton.interactable = _selectedQuantity > 1;
            
        if (_increaseButton != null)
            _increaseButton.interactable = _selectedQuantity < _maxQuantity;
            
        // 구매/판매 버튼 활성화 체크
        if (_actionButton != null)
        {
            if (_isBuyMode)
            {
                // 구매 가능 여부 체크 (돈, 인벤토리 공간)
                var shopManager = ShopManager.Instance;
                bool canAfford = shopManager != null && shopManager.CanAfford(_unitPrice * _selectedQuantity);
                bool hasSpace = InventoryManager.Instance != null && InventoryManager.Instance.CanAddItem(_itemType, _selectedQuantity);
                _actionButton.interactable = canAfford && hasSpace;
            }
            else
            {
                // 판매 가능 여부 체크 (보유 수량)
                _actionButton.interactable = _selectedQuantity <= _maxQuantity;
            }
        }
    }
    
    private void DecreaseQuantity()
    {
        if (_selectedQuantity > 1)
        {
            _selectedQuantity--;
            UpdateUI();
        }
    }
    
    private void IncreaseQuantity()
    {
        if (_selectedQuantity < _maxQuantity)
        {
            _selectedQuantity++;
            UpdateUI();
        }
    }
    
    private void OnActionButtonClicked()
    {
        _onActionClicked?.Invoke(_itemType, _selectedQuantity);
    }
    
    private string GetItemDisplayName()
    {
        // ItemDatabase에서 아이템 이름 가져오기
        var itemData = InventoryManager.Instance?.GetComponent<ItemDatabase>()?.GetItemData(_itemType);
        return itemData?.GetDisplayName() ?? _itemType.ToString();
    }
    
    private void OnDestroy()
    {
        if (_actionButton != null)
            _actionButton.onClick.RemoveListener(OnActionButtonClicked);
            
        if (_decreaseButton != null)
            _decreaseButton.onClick.RemoveListener(DecreaseQuantity);
            
        if (_increaseButton != null)
            _increaseButton.onClick.RemoveListener(IncreaseQuantity);
    }
}