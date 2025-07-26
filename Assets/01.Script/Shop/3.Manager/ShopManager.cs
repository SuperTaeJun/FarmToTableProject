using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("상점 설정")]
    [SerializeField] private ShopData _shopData;
    [SerializeField] private int _playerMoney = 1000; // 임시로 여기서 관리
    
    public static ShopManager Instance;
    private InventoryManager _inventoryManager;
    
    public int PlayerMoney => _playerMoney;
    public ShopData ShopData => _shopData;
    
    // 상점 이벤트들
    public DebugEvent<ShopTransaction> OnItemPurchased = new DebugEvent<ShopTransaction>();
    public DebugEvent<ShopTransaction> OnItemSold = new DebugEvent<ShopTransaction>();
    public DebugEvent<int> OnMoneyChanged = new DebugEvent<int>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        _inventoryManager = InventoryManager.Instance;
    }
    
    #region 구매 시스템
    public async Task<bool> TryBuyItem(EItemType itemType, int quantity = 1)
    {
        if (_shopData == null) return false;
        
        var shopItem = _shopData.GetShopItem(itemType);
        if (shopItem == null || !shopItem.IsAvailableForPurchase(quantity))
        {
            Debug.LogWarning($"아이템 '{itemType}'을 구매할 수 없습니다.");
            return false;
        }
        
        int totalPrice = shopItem.GetTotalBuyPrice(quantity);
        if (!CanAfford(totalPrice))
        {
            Debug.LogWarning($"돈이 부족합니다. 필요: {totalPrice}, 보유: {_playerMoney}");
            return false;
        }
        
        if (!_inventoryManager.CanAddItem(itemType, quantity))
        {
            Debug.LogWarning("인벤토리 공간이 부족합니다.");
            return false;
        }
        
        // 구매 실행
        if (shopItem.TryPurchase(quantity))
        {
            await _inventoryManager.TryAddItem(itemType, quantity);
            DeductMoney(totalPrice);
            
            var transaction = new ShopTransaction(ETransactionType.Buy, itemType, quantity, shopItem.BuyPrice, _shopData.shopType);
            OnItemPurchased.Invoke(transaction);
            Debug.Log($"구매 완료: {itemType} x{quantity} (총 {totalPrice}원)");
            return true;
        }
        
        return false;
    }
    
    public List<ShopItem> GetBuyableItems(EShopCategory category = EShopCategory.All)
    {
        if (_shopData == null) return new List<ShopItem>();
        
        var allItems = _shopData.GetAllShopItems().Where(item => item.CanBuy).ToList();
        
        return category switch
        {
            EShopCategory.Seeds => allItems.Where(item => IsItemInCategory(item.ItemType, EShopCategory.Seeds)).ToList(),
            EShopCategory.Materials => allItems.Where(item => IsItemInCategory(item.ItemType, EShopCategory.Materials)).ToList(),
            _ => allItems
        };
    }
    #endregion
    
    #region 판매 시스템
    public async Task<bool> TrySellItem(EItemType itemType, int quantity = 1)
    {
        if (_shopData == null) return false;
        
        var shopItem = _shopData.GetShopItem(itemType);
        if (shopItem == null || !shopItem.IsAvailableForSale())
        {
            Debug.LogWarning($"아이템 '{itemType}'을 판매할 수 없습니다.");
            return false;
        }
        
        if (!_inventoryManager.HasItem(itemType, quantity))
        {
            Debug.LogWarning($"판매할 아이템이 부족합니다: {itemType} x{quantity}");
            return false;
        }
        
        // 판매 실행
        if (await _inventoryManager.TryRemoveItem(itemType, quantity))
        {
            int totalPrice = shopItem.GetTotalSellPrice(quantity);
            AddMoney(totalPrice);
            
            var transaction = new ShopTransaction(ETransactionType.Sell, itemType, quantity, shopItem.SellPrice, _shopData.shopType);
            OnItemSold.Invoke(transaction);
            Debug.Log($"판매 완료: {itemType} x{quantity} (총 {totalPrice}원)");
            return true;
        }
        
        return false;
    }
    
    public List<InventoryItem> GetSellableItems()
    {
        if (_inventoryManager == null) return new List<InventoryItem>();
        
        return _inventoryManager.Items.Where(item => CanSellItem(item.ItemType)).ToList();
    }
    
    public bool CanSellItem(EItemType itemType)
    {
        if (_shopData == null) return false;
        
        var shopItem = _shopData.GetShopItem(itemType);
        return shopItem != null && shopItem.IsAvailableForSale();
    }
    
    public int GetSellPrice(EItemType itemType, int quantity = 1)
    {
        if (_shopData == null) return 0;
        
        var shopItem = _shopData.GetShopItem(itemType);
        return shopItem?.GetTotalSellPrice(quantity) ?? 0;
    }
    #endregion
    
    #region 돈 관리
    public bool CanAfford(int amount)
    {
        return _playerMoney >= amount;
    }
    
    public void AddMoney(int amount)
    {
        if (amount > 0)
        {
            _playerMoney += amount;
            OnMoneyChanged.Invoke(_playerMoney);
        }
    }
    
    public void DeductMoney(int amount)
    {
        if (amount > 0 && _playerMoney >= amount)
        {
            _playerMoney -= amount;
            OnMoneyChanged.Invoke(_playerMoney);
        }
    }
    #endregion
    
    #region 유틸리티
    private bool IsItemInCategory(EItemType itemType, EShopCategory category)
    {
        return category switch
        {
            EShopCategory.Seeds => itemType.ToString().EndsWith("Seed"),
            EShopCategory.Materials => itemType == EItemType.Wood || itemType == EItemType.Stone,
            _ => true
        };
    }
    
    public int GetBuyPrice(EItemType itemType, int quantity = 1)
    {
        if (_shopData == null) return 0;
        
        var shopItem = _shopData.GetShopItem(itemType);
        return shopItem?.GetTotalBuyPrice(quantity) ?? 0;
    }
    #endregion
}