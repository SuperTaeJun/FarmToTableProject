using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance;

    private BuildingRepository _buildingRepository;
    private Dictionary<string, List<Building>> loadedBuildings = new Dictionary<string, List<Building>>();
    private Dictionary<string, GameObject> buildingGameObjects = new Dictionary<string, GameObject>(); // 게임오브젝트 캐시

    [SerializeField] private float gridSize = 1f;
    [SerializeField] private SO_Building[] buildingData; // SO_Building 데이터

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
        _buildingRepository = new BuildingRepository();
    }

    private SO_Building GetBuildingData(EBuildingType type)
    {
        foreach (var data in buildingData)
        {
            if (data.Type == type)
            {
                return data;
            }
        }
        return null;
    }

    private GameObject CreateBuildingGameObject(Building building)
    {
        var buildingData = GetBuildingData(building.Type);
        if (buildingData == null || buildingData.Prefab == null)
        {
            Debug.LogError($"Building data not found for type: {building.Type}");
            return null;
        }

        // 월드 좌표로 변환
        ChunkPosition chunkPos = WorldManager.Instance.GetChunkPositionFromId(building.ChunkId);
        Vector3 worldPosition = WorldManager.Instance.GetWorldPositionFromChunkLocal(chunkPos, building.Position);

        // 게임 오브젝트 생성
        GameObject buildingObj = Instantiate(buildingData.Prefab, worldPosition, Quaternion.Euler(building.Rotation));
        buildingObj.name = $"{building.Type}_{building.GetBuildingId()}";

        return buildingObj;
    }

    private void DestroyBuildingGameObject(string buildingId)
    {
        if (buildingGameObjects.ContainsKey(buildingId))
        {
            Destroy(buildingGameObjects[buildingId]);
            buildingGameObjects.Remove(buildingId);
        }
    }

    public Vector3 SnapToGrid(Vector3 position, Vector2Int size)
    {
        // 건물의 중심점을 기준으로 스냅
        float centerOffsetX = (size.x - 1) * 0.5f;
        float centerOffsetZ = (size.y - 1) * 0.5f;

        // 중심점을 그리드에 스냅
        Vector3 centerPos = position + new Vector3(centerOffsetX, 0, centerOffsetZ);
        Vector3 snappedCenter = new Vector3(
            Mathf.Round(centerPos.x),
            position.y,
            Mathf.Round(centerPos.z)
        );

        // 피벗 위치로 다시 변환
        return snappedCenter - new Vector3(centerOffsetX, 0, centerOffsetZ);
    }

    public bool CanPlaceBuilding(string chunkId, Vector3 worldPosition, Vector2Int size)
    {
        ChunkPosition chunkPos = WorldManager.Instance.GetChunkPositionFromId(chunkId);
        Vector3 localPosition = WorldManager.Instance.GetLocalPositionInChunk(worldPosition, chunkPos);
        var snappedPosition = SnapToGrid(localPosition, size);
        var buildings = GetLoadedBuildings(chunkId);

        // 배치하려는 영역 체크 (모두 로컬 좌표계에서 비교)
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                var checkPos = snappedPosition + new Vector3(x * gridSize, 0, z * gridSize);
                if (IsPositionOccupied(buildings, checkPos))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private bool IsPositionOccupied(List<Building> buildings, Vector3 position)
    {
        foreach (var building in buildings)
        {
            var buildingPos = building.Position;
            var buildingSize = building.Size;

            for (int x = 0; x < buildingSize.x; x++)
            {
                for (int z = 0; z < buildingSize.y; z++)
                {
                    var occupiedPos = buildingPos + new Vector3(x * gridSize, 0, z * gridSize);
                    if (Vector3.Distance(occupiedPos, position) < 0.1f)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public async Task<bool> TryPlaceBuilding(EBuildingType type, string chunkId, Vector3 worldPosition, Vector3 rotation)
    {
        var buildingData = GetBuildingData(type);
        if (buildingData == null)
        {
            Debug.LogError($"Building data not found for type: {type}");
            return false;
        }

        Vector3 localPosition = WorldManager.Instance.GetLocalPositionInChunk(worldPosition, WorldManager.Instance.GetChunkPositionFromId(chunkId));
        Vector3 snappedPosition = SnapToGrid(localPosition, buildingData.Size);

        if (!CanPlaceBuilding(chunkId, worldPosition, buildingData.Size))
        {
            return false;
        }

        var building = new Building(type, chunkId, snappedPosition, rotation, buildingData.Size);
        await AddBuilding(building);

        // 게임 오브젝트 생성
        GameObject buildingObj = CreateBuildingGameObject(building);
        if (buildingObj != null)
        {
            buildingGameObjects[building.GetBuildingId()] = buildingObj;
        }

        return true;
    }

    public async Task SaveBuildingsForChunk(string chunkId, List<Building> buildings)
    {
        await _buildingRepository.SaveBuildings(chunkId, buildings);
        loadedBuildings[chunkId] = new List<Building>(buildings);
    }

    public async Task<List<Building>> LoadBuildingsForChunk(string chunkId)
    {
        if (loadedBuildings.ContainsKey(chunkId))
        {
            return loadedBuildings[chunkId];
        }

        var buildings = await _buildingRepository.LoadBuildingByChunk(chunkId);
        loadedBuildings[chunkId] = buildings;

        // 로드된 건물들의 게임 오브젝트 생성
        foreach (var building in buildings)
        {
            GameObject buildingObj = CreateBuildingGameObject(building);
            if (buildingObj != null)
            {
                buildingGameObjects[building.GetBuildingId()] = buildingObj;
            }
        }

        return buildings;
    }

    public async Task AddBuilding(Building building)
    {
        await _buildingRepository.SaveSingleBuilding(building);

        if (!loadedBuildings.ContainsKey(building.ChunkId))
        {
            loadedBuildings[building.ChunkId] = new List<Building>();
        }
        loadedBuildings[building.ChunkId].Add(building);
    }

    public List<Building> GetLoadedBuildings(string chunkId)
    {
        return loadedBuildings.ContainsKey(chunkId) ? loadedBuildings[chunkId] : new List<Building>();
    }

    public void UnloadChunk(string chunkId)
    {
        // 해당 청크의 게임 오브젝트들 삭제
        var buildingsToRemove = new List<string>();
        foreach (var kvp in buildingGameObjects)
        {
            if (kvp.Key.StartsWith(chunkId))
            {
                Destroy(kvp.Value);
                buildingsToRemove.Add(kvp.Key);
            }
        }

        foreach (var buildingId in buildingsToRemove)
        {
            buildingGameObjects.Remove(buildingId);
        }

        loadedBuildings.Remove(chunkId);
    }

    public SO_Building GetBuildingInfo(EBuildingType type)
    {
        return GetBuildingData(type);
    }
}