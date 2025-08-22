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
    [SerializeField] private Button _seedsCategoryButton;
    [SerializeField] private Button _vehicleCategoryButton;
    
    [Header("판매 탭")]
    [SerializeField] private Transform _sellItemsParent;
    [SerializeField] private GameObject _sellItemSlotPrefab;

    [Header("아이템 정보 패널 (구매/판매 공통)")]
    [SerializeField] private Transform _itemInfoParent;
    [SerializeField] private Button _upButton;
    [SerializeField] private Button _downButton;
    [SerializeField] private Button _actionButton;
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _quantityText;
    [SerializeField] private TextMeshProUGUI _totalPriceText;
    [SerializeField] private TextMeshProUGUI _actionButtonText;

    private List<UI_ShopItemSlot> _buySlots = new List<UI_ShopItemSlot>();
    private List<UI_ShopItemSlot> _sellSlots = new List<UI_ShopItemSlot>();
    private EShopCategory _currentCategory = EShopCategory.Seeds;
    private bool _isBuyTabActive = true;
    
    // 아이템 선택 관련 변수 (구매/판매 공통)
    private EItemType _selectedItemType;
    private int _selectedQuantity = 1;
    private int _maxQuantity = 0;
    private int _unitPrice = 0;
    private bool _hasItemSelected = false;
    private bool _isBuyMode = true;
    
    private void Awake()
    {
        SetupButtons();
    }
    
    private void SetupButtons()
    {
        if (_closeButton != null)
            _closeButton.onClick.AddListener(OnClose);
            
        if (_buyTabButton != null)
            _buyTabButton.onClick.AddListener(() => SwitchTab(true));
            
        if (_sellTabButton != null)
            _sellTabButton.onClick.AddListener(() => SwitchTab(false));
            
        if (_seedsCategoryButton != null)
            _seedsCategoryButton.onClick.AddListener(() => SetCategory(EShopCategory.Seeds));
            
        if (_vehicleCategoryButton != null)
            _vehicleCategoryButton.onClick.AddListener(() => SetCategory(EShopCategory.Vehicle));
            
        // 아이템 정보 패널 버튼들
        if (_upButton != null)
            _upButton.onClick.AddListener(IncreaseQuantity);
            
        if (_downButton != null)
            _downButton.onClick.AddListener(DecreaseQuantity);
            
        if (_actionButton != null)
            _actionButton.onClick.AddListener(ExecuteAction);
    }
    
    private void Start()
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OnItemPurchased.AddListener(OnItemPurchased);
            ShopManager.Instance.OnItemSold.AddListener(OnItemSold);
        }
        
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged.AddListener(OnCurrencyChanged);
        }
    }
    public override void Open(System.Action callback = null)
    {
        base.Open();
        OpenShop();
        SoundManager.Instance.PlayBGM(BGMType.Store);
    }

    public void OnClose()
    {
        PopupManager.Instance.PopUpClose();

        _buyTabButton.image.color = Color.white;
        _buyTabPanel.gameObject.SetActive(true);

        _sellTabButton.image.color = Color.white;
        _sellTabPanel.gameObject.SetActive(false);

        SoundManager.Instance.PlayBGM(BGMType.Pm);
    }

    public void OpenShop()
    {
        if (ShopManager.Instance == null || ShopManager.Instance.ShopData == null)
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
        if (_shopNameText != null && ShopManager.Instance.ShopData != null)
        {
            _shopNameText.text = ShopManager.Instance.ShopData.shopName;
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

        _buyTabButton.image.color = Color.white;
        _sellTabButton.image.color = Color.white;
        if (isBuyTab)
        {
            _buyTabButton.image.color = Color.green;
            RefreshBuyItems();
        }
        else
        {
            _sellTabButton.image.color = Color.green;
            RefreshSellItems();
            ClearItemInfo(); // 판매 탭으로 전환할 때 선택 정보 초기화
        }
        
        _isBuyMode = isBuyTab;
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
        
        if (ShopManager.Instance == null) return;
        
        var buyableItems = ShopManager.Instance.GetBuyableItems(_currentCategory);
        
        foreach (var shopItem in buyableItems)
        {
            CreateBuySlot(shopItem);
        }
    }
    
    private void RefreshSellItems()
    {
        ClearSellSlots();
        
        if (ShopManager.Instance == null) return;
        
        var sellableItems = ShopManager.Instance.GetSellableItems();
        
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
            slot.SetupBuySlot(shopItem, OnBuyItemSelected);
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
            int sellPrice = ShopManager.Instance.GetSellPrice(inventoryItem.ItemType);
            slot.SetupSellSlot(inventoryItem, sellPrice, OnSellItemSelected);
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
    
    private void OnBuyItemSelected(EItemType itemType, int maxQuantity)
    {
        // 구매 아이템 선택 시 정보 패널에 표시
        var shopItem = ShopManager.Instance.ShopData?.GetShopItem(itemType);
        if (shopItem != null)
        {
            int buyPrice = shopItem.BuyPrice;
            SetupItemInfo(itemType, maxQuantity, buyPrice, true);
        }
    }
    
    private void OnSellItemSelected(EItemType itemType, int maxQuantity)
    {
        // 판매 아이템 선택 시 정보 패널에 표시
        int sellPrice = ShopManager.Instance.GetSellPrice(itemType, 1);
        SetupItemInfo(itemType, maxQuantity, sellPrice, false);
    }
    
    private void OnItemPurchased(ShopTransaction transaction)
    {
        RefreshBuyItems();

        SoundManager.Instance.PlaySFX(SFXType.StoreCash);
    }
    
    private void OnItemSold(ShopTransaction transaction)
    {
        RefreshSellItems();

        SoundManager.Instance.PlaySFX(SFXType.StoreCash);
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
        if (_playerMoneyText != null && CurrencyManager.Instance != null)
        {
            int money = CurrencyManager.Instance.GetCurrencyAmount(ECurrencyType.Money);
            _playerMoneyText.text = $"{money:N0}원";
        }
    }
    
    private void OnDestroy()
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OnItemPurchased.RemoveListener(OnItemPurchased);
            ShopManager.Instance.OnItemSold.RemoveListener(OnItemSold);
        }
        
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged.RemoveListener(OnCurrencyChanged);
        }
        
        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(OnClose);
    }
    
    #region 아이템 정보 시스템 (구매/판매 공통)
    private void SetupItemInfo(EItemType itemType, int maxQuantity, int unitPrice, bool isBuyMode)
    {
        _selectedItemType = itemType;
        _maxQuantity = maxQuantity;
        _selectedQuantity = 1;
        _unitPrice = unitPrice;
        _hasItemSelected = true;
        _isBuyMode = isBuyMode;
        
        // 아이템 정보 표시
        var itemData = InventoryManager.Instance?.ItemDatabase?.GetItemData(itemType);
        
        if (_itemIcon != null)
        {
            _itemIcon.sprite = InventoryManager.Instance.ItemDatabase.GetItemIcon(itemType);
            _itemIcon.color = Color.white;
        }
        
        if (_itemNameText != null)
        {
            string itemName = itemData?.GetDisplayName() ?? itemType.ToString();
            _itemNameText.text = itemName;
        }
        
        // 액션 버튼 텍스트 설정
        if (_actionButtonText != null)
            _actionButtonText.text = isBuyMode ? "구매" : "판매";
        
        // 정보 패널 활성화
        if (_itemInfoParent != null)
            _itemInfoParent.gameObject.SetActive(true);
            
        UpdateItemInfo();
    }
    
    private void UpdateItemInfo()
    {
        if (!_hasItemSelected) return;
        
        // 수량 표시
        if (_quantityText != null)
        {
            if (_isBuyMode)
                _quantityText.text = $"{_selectedQuantity}개";
            else
                _quantityText.text = $"{_selectedQuantity} / {_maxQuantity}";
        }
        
        // 총 가격 표시
        if (_totalPriceText != null)
        {
            int totalPrice = _unitPrice * _selectedQuantity;
            _totalPriceText.text = $"총액: {totalPrice:N0}원";
        }
        
        // 버튼 상태 업데이트
        if (_upButton != null)
            _upButton.interactable = _selectedQuantity < _maxQuantity;
            
        if (_downButton != null)
            _downButton.interactable = _selectedQuantity > 1;
            
        if (_actionButton != null)
        {
            if (_isBuyMode)
            {
                // 구매 가능 여부 체크
                int totalPrice = _unitPrice * _selectedQuantity;
                bool canAfford = ShopManager.Instance.CanAfford(totalPrice);
                bool hasSpace = InventoryManager.Instance?.CanAddItem(_selectedItemType, _selectedQuantity) ?? false;
                _actionButton.interactable = canAfford && hasSpace && _selectedQuantity > 0;
            }
            else
            {
                // 판매 가능 여부 체크
                _actionButton.interactable = _selectedQuantity > 0 && _selectedQuantity <= _maxQuantity;
            }
        }
    }
    
    private void ClearItemInfo()
    {
        _hasItemSelected = false;
        _selectedQuantity = 1;
        _maxQuantity = 0;
        _unitPrice = 0;
        
        if (_itemInfoParent != null)
            _itemInfoParent.gameObject.SetActive(false);
    }
    
    private void IncreaseQuantity()
    {
        if (_hasItemSelected && _selectedQuantity < _maxQuantity)
        {
            _selectedQuantity++;
            UpdateItemInfo();
        }
    }
    
    private void DecreaseQuantity()
    {
        if (_hasItemSelected && _selectedQuantity > 1)
        {
            _selectedQuantity--;
            UpdateItemInfo();
        }
    }
    
    private async void ExecuteAction()
    {
        if (!_hasItemSelected) return;
        
        bool success;
        if (_isBuyMode)
        {
            success = await ShopManager.Instance.TryBuyItem(_selectedItemType, _selectedQuantity);
        }
        else
        {
            success = await ShopManager.Instance.TrySellItem(_selectedItemType, _selectedQuantity);
        }
        
        if (success)
        {
            // 성공 시 UI 갱신
            if (_isBuyMode)
                RefreshBuyItems();
            else
                RefreshSellItems();
                
            ClearItemInfo();
        }
    }
    #endregion
}