using System.Collections.Generic;
using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    public static VehicleManager Instance { get; private set; }
    
    [Header("Vehicle Prefabs")]
    [SerializeField] private GameObject truckPrefab;
    [SerializeField] private GameObject tractorPrefab;
    
    private Dictionary<EVehicleType, GameObject> vehiclePrefabs;
    private List<Vehicle> activeVehicles = new List<Vehicle>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeVehiclePrefabs();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeVehiclePrefabs()
    {
        vehiclePrefabs = new Dictionary<EVehicleType, GameObject>();
        
        if (truckPrefab != null)
            vehiclePrefabs[EVehicleType.Truck] = truckPrefab;
        if (tractorPrefab != null)
            vehiclePrefabs[EVehicleType.Tractor] = tractorPrefab;
    }
    
    public Vehicle SpawnVehicle(EVehicleType vehicleType, Vector3 position, Player player)
    {
        if (!vehiclePrefabs.ContainsKey(vehicleType))
        {
            Debug.LogError($"차량 프리팹을 찾을 수 없음: {vehicleType}");
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
        
        // 플레이어 주변에 스냅된 위치에 생성
        Vector3 spawnPosition = SnapToGrid(position);
        
        GameObject vehicleObj = Instantiate(vehiclePrefabs[vehicleType], spawnPosition, Quaternion.identity);
        Vehicle vehicle = vehicleObj.GetComponent<Vehicle>();
        
        if (vehicle == null)
        {
            Debug.LogError($"Vehicle 컴포넌트를 찾을 수 없음: {vehicleType}");
            Destroy(vehicleObj);
            return null;
        }
        
        activeVehicles.Add(vehicle);
        
        // 플레이어를 즉시 탑승시킴
        vehicle.MountPlayer(player);
        
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
        truckPrefab = truck;
        tractorPrefab = tractor;
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
}