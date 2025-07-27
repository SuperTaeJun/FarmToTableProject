using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Shop : UI_Popup
{
    [Header("상점 기본 UI")]
    [SerializeField] private Button _closeButton;
    [SerializeField] private TextMeshProUGUI _shopNameText;
    [SerializeField] private TextMeshProUGUI _playerMoneyText;
    
    [Header("탭 시스템")]
    [SerializeField] private Button _buyTabButton;
    [SerializeField] private Button _sellTabButton;
    [SerializeField] private GameObject _buyTabPanel;
    [SerializeField] private GameObject _sellTabPanel;
    
    [Header("구매 탭")]
    [SerializeField] private Transform _buyItemsParent;
    [SerializeField] private GameObject _buyItemSlotPrefab;
    [SerializeField] private Button _allCategoryButton;
    [SerializeField] private Button _seedsCategoryButton;
    [SerializeField] private Button _materialsCategoryButton;
    
    [Header("판매 탭")]
    [SerializeField] private Transform _sellItemsParent;
    [SerializeField] private GameObject _sellItemSlotPrefab;
    
    private ShopManager _shopManager;
    private CurrencyManager _currencyManager;
    private List<UI_ShopItemSlot> _buySlots = new List<UI_ShopItemSlot>();
    private List<UI_ShopItemSlot> _sellSlots = new List<UI_ShopItemSlot>();
    private EShopCategory _currentCategory = EShopCategory.All;
    private bool _isBuyTabActive = true;
    
    private void Awake()
    {
        SetupButtons();
    }
    
    private void SetupButtons()
    {
        if (_closeButton != null)
            _closeButton.onClick.AddListener(Close);
            
        if (_buyTabButton != null)
            _buyTabButton.onClick.AddListener(() => SwitchTab(true));
            
        if (_sellTabButton != null)
            _sellTabButton.onClick.AddListener(() => SwitchTab(false));
            
        if (_allCategoryButton != null)
            _allCategoryButton.onClick.AddListener(() => SetCategory(EShopCategory.All));
            
        if (_seedsCategoryButton != null)
            _seedsCategoryButton.onClick.AddListener(() => SetCategory(EShopCategory.Seeds));
            
        if (_materialsCategoryButton != null)
            _materialsCategoryButton.onClick.AddListener(() => SetCategory(EShopCategory.Materials));
    }
    
    private void Start()
    {
        _shopManager = ShopManager.Instance;
        _currencyManager = CurrencyManager.Instance;
        
        if (_shopManager != null)
        {
            _shopManager.OnItemPurchased.AddListener(OnItemPurchased);
            _shopManager.OnItemSold.AddListener(OnItemSold);
        }
        
        if (_currencyManager != null)
        {
            _currencyManager.OnCurrencyChanged.AddListener(OnCurrencyChanged);
        }
    }
    
    public void OpenShop()
    {
        if (_shopManager == null || _shopManager.ShopData == null)
        {
            Debug.LogError("ShopManager 또는 ShopData가 설정되지 않았습니다.");
            return;
        }
        
        UpdateShopInfo();
        SwitchTab(true); // 기본적으로 구매 탭 열기
        UpdateMoneyDisplay();
    }
    
    private void UpdateShopInfo()
    {
        if (_shopNameText != null && _shopManager.ShopData != null)
        {
            _shopNameText.text = _shopManager.ShopData.shopName;
        }
    }
    
    private void SwitchTab(bool isBuyTab)
    {
        _isBuyTabActive = isBuyTab;
        
        if (_buyTabPanel != null)
            _buyTabPanel.SetActive(isBuyTab);
            
        if (_sellTabPanel != null)
            _sellTabPanel.SetActive(!isBuyTab);
        
        // 탭 버튼 스타일 업데이트 (선택적)
        UpdateTabButtonStyles();
        
        if (isBuyTab)
        {
            RefreshBuyItems();
        }
        else
        {
            RefreshSellItems();
        }
    }
    
    private void UpdateTabButtonStyles()
    {
        // 선택된 탭 스타일 변경 (색상 등)
        if (_buyTabButton != null)
        {
            var colors = _buyTabButton.colors;
            colors.normalColor = _isBuyTabActive ? Color.yellow : Color.white;
            _buyTabButton.colors = colors;
        }
        
        if (_sellTabButton != null)
        {
            var colors = _sellTabButton.colors;
            colors.normalColor = !_isBuyTabActive ? Color.yellow : Color.white;
            _sellTabButton.colors = colors;
        }
    }
    
    private void SetCategory(EShopCategory category)
    {
        _currentCategory = category;
        RefreshBuyItems();
    }
    
    private void RefreshBuyItems()
    {
        ClearBuySlots();
        
        if (_shopManager == null) return;
        
        var buyableItems = _shopManager.GetBuyableItems(_currentCategory);
        
        foreach (var shopItem in buyableItems)
        {
            CreateBuySlot(shopItem);
        }
    }
    
    private void RefreshSellItems()
    {
        ClearSellSlots();
        
        if (_shopManager == null) return;
        
        var sellableItems = _shopManager.GetSellableItems();
        
        foreach (var inventoryItem in sellableItems)
        {
            CreateSellSlot(inventoryItem);
        }
    }
    
    private void CreateBuySlot(ShopItem shopItem)
    {
        if (_buyItemSlotPrefab == null || _buyItemsParent == null) return;
        
        GameObject slotObj = Instantiate(_buyItemSlotPrefab, _buyItemsParent);
        UI_ShopItemSlot slot = slotObj.GetComponent<UI_ShopItemSlot>();
        
        if (slot != null)
        {
            slot.SetupBuySlot(shopItem, OnBuyItemClicked);
            _buySlots.Add(slot);
        }
    }
    
    private void CreateSellSlot(InventoryItem inventoryItem)
    {
        if (_sellItemSlotPrefab == null || _sellItemsParent == null) return;
        
        GameObject slotObj = Instantiate(_sellItemSlotPrefab, _sellItemsParent);
        UI_ShopItemSlot slot = slotObj.GetComponent<UI_ShopItemSlot>();
        
        if (slot != null)
        {
            int sellPrice = _shopManager.GetSellPrice(inventoryItem.ItemType);
            slot.SetupSellSlot(inventoryItem, sellPrice, OnSellItemClicked);
            _sellSlots.Add(slot);
        }
    }
    
    private void ClearBuySlots()
    {
        foreach (var slot in _buySlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        _buySlots.Clear();
    }
    
    private void ClearSellSlots()
    {
        foreach (var slot in _sellSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        _sellSlots.Clear();
    }
    
    private async void OnBuyItemClicked(EItemType itemType, int quantity)
    {
        await _shopManager.TryBuyItem(itemType, quantity);
    }
    
    private async void OnSellItemClicked(EItemType itemType, int quantity)
    {
        await _shopManager.TrySellItem(itemType, quantity);
    }
    
    private void OnItemPurchased(ShopTransaction transaction)
    {
        RefreshBuyItems();
        // 구매 효과나 알림 표시
    }
    
    private void OnItemSold(ShopTransaction transaction)
    {
        RefreshSellItems();
        // 판매 효과나 알림 표시
    }
    
    private void OnCurrencyChanged(ECurrencyType currencyType, int oldAmount, int newAmount)
    {
        if (currencyType == ECurrencyType.Money)
        {
            UpdateMoneyDisplay();
        }
    }
    
    private void UpdateMoneyDisplay()
    {
        if (_playerMoneyText != null && _currencyManager != null)
        {
            int money = _currencyManager.GetCurrencyAmount(ECurrencyType.Money);
            _playerMoneyText.text = $"💰 {money:N0}";
        }
    }
    
    public new void Open(System.Action callback = null)
    {
        base.Open(callback);
        OpenShop();
    }
    
    private void OnDestroy()
    {
        if (_shopManager != null)
        {
            _shopManager.OnItemPurchased.RemoveListener(OnItemPurchased);
            _shopManager.OnItemSold.RemoveListener(OnItemSold);
        }
        
        if (_currencyManager != null)
        {
            _currencyManager.OnCurrencyChanged.RemoveListener(OnCurrencyChanged);
        }
        
        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(Close);
    }
}