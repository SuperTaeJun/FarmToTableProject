using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public EItemType ItemType { get; private set; }
    public int Quantity { get; private set; }
    public string ItemId { get; private set; }
    public DateTime AcquiredTime { get; private set; }
    
    private static ItemDatabase itemDatabase;
    
    public static void SetItemDatabase(ItemDatabase database)
    {
        itemDatabase = database;
    }

    public InventoryItem(EItemType itemType, int quantity = 1, string itemId = null)
    {
        ItemType = itemType;
        Quantity = quantity;
        ItemId = itemId ?? Guid.NewGuid().ToString();
        AcquiredTime = DateTime.Now;
    }

    public InventoryItem(EItemType itemType, int quantity, string itemId, DateTime acquiredTime)
    {
        ItemType = itemType;
        Quantity = quantity;
        ItemId = itemId;
        AcquiredTime = acquiredTime;
    }

    public bool CanStackWith(InventoryItem other)
    {
        return other != null && other.ItemType == ItemType;
    }

    public void AddQuantity(int amount)
    {
        if (amount > 0)
        {
            Quantity += amount;
        }
    }

    public bool TryRemoveQuantity(int amount)
    {
        if (amount <= 0 || amount > Quantity)
            return false;

        Quantity -= amount;
        return true;
    }

    public bool IsEmpty()
    {
        return Quantity <= 0;
    }
    
    public Sprite GetIcon()
    {
        return itemDatabase?.GetItemIcon(ItemType);
    }
    
    public string GetName()
    {
        return itemDatabase?.GetItemName(ItemType) ?? ItemType.ToString();
    }
    
    public string GetDescription()
    {
        return itemDatabase?.GetItemDescription(ItemType) ?? string.Empty;
    }
    
    public int GetMaxStackSize()
    {
        return itemDatabase?.GetMaxStackSize(ItemType) ?? 1;
    }
    
    public bool IsStackable()
    {
        return itemDatabase?.IsStackable(ItemType) ?? false;
    }
    
    public ItemData GetItemData()
    {
        return itemDatabase?.GetItemData(ItemType);
    }
}