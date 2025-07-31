using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    public static VehicleManager Instance { get; private set; }
    
    [Header("Vehicle Prefabs")]
    [SerializeField] private GameObject _truckPrefab;
    [SerializeField] private GameObject _tractorPrefab;
    [SerializeField] private GameObject _bikePrefab;
    [SerializeField] private GameObject _bike2Prefab;

    private Dictionary<EVehicleType, GameObject> vehiclePrefabs;
    private List<Vehicle> activeVehicles = new List<Vehicle>();
    
    // 해금 시스템
    private Dictionary<EVehicleType, bool> unlockedVehicles = new Dictionary<EVehicleType, bool>();
    public DebugEvent<EVehicleType> OnVehicleUnlocked = new DebugEvent<EVehicleType>();
    
    // 아이템과 차량 타입 매핑
    public Dictionary<EItemType, EVehicleType> ItemToVehicleMap = new Dictionary<EItemType, EVehicleType>
    {
        { EItemType.Bike1, EVehicleType.Bike1 },
        { EItemType.Bike2, EVehicleType.Bike2 },
        { EItemType.Tractor, EVehicleType.Tractor },
        { EItemType.Sedan, EVehicleType.Truck }
    };
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeVehiclePrefabs();
            InitializeUnlockSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeUnlockSystem()
    {
        // 모든 차량을 잠금 상태로 시작
        foreach (EVehicleType vehicleType in System.Enum.GetValues(typeof(EVehicleType)))
        {
            if (vehicleType != EVehicleType.None)
            {
                unlockedVehicles[vehicleType] = false;
            }
        }
    }
    
    private void InitializeVehiclePrefabs()
    {
        vehiclePrefabs = new Dictionary<EVehicleType, GameObject>();
        
        if (_truckPrefab != null)
            vehiclePrefabs[EVehicleType.Truck] = _truckPrefab;
        if (_tractorPrefab != null)
            vehiclePrefabs[EVehicleType.Tractor] = _tractorPrefab;
        if(_bikePrefab != null)
            vehiclePrefabs[EVehicleType.Bike1] = _bikePrefab;

        vehiclePrefabs[EVehicleType.Bike2] = _bike2Prefab;
    }

    public Vehicle SpawnVehicle(EVehicleType vehicleType, Vector3 position, Player player, Quaternion rotation)
    {
        if (!vehiclePrefabs.ContainsKey(vehicleType))
        {
            Debug.LogError($"차량 프리팹을 찾을 수 없음: {vehicleType}");
            return null;
        }
        
        // 해금 체크
        if (!IsVehicleUnlocked(vehicleType))
        {
            Debug.LogWarning($"{vehicleType} 차량이 해금되지 않았습니다.");
            return null;
        }

        // 기존에 타고 있는 차량이 있다면 하차
        if (player.ModeController.CurrentMode == EPlayerMode.Vehicle)
        {
            Vehicle currentVehicle = GetPlayerVehicle(player);
            if (currentVehicle != null)
            {
                currentVehicle.DismountPlayer();
            }
        }

        GameObject vehicleObj = Instantiate(vehiclePrefabs[vehicleType], position, rotation);
        Vehicle vehicle = vehicleObj.GetComponent<Vehicle>();

        if (vehicle == null)
        {
            Debug.LogError($"Vehicle 컴포넌트를 찾을 수 없음: {vehicleType}");
            Destroy(vehicleObj);
            return null;
        }

        activeVehicles.Add(vehicle);

        player.GetAbility<PlayerVehicleAbility>().MountVehicle(vehicle);

        Debug.Log($"{vehicleType} 차량이 생성되어 플레이어가 탑승했습니다.");
        return vehicle;
    }

    public void DestroyVehicle(Vehicle vehicle)
    {
        if (vehicle == null) return;
        
        activeVehicles.Remove(vehicle);
        Destroy(vehicle.gameObject);
        
        Debug.Log($"{vehicle.VehicleType} 차량이 제거되었습니다.");
    }
    
    public Vehicle GetPlayerVehicle(Player player)
    {
        foreach (Vehicle vehicle in activeVehicles)
        {
            if (vehicle.IsOccupied && vehicle.CurrentDriver == player)
            {
                return vehicle;
            }
        }
        return null;
    }
    
    private Vector3 SnapToGrid(Vector3 position)
    {
        Vector3 blockOffset = ChunkGenerator.Instance.blockOffset;
        
        float snappedX = Mathf.Round(position.x / blockOffset.x) * blockOffset.x;
        float snappedZ = Mathf.Round(position.z / blockOffset.z) * blockOffset.z;
        
        // 지면 높이 가져오기
        float groundHeight = WorldManager.Instance.GetGroundHeight(new Vector3(snappedX, position.y, snappedZ));
        
        return new Vector3(snappedX, groundHeight + blockOffset.y, snappedZ);
    }
    
    public void SetVehiclePrefabs(GameObject truck, GameObject tractor)
    {
        _truckPrefab = truck;
        _tractorPrefab = tractor;
        InitializeVehiclePrefabs();
    }
    
    public List<Vehicle> GetActiveVehicles()
    {
        return new List<Vehicle>(activeVehicles);
    }
    
    public int GetActiveVehicleCount()
    {
        return activeVehicles.Count;
    }
    
    // 해금 시스템 메서드들
    public bool IsVehicleUnlocked(EVehicleType vehicleType)
    {
        return unlockedVehicles.ContainsKey(vehicleType) && unlockedVehicles[vehicleType];
    }
    
    public void UnlockVehicle(EVehicleType vehicleType)
    {
        if (!unlockedVehicles.ContainsKey(vehicleType) || !unlockedVehicles[vehicleType])
        {
            unlockedVehicles[vehicleType] = true;
            OnVehicleUnlocked.Invoke(vehicleType);
            Debug.Log($"{vehicleType} 차량이 해금되었습니다!");
        }
    }
    
    public void UnlockVehicleFromItem(EItemType itemType)
    {
        EVehicleType vehicleType = GetVehicleTypeFromItem(itemType);
        if (vehicleType != EVehicleType.None)
        {
            UnlockVehicle(vehicleType);
        }
    }
    
    private EVehicleType GetVehicleTypeFromItem(EItemType itemType)
    {
        return ItemToVehicleMap.TryGetValue(itemType, out EVehicleType vehicleType) ? vehicleType : EVehicleType.None;
    }
    
    public List<EVehicleType> GetUnlockedVehicles()
    {
        var result = new List<EVehicleType>();
        foreach (var kvp in unlockedVehicles)
        {
            if (kvp.Value)
                result.Add(kvp.Key);
        }
        return result;
    }
}