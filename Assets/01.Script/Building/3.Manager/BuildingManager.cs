using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

    private async void Start()
    {
        await LoadAllBuilding();
    }

    public async Task LoadAllBuilding()
    {
        var loadedChunks = WorldManager.Instance.LoadedChunkPositions;
        Debug.Log($"로드된 청크 수: {loadedChunks?.Count() ?? 0}");

        if (loadedChunks == null || !loadedChunks.Any())
        {
            Debug.Log("로드된 청크가 없습니다.");
            return;
        }

        foreach (var chunkPos in loadedChunks)
        {
            string chunkId = chunkPos.ToChunkId();
            Debug.Log($"청크 로드 시도: {chunkId}");

            try
            {
                var buildings = await LoadBuildingsForChunk(chunkId);
                Debug.Log($"청크 {chunkId}에서 {buildings.Count}개 건물 로드됨");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"청크 {chunkId} 로드 실패: {e.Message}");
            }
        }
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

    public Vector3 SnapToGrid(Vector3 position, Vector2Int size)
    {
        // 피벗을 항상 정수 그리드에 배치
        return new Vector3(
            Mathf.Round(position.x),
            position.y,
            Mathf.Round(position.z)
        );
    }

    public bool CanPlaceBuilding(string chunkId, Vector3 worldPosition, Vector2Int size)
    {
        ChunkPosition chunkPos = WorldManager.Instance.GetChunkPositionFromId(chunkId);
        Vector3 localPosition = WorldManager.Instance.GetLocalPositionInChunk(worldPosition, chunkPos);
        var buildings = GetLoadedBuildings(chunkId);

        // 중앙 피벗 기준으로 배치하려는 영역의 시작점 계산
        float halfSizeX = size.x * 0.5f;
        float halfSizeZ = size.y * 0.5f;

        Vector3 startPosition = new Vector3(
            localPosition.x - halfSizeX + 0.5f,
            localPosition.y,
            localPosition.z - halfSizeZ + 0.5f
        );

        // 배치하려는 영역 체크
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                var checkPos = startPosition + new Vector3(x * gridSize, 0, z * gridSize);
                if (IsPositionOccupied(buildings, checkPos))
                {
                    Debug.Log($"배치 실패 - 위치 ({checkPos.x}, {checkPos.z})가 점유됨");
                    return false;
                }
            }
        }
        return true;
    }

    private bool IsPositionOccupied(List<Building> buildings, Vector3 position)
    {
        Vector2Int checkGrid = new Vector2Int(
          Mathf.RoundToInt(position.x),
          Mathf.RoundToInt(position.z)
      );

        foreach (var building in buildings)
        {
            // 중앙 피벗 건물의 실제 점유 영역 계산
            Vector3 buildingCenter = building.Position;

            // 건물 크기의 절반만큼 뒤로 이동하여 시작점 계산
            float halfSizeX = building.Size.x * 0.5f;
            float halfSizeZ = building.Size.y * 0.5f;

            Vector2 buildingStart = new Vector2(
                buildingCenter.x - halfSizeX + 0.5f,  // 0.5f는 그리드 중앙 보정
                buildingCenter.z - halfSizeZ + 0.5f
            );

            // 건물이 차지하는 모든 그리드 체크
            for (int x = 0; x < building.Size.x; x++)
            {
                for (int z = 0; z < building.Size.y; z++)
                {
                    Vector2Int occupiedGrid = new Vector2Int(
                        Mathf.RoundToInt(buildingStart.x + x),
                        Mathf.RoundToInt(buildingStart.y + z)
                    );

                    if (occupiedGrid.x == checkGrid.x && occupiedGrid.y == checkGrid.y)
                    {
                        Debug.Log($"충돌! 체크:{checkGrid} vs 점유:{occupiedGrid}");
                        Debug.Log($"건물중심:{buildingCenter}, 시작점:{buildingStart}, 크기:{building.Size}");
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

        // 이미 스냅된 위치가 들어오므로 추가 스냅 불필요
        if (!CanPlaceBuilding(chunkId, worldPosition, buildingData.Size))
        {
            return false;
        }

        var building = new Building(type, chunkId, localPosition, rotation, buildingData.Size);
        await AddBuilding(building);

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