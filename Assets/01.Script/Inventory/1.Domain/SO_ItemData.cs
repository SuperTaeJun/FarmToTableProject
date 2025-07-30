using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventory/Item Data")]
public class SO_ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public EItemType Type;
    public string ItemName;
    [TextArea(3, 5)]
    public string Description;

    [Header("Visual")]
    public AssetReference Icon;           

    [Header("Properties")]
    public int maxStackSize = 99;
    public bool isStackable = true;

    public string GetDisplayName()
    {
        return string.IsNullOrEmpty(ItemName) ? Type.ToString() : ItemName;
    }
}