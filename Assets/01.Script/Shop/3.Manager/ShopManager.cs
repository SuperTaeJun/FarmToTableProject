using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("상점 설정")]
    [SerializeField] private SO_ShopData _shopData;
    
    public static ShopManager Instance;
    private InventoryManager _inventoryManager;
    private CurrencyManager _currencyManager;
    
    public SO_ShopData ShopData => _shopData;
    
    // 상점 이벤트들
    public DebugEvent<ShopTransaction> OnItemPurchased = new DebugEvent<ShopTransaction>();
    public DebugEvent<ShopTransaction> OnItemSold = new DebugEvent<ShopTransaction>();
    
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
        _currencyManager = CurrencyManager.Instance;
    }
    

    public async Task<bool> TryBuyItem(EItemType itemType, int quantity = 1)
    {
        if (_shopData == null) return false;
        
        var shopItem = _shopData.GetShopItem(itemType);
        if (shopItem == null || !shopItem.IsAvailableForPurchase(quantity))
        {
            Debug.LogWarning($"아이템 '{itemType}'을 구매할 수 없습니다.");
            return false;
        }
        
        // 바이시클 아이템이 이미 해금되어 있는지 확인
        if (IsVehicleItem(itemType) && IsVehicleAlreadyUnlocked(itemType))
        {
            Debug.LogWarning($"'{itemType}' 차량은 이미 해금되어 있습니다.");
            return false;
        }
        
        int totalPrice = shopItem.GetTotalBuyPrice(quantity);
        if (!_currencyManager.CanAfford(ECurrencyType.Money, totalPrice))
        {
            int currentMoney = _currencyManager.GetCurrencyAmount(ECurrencyType.Money);
            Debug.LogWarning($"돈이 부족합니다. 필요: {totalPrice}, 보유: {currentMoney}");
            return false;
        }
        
        // 차량 아이템이 아닌 경우에만 인벤토리 공간 확인
        if (!IsVehicleItem(itemType) && !_inventoryManager.CanAddItem(itemType, quantity))
        {
            Debug.LogWarning("인벤토리 공간이 부족합니다.");
            return false;
        }
        
        // 구매 실행
        if (shopItem.TryPurchase(quantity))
        {
            // 차량 아이템이 아닌 경우에만 인벤토리에 추가
            if (!IsVehicleItem(itemType))
            {
                await _inventoryManager.TryAddItem(itemType, quantity);
            }
            
            await _currencyManager.TrySpendCurrency(ECurrencyType.Money, totalPrice);
            
            // 바이시클 아이템 구매 시 해금 처리
            if (IsVehicleItem(itemType))
            {
                VehicleManager.Instance.UnlockVehicleFromItem(itemType);
            }
            
            var transaction = new ShopTransaction(ETransactionType.Buy, itemType, quantity, shopItem.BuyPrice, _shopData.shopType);
            OnItemPurchased.Invoke(transaction);
            Debug.Log($"구매 완료: {itemType} x{quantity} (총 {totalPrice}원)");
            return true;
        }
        
        return false;
    }
    
    public List<ShopItem> GetBuyableItems(EShopCategory category)
    {
        if (_shopData == null) return new List<ShopItem>();
        
        var allItems = _shopData.GetAllShopItems().Where(item => item.CanBuy).ToList();
        
        return category switch
        {
            EShopCategory.Seeds => allItems.Where(item => IsItemInCategory(item.ItemType, EShopCategory.Seeds)).ToList(),
            EShopCategory.Vehicle => allItems.Where(item => IsItemInCategory(item.ItemType, EShopCategory.Vehicle)).ToList(),
            _ => new List<ShopItem>()
        };
    }
    public async Task<bool> TrySellItem(EItemType itemType, int quantity = 1)
    {
        if (_shopData == null) return false;
        
        var shopItem = _shopData.GetShopItem(itemType);
        if (shopItem == null || !shopItem.IsAvailableForSale())
        {
            Debug.LogWarning($"아이템을 판매할 수 없습니다.");
            return false;
        }
        
        if (!_inventoryManager.HasItem(itemType, quantity))
        {
            return false;
        }
        
        // 판매 실행
        if (await _inventoryManager.TryRemoveItem(itemType, quantity))
        {
            int totalPrice = shopItem.GetTotalSellPrice(quantity);
            await _currencyManager.TryEarnCurrency(ECurrencyType.Money, totalPrice);
            
            var transaction = new ShopTransaction(ETransactionType.Sell, itemType, quantity, shopItem.SellPrice, _shopData.shopType);
            OnItemSold.Invoke(transaction);
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

    public bool CanAfford(int amount)
    {
        return _currencyManager?.CanAfford(ECurrencyType.Money, amount) ?? false;
    }
    
    public bool CanAfford(ECurrencyType currencyType, int amount)
    {
        return _currencyManager?.CanAfford(currencyType, amount) ?? false;
    }
    
    public int GetPlayerMoney()
    {
        return _currencyManager?.GetCurrencyAmount(ECurrencyType.Money) ?? 0;
    }
    
    public int GetPlayerCurrency(ECurrencyType currencyType)
    {
        return _currencyManager?.GetCurrencyAmount(currencyType) ?? 0;
    }

    private bool IsItemInCategory(EItemType itemType, EShopCategory category)
    {
        return category switch
        {
            EShopCategory.Seeds => itemType.ToString().EndsWith("Seed"),
            EShopCategory.Vehicle => IsVehicleItem(itemType),
            _ => false
        };
    }
    
    public int GetBuyPrice(EItemType itemType, int quantity = 1)
    {
        if (_shopData == null) return 0;
        
        var shopItem = _shopData.GetShopItem(itemType);
        return shopItem?.GetTotalBuyPrice(quantity) ?? 0;
    }
    
    private bool IsVehicleItem(EItemType itemType)
    {
        return VehicleManager.Instance.ItemToVehicleMap.ContainsKey(itemType);
    }
    
    private bool IsVehicleAlreadyUnlocked(EItemType itemType)
    {
        if (VehicleManager.Instance.ItemToVehicleMap.TryGetValue(itemType, out EVehicleType vehicleType))
        {
            return VehicleManager.Instance.IsVehicleUnlocked(vehicleType);
        }
        return false;
    }

}