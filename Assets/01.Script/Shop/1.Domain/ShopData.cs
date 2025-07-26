using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Data", menuName = "Shop/Shop Data")]
public class ShopData : ScriptableObject
{
    [Header("상점 기본 정보")]
    public EShopType shopType;
    public string shopName;
    [TextArea(3, 5)]
    public string shopDescription;
    
    [Header("상점 아이템 목록")]
    public List<ShopItemData> availableItems = new List<ShopItemData>();
    
    [Header("상점 설정")]
    [Range(0f, 1f)]
    public float discountRate = 0f;
    public bool isOpen = true;
    
    public ShopItem GetShopItem(EItemType itemType)
    {
        var itemData = availableItems.Find(item => item.itemType == itemType);
        if (itemData == null) return null;
        
        int buyPrice = Mathf.RoundToInt(itemData.buyPrice * (1f - discountRate));
        int sellPrice = Mathf.RoundToInt(itemData.sellPrice * (1f - discountRate));
        
        return new ShopItem(
            itemData.itemType,
            itemData.stock,
            buyPrice,
            sellPrice,
            itemData.canBuy,
            itemData.canSell,
            itemData.hasUnlimitedStock
        );
    }
    
    public List<ShopItem> GetAllShopItems()
    {
        var shopItems = new List<ShopItem>();
        foreach (var itemData in availableItems)
        {
            var shopItem = GetShopItem(itemData.itemType);
            if (shopItem != null)
            {
                shopItems.Add(shopItem);
            }
        }
        return shopItems;
    }
}

[System.Serializable]
public class ShopItemData
{
    public EItemType itemType;
    public int stock = 99;
    public int buyPrice = 10;
    public int sellPrice = 5;
    public bool canBuy = true;
    public bool canSell = true;
    public bool hasUnlimitedStock = true;
}