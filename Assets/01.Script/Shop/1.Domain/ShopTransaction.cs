using System;

[Serializable]
public class ShopTransaction
{
    public string TransactionId { get; private set; }
    public ETransactionType TransactionType { get; private set; }
    public EItemType ItemType { get; private set; }
    public int Quantity { get; private set; }
    public int UnitPrice { get; private set; }
    public int TotalPrice { get; private set; }
    public DateTime TransactionTime { get; private set; }
    public EShopType ShopType { get; private set; }

    public ShopTransaction(ETransactionType transactionType, EItemType itemType, int quantity, int unitPrice, EShopType shopType)
    {
        TransactionId = Guid.NewGuid().ToString();
        TransactionType = transactionType;
        ItemType = itemType;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = unitPrice * quantity;
        TransactionTime = DateTime.Now;
        ShopType = shopType;
    }

    public ShopTransaction(string transactionId, ETransactionType transactionType, EItemType itemType, int quantity, int unitPrice, int totalPrice, DateTime transactionTime, EShopType shopType)
    {
        TransactionId = transactionId;
        TransactionType = transactionType;
        ItemType = itemType;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = totalPrice;
        TransactionTime = transactionTime;
        ShopType = shopType;
    }
}