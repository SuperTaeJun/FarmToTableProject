using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "Item Database", menuName = "Inventory/Item Database")]
public class SO_ItemDatabase : ScriptableObject
{
    [SerializeField] private List<SO_ItemData> itemDataList = new List<SO_ItemData>();

    private Dictionary<EItemType, SO_ItemData> itemDataDictionary;
    // 스프라이트 캐시만 유지
    private Dictionary<EItemType, Sprite> iconCache = new Dictionary<EItemType, Sprite>();

    private void OnEnable()
    {
        BuildDictionary();
    }

    private void BuildDictionary()
    {
        itemDataDictionary = new Dictionary<EItemType, SO_ItemData>();

        foreach (var itemData in itemDataList)
        {
            if (itemData != null && !itemDataDictionary.ContainsKey(itemData.Type))
            {
                itemDataDictionary[itemData.Type] = itemData;
            }
        }
    }

    public SO_ItemData GetItemData(EItemType itemType)
    {
        if (itemDataDictionary == null)
            BuildDictionary();

        return itemDataDictionary.TryGetValue(itemType, out SO_ItemData data) ? data : null;
    }

    // 캐시된 아이콘 반환 (동기)
    public Sprite GetItemIcon(EItemType itemType)
    {
        if (iconCache.TryGetValue(itemType, out Sprite cachedIcon))
        {
            return cachedIcon;
        }
        return null;
    }

    // 아이콘 로드 및 캐싱 (비동기)
    public async Task<Sprite> LoadItemIconAsync(EItemType itemType)
    {
        // 이미 캐시에 있으면 반환
        if (iconCache.TryGetValue(itemType, out Sprite cachedIcon))
        {
            return cachedIcon;
        }

        var itemData = GetItemData(itemType);
        if (itemData?.Icon != null && itemData.Icon.RuntimeKeyIsValid())
        {
            try
            {
                var sprite = await itemData.Icon.LoadAssetAsync<Sprite>().Task;
                iconCache[itemType] = sprite;
                return sprite;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"아이콘 로드 실패: {itemType} - {e.Message}");
            }
        }

        return null;
    }

    // 자주 사용되는 아이템들의 아이콘 미리 로드
    public async Task PreloadCommonIcons()
    {
        List<Task> loadTasks = new List<Task>();

        foreach (var itemData in itemDataList)
        {
            if (itemData?.Icon != null && itemData.Icon.RuntimeKeyIsValid())
            {
                loadTasks.Add(LoadItemIconAsync(itemData.Type));
            }
        }

        await Task.WhenAll(loadTasks);
        Debug.Log($"아이템 아이콘 로드 완료: {iconCache.Count}개");
    }

    // InventoryManager에서 사용하는 기본 정보들
    public string GetItemName(EItemType itemType)
    {
        var itemData = GetItemData(itemType);
        return itemData?.GetDisplayName() ?? itemType.ToString();
    }

    public string GetItemDescription(EItemType itemType)
    {
        var itemData = GetItemData(itemType);
        return itemData?.Description ?? string.Empty;
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

    public List<SO_ItemData> GetAllItems()
    {
        return itemDataList.ToList();
    }

    // 메모리 해제
    private void OnDestroy()
    {
        iconCache.Clear();
    }

#if UNITY_EDITOR
    [ContextMenu("Auto Generate Missing Items")]
    private void AutoGenerateMissingItems()
    {
        var existingTypes = itemDataList.Where(item => item != null).Select(item => item.Type).ToHashSet();
        var allTypes = System.Enum.GetValues(typeof(EItemType)).Cast<EItemType>();

        foreach (var itemType in allTypes)
        {
            if (!existingTypes.Contains(itemType))
            {
                var newItemData = CreateInstance<SO_ItemData>();
                newItemData.Type = itemType;
                newItemData.ItemName = itemType.ToString();
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