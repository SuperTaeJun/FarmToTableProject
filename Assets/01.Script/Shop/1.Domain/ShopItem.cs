using System;

[Serializable]
public class ShopItem
{
    public EItemType ItemType { get; private set; }
    public int Stock { get; private set; }
    public bool HasUnlimitedStock { get; private set; }
    public int BuyPrice { get; private set; }
    public int SellPrice { get; private set; }
    public bool CanBuy { get; private set; }
    public bool CanSell { get; private set; }

    public ShopItem(EItemType itemType, int stock, int buyPrice, int sellPrice, bool canBuy = true, bool canSell = true, bool hasUnlimitedStock = true)
    {
        ItemType = itemType;
        Stock = stock;
        BuyPrice = buyPrice;
        SellPrice = sellPrice;
        CanBuy = canBuy;
        CanSell = canSell;
        HasUnlimitedStock = hasUnlimitedStock;
    }

    public bool IsAvailableForPurchase(int quantity = 1)
    {
        return CanBuy && (HasUnlimitedStock || Stock >= quantity);
    }

    public bool IsAvailableForSale()
    {
        return CanSell;
    }

    public bool TryPurchase(int quantity)
    {
        if (!IsAvailableForPurchase(quantity)) return false;

        if (!HasUnlimitedStock)
        {
            Stock -= quantity;
        }
        return true;
    }

    public void RestockItem(int quantity)
    {
        if (!HasUnlimitedStock)
        {
            Stock += quantity;
        }
    }

    public int GetTotalBuyPrice(int quantity)
    {
        return BuyPrice * quantity;
    }

    public int GetTotalSellPrice(int quantity)
    {
        return SellPrice * quantity;
    }
}