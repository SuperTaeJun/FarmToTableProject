using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public EItemType itemType;
    public string itemName;
    [TextArea(3, 5)]
    public string description;
    
    [Header("Visual")]
    public Sprite icon;
    public Sprite previewImage;
    
    [Header("Properties")]
    public int maxStackSize = 99;
    public bool isStackable = true;
    public float weight = 1f;
    
    [Header("Value")]
    public int sellPrice = 0;
    public int buyPrice = 0;
    
    public string GetDisplayName()
    {
        return string.IsNullOrEmpty(itemName) ? itemType.ToString() : itemName;
    }
}