using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("인벤토리 설정")]
    [SerializeField] private int _maxSlots = 30;
    [SerializeField] private ItemDatabase _itemDatabase;

    public static InventoryManager Instance;
    private InventoryRepository _repo;
    private Dictionary<EInventoryType, List<InventoryItem>> _inventories = new Dictionary<EInventoryType, List<InventoryItem>>();
    private EInventoryType _currentInventoryType = EInventoryType.Player;

    public List<InventoryItem> Items => _inventories.ContainsKey(_currentInventoryType) ? _inventories[_currentInventoryType] : new List<InventoryItem>();
    public int MaxSlots => _maxSlots;
    public int UsedSlots => Items.Count;
    public int AvailableSlots => _maxSlots - Items.Count;
    public EInventoryType CurrentInventoryType => _currentInventoryType;

    // 인벤토리 이벤트들
    public DebugEvent<InventoryItem> OnItemAdded = new DebugEvent<InventoryItem>();
    public DebugEvent<InventoryItem> OnItemRemoved = new DebugEvent<InventoryItem>();
    public DebugEvent<InventoryItem> OnItemQuantityChanged = new DebugEvent<InventoryItem>();
    public DebugEvent OnInventoryChanged = new DebugEvent();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        _repo = new InventoryRepository();
        
        // 인벤토리 딕셔너리 초기화
        _inventories[EInventoryType.Player] = new List<InventoryItem>();
        _inventories[EInventoryType.Vehicle] = new List<InventoryItem>();
        
        // ItemDatabase 초기화
        if (_itemDatabase != null)
        {
            InventoryItem.SetItemDatabase(_itemDatabase);
        }
        else
        {
            Debug.LogWarning("ItemDatabase가 설정되지 않았습니다. Inspector에서 ItemDatabase를 할당해주세요.");
        }
    }

    private async void Start()
    {
        await LoadInventory();
    }

    public void SwitchInventory(EInventoryType inventoryType)
    {
        _currentInventoryType = inventoryType;
        OnInventoryChanged.Invoke();
        Debug.Log($"인벤토리 전환: {inventoryType}");
    }

    public List<InventoryItem> GetInventoryItems(EInventoryType inventoryType)
    {
        return _inventories.ContainsKey(inventoryType) ? _inventories[inventoryType] : new List<InventoryItem>();
    }

    public async Task LoadInventory()
    {
        try
        {
            var playerItems = await _repo.LoadInventory();
            _inventories[EInventoryType.Player] = playerItems;
            OnInventoryChanged.Invoke();
            Debug.Log($"플레이어 인벤토리 로드 완료: {playerItems.Count}개 아이템");
        }
        catch (Exception e)
        {
            Debug.LogError($"인벤토리 로드 실패: {e.Message}");
            _inventories[EInventoryType.Player] = new List<InventoryItem>();
        }
    }

    public async Task SaveInventory()
    {
        try
        {
            await _repo.SaveInventory(_inventories[EInventoryType.Player]);
            Debug.Log("플레이어 인벤토리 저장 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"인벤토리 저장 실패: {e.Message}");
        }
    }

    public async Task<bool> TryAddItem(EItemType itemType, int quantity = 1, EInventoryType inventoryType = EInventoryType.Player)
    {
        if (quantity <= 0) return false;

        var targetInventory = _inventories[inventoryType];
        
        // 스택 가능한 아이템이 있는지 확인
        var stackableItem = targetInventory.FirstOrDefault(item => item.ItemType == itemType);
        
        if (stackableItem != null)
        {
            // 기존 아이템에 수량 추가
            stackableItem.AddQuantity(quantity);
            if (inventoryType == EInventoryType.Player)
                await _repo.UpdateItemQuantity(stackableItem.ItemId, stackableItem.Quantity);
            OnItemQuantityChanged.Invoke(stackableItem);
            OnInventoryChanged.Invoke();
            return true;
        }
        else
        {
            // 새 슬롯이 필요한 경우
            if (targetInventory.Count >= _maxSlots)
            {
                Debug.LogWarning($"{inventoryType} 인벤토리가 가득 참");
                return false;
            }

            var newItem = new InventoryItem(itemType, quantity);
            targetInventory.Add(newItem);
            if (inventoryType == EInventoryType.Player)
                await _repo.AddItem(newItem);
            OnItemAdded.Invoke(newItem);
            OnInventoryChanged.Invoke();
            return true;
        }
    }
    
    public async Task<bool> TryAddItem(EItemType itemType, int quantity = 1)
    {
        return await TryAddItem(itemType, quantity, _currentInventoryType);
    }

    public async Task<bool> TryRemoveItem(EItemType itemType, int quantity = 1, EInventoryType inventoryType = EInventoryType.Player)
    {
        if (quantity <= 0) return false;

        var targetInventory = _inventories[inventoryType];
        var item = targetInventory.FirstOrDefault(i => i.ItemType == itemType);
        if (item == null || item.Quantity < quantity)
        {
            Debug.LogWarning($"{inventoryType}에서 제거할 아이템이 부족함: {itemType}, 요청량: {quantity}");
            return false;
        }

        if (item.TryRemoveQuantity(quantity))
        {
            if (item.IsEmpty())
            {
                targetInventory.Remove(item);
                if (inventoryType == EInventoryType.Player)
                    await _repo.RemoveItem(item.ItemId);
                OnItemRemoved.Invoke(item);
            }
            else
            {
                if (inventoryType == EInventoryType.Player)
                    await _repo.UpdateItemQuantity(item.ItemId, item.Quantity);
                OnItemQuantityChanged.Invoke(item);
            }
            
            OnInventoryChanged.Invoke();
            return true;
        }

        return false;
    }
    
    public async Task<bool> TryRemoveItem(EItemType itemType, int quantity = 1)
    {
        return await TryRemoveItem(itemType, quantity, _currentInventoryType);
    }

    public async Task<bool> TryRemoveItemById(string itemId, int quantity = 1)
    {
        var item = Items.FirstOrDefault(i => i.ItemId == itemId);
        if (item == null) return false;

        return await TryRemoveItem(item.ItemType, quantity);
    }

    public int GetItemCount(EItemType itemType, EInventoryType inventoryType = EInventoryType.Player)
    {
        var targetInventory = _inventories[inventoryType];
        var item = targetInventory.FirstOrDefault(i => i.ItemType == itemType);
        return item?.Quantity ?? 0;
    }

    public bool HasItem(EItemType itemType, int quantity = 1, EInventoryType inventoryType = EInventoryType.Player)
    {
        return GetItemCount(itemType, inventoryType) >= quantity;
    }

    public List<InventoryItem> GetItemsByType(EItemType itemType, EInventoryType inventoryType = EInventoryType.Player)
    {
        var targetInventory = _inventories[inventoryType];
        return targetInventory.Where(item => item.ItemType == itemType).ToList();
    }

    public InventoryItem GetItemById(string itemId)
    {
        return Items.FirstOrDefault(item => item.ItemId == itemId);
    }

    public bool IsFull(EInventoryType inventoryType = EInventoryType.Player)
    {
        var targetInventory = _inventories[inventoryType];
        return targetInventory.Count >= _maxSlots;
    }

    public bool CanAddItem(EItemType itemType, int quantity = 1, EInventoryType inventoryType = EInventoryType.Player)
    {
        var targetInventory = _inventories[inventoryType];
        
        // 스택 가능한 아이템이 있으면 추가 가능
        var stackableItem = targetInventory.FirstOrDefault(item => item.ItemType == itemType);
        if (stackableItem != null) return true;

        // 새 슬롯이 필요한 경우 여유 공간 확인
        return targetInventory.Count < _maxSlots;
    }

    // 외부에서 호출할 수 있는 편의 메서드들
    public async Task AddCropToInventory(ECropType cropType, int quantity = 1, EInventoryType inventoryType = EInventoryType.Player)
    {
        EItemType itemType = ConvertCropToItem(cropType);
        await TryAddItem(itemType, quantity, inventoryType);
    }

    public async Task AddSeedToInventory(ECropType cropType, int quantity = 1, EInventoryType inventoryType = EInventoryType.Player)
    {
        EItemType seedType = ConvertCropToSeed(cropType);
        await TryAddItem(seedType, quantity, inventoryType);
    }

    // 차량 전용 편의 메서드들
    public async Task AddCropToVehicle(ECropType cropType, int quantity = 1)
    {
        await AddCropToInventory(cropType, quantity, EInventoryType.Vehicle);
    }

    public async Task AddSeedToVehicle(ECropType cropType, int quantity = 1)
    {
        await AddSeedToInventory(cropType, quantity, EInventoryType.Vehicle);
    }

    public bool HasSeed(ECropType cropType, int quantity = 1)
    {
        EItemType seedType = ConvertCropToSeed(cropType);
        return HasItem(seedType, quantity);
    }

    public async Task<bool> TryUseSeed(ECropType cropType, int quantity = 1)
    {
        EItemType seedType = ConvertCropToSeed(cropType);
        return await TryRemoveItem(seedType, quantity);
    }

    private EItemType ConvertCropToItem(ECropType cropType)
    {
        return cropType switch
        {
            ECropType.Carrot => EItemType.Carrot,
            ECropType.Beet => EItemType.Beet,
            ECropType.Bean => EItemType.Bean,
            ECropType.Broccoli => EItemType.Broccoli,
            ECropType.Chilli => EItemType.Chilli,
            ECropType.Cucumber => EItemType.Cucumber,
            ECropType.Eggplaint => EItemType.Eggplaint,
            ECropType.Pumkin => EItemType.Pumkin,
            ECropType.Corn => EItemType.Corn,
            ECropType.Watermelon => EItemType.Watermelon,
            ECropType.Onion => EItemType.Onion,
            ECropType.Pepper => EItemType.Pepper,
            ECropType.Asparagus => EItemType.Asparagus,
            _ => throw new ArgumentException($"Unknown crop type: {cropType}")
        };
    }

    private EItemType ConvertCropToSeed(ECropType cropType)
    {
        return cropType switch
        {
            ECropType.Carrot => EItemType.CarrotSeed,
            ECropType.Beet => EItemType.BeetSeed,
            ECropType.Bean => EItemType.BeanSeed,
            ECropType.Broccoli => EItemType.BroccoliSeed,
            ECropType.Chilli => EItemType.ChilliSeed,
            ECropType.Cucumber => EItemType.CucumberSeed,
            ECropType.Eggplaint => EItemType.EggplaintSeed,
            ECropType.Pumkin => EItemType.PumkinSeed,
            ECropType.Corn => EItemType.CornSeed,
            ECropType.Watermelon => EItemType.WatermelonSeed,
            ECropType.Onion => EItemType.OnionSeed,
            ECropType.Pepper => EItemType.PepperSeed,
            ECropType.Asparagus => EItemType.AsparagusSeed,
            _ => throw new ArgumentException($"Unknown crop type: {cropType}")
        };
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            _ = SaveInventory(); // 게임 종료 시 저장
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && Instance == this)
        {
            _ = SaveInventory(); // 앱 일시정지 시 저장
        }
    }
}