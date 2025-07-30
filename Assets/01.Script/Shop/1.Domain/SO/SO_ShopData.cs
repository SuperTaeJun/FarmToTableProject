using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Data", menuName = "Shop/Shop Data")]
public class SO_ShopData : ScriptableObject
{
    [Header("상점 기본 정보")]
    public EShopType shopType;
    public string shopName;
    [TextArea(3, 5)]
    public string shopDescription;
    
    [Header("상점 아이템 목록")]
    public List<SO_ShopItemData> availableItems = new List<SO_ShopItemData>();
    
    
    public ShopItem GetShopItem(EItemType itemType)
    {
        var itemData = availableItems.Find(item => item.Type == itemType);
        if (itemData == null) return null;
        

        return new ShopItem(
            itemData.Type,
            itemData.Stock,
            itemData.BuyPrice,
            itemData.SellPrice,
            itemData.CanBuy,
            itemData.CanSell,
            itemData.HasUnlimitedStock
        );
    }
    
    public List<ShopItem> GetAllShopItems()
    {
        var shopItems = new List<ShopItem>();
        foreach (var itemData in availableItems)
        {
            var shopItem = GetShopItem(itemData.Type);
            if (shopItem != null)
            {
                shopItems.Add(shopItem);
            }
        }
        return shopItems;
    }
}

//[System.Serializable]
//public class ShopItemData
//{
//    public EItemType itemType;
//    public int stock = 99;
//    public int buyPrice = 10;
//    public int sellPrice = 5;
//    public bool canBuy = true;
//    public bool canSell = true;
//    public bool hasUnlimitedStock = true;
//}