using Firebase.Firestore;
using System;

[FirestoreData]
public class InventoryItemDto
{
    [FirestoreProperty]
    public int ItemType { get; set; }

    [FirestoreProperty]
    public int Quantity { get; set; }

    [FirestoreProperty]
    public string ItemId { get; set; }

    [FirestoreProperty]
    public Timestamp AcquiredTime { get; set; }

    public InventoryItemDto() { }

    public InventoryItemDto(InventoryItem item)
    {
        ItemType = (int)item.ItemType;
        Quantity = item.Quantity;
        ItemId = item.ItemId;
        AcquiredTime = Timestamp.FromDateTime(item.AcquiredTime.ToUniversalTime());
    }

    public InventoryItem ToInventoryItem()
    {
        return new InventoryItem(
            (EItemType)ItemType,
            Quantity,
            ItemId,
            AcquiredTime.ToDateTime().ToLocalTime()
        );
    }
}