using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Item Database", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> itemDataList = new List<ItemData>();
    
    private Dictionary<EItemType, ItemData> itemDataDictionary;
    
    private void OnEnable()
    {
        BuildDictionary();
    }
    
    private void BuildDictionary()
    {
        itemDataDictionary = new Dictionary<EItemType, ItemData>();
        
        foreach (var itemData in itemDataList)
        {
            if (itemData != null && !itemDataDictionary.ContainsKey(itemData.itemType))
            {
                itemDataDictionary[itemData.itemType] = itemData;
            }
        }
    }
    
    public ItemData GetItemData(EItemType itemType)
    {
        if (itemDataDictionary == null)
            BuildDictionary();
            
        return itemDataDictionary.TryGetValue(itemType, out ItemData data) ? data : null;
    }
    
    public Sprite GetItemIcon(EItemType itemType)
    {
        var itemData = GetItemData(itemType);
        return itemData?.icon;
    }
    
    public string GetItemName(EItemType itemType)
    {
        var itemData = GetItemData(itemType);
        return itemData?.GetDisplayName() ?? itemType.ToString();
    }
    
    public string GetItemDescription(EItemType itemType)
    {
        var itemData = GetItemData(itemType);
        return itemData?.description ?? string.Empty;
    }
    
    public int GetMaxStackSize(EItemType itemType)
    {
        var itemData = GetItemData(itemType);
        return itemData?.maxStackSize ?? 1;
    }
    
    public bool IsStackable(EItemType itemType)
    {
        var itemData = GetItemData(itemType);
        return itemData?.isStackable ?? false;
    }
    
    public List<ItemData> GetAllItems()
    {
        return itemDataList.ToList();
    }
    
    public void AddItemData(ItemData itemData)
    {
        if (itemData != null && !itemDataList.Contains(itemData))
        {
            itemDataList.Add(itemData);
            BuildDictionary();
        }
    }
    
    public void RemoveItemData(ItemData itemData)
    {
        if (itemDataList.Contains(itemData))
        {
            itemDataList.Remove(itemData);
            BuildDictionary();
        }
    }
    
#if UNITY_EDITOR
    [ContextMenu("Auto Generate Missing Items")]
    private void AutoGenerateMissingItems()
    {
        var existingTypes = itemDataList.Where(item => item != null).Select(item => item.itemType).ToHashSet();
        var allTypes = System.Enum.GetValues(typeof(EItemType)).Cast<EItemType>();
        
        foreach (var itemType in allTypes)
        {
            if (!existingTypes.Contains(itemType))
            {
                var newItemData = CreateInstance<ItemData>();
                newItemData.itemType = itemType;
                newItemData.itemName = itemType.ToString();
                newItemData.name = $"ItemData_{itemType}";
                
                itemDataList.Add(newItemData);
                
                string assetPath = $"Assets/Resources/ItemData/ItemData_{itemType}.asset";
                UnityEditor.AssetDatabase.CreateAsset(newItemData, assetPath);
            }
        }
        
        BuildDictionary();
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
    }
#endif
}