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
    private Dictionary<string, GameObject> buildingGameObjects = new Dictionary<string, GameObject>(); // 건물 오브젝트 캐시

    [SerializeField] private float gridSize = 1f;
    [SerializeField] private SO_Building[] buildingData; // SO_Building 배열 데이터

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
        Debug.Log($"로딩된 청크 수: {loadedChunks?.Count() ?? 0}");

        if (loadedChunks == null || !loadedChunks.Any())
        {
            Debug.Log("로딩된 청크가 없습니다.");
            return;
        }

        foreach (var chunkPos in loadedChunks)
        {
            string chunkId = chunkPos.ToChunkId();
            Debug.Log($"청크 로딩 시도: {chunkId}");

            try
            {
                var buildings = await LoadBuildingsForChunk(chunkId);
                Debug.Log($"청크 {chunkId}에서 {buildings.Count}개의 건물 로딩됨");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"청크 {chunkId} 로딩 실패: {e.Message}");
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

        // 로컬 좌표를 월드 좌표로 변환
        ChunkPosition chunkPos = WorldManager.Instance.GetChunkPositionFromId(building.ChunkId);
        Vector3 worldPosition = WorldManager.Instance.GetWorldPositionFromChunkLocal(chunkPos, building.Position);

        // 건물 게임오브젝트 생성
        GameObject buildingObj = Instantiate(buildingData.Prefab, worldPosition, Quaternion.Euler(building.Rotation));
        buildingObj.name = $"{building.Type}_{building.GetBuildingId()}";

        return buildingObj;
    }

    public Vector3 SnapToGrid(Vector3 position, Vector2Int size)
    {
        // 건물은 반드시 정해진 그리드에 위치
        return new Vector3(
            Mathf.Round(position.x),
            position.y,
            Mathf.Round(position.z)
        );
    }

    public bool CanPlaceBuilding(string chunkId, Vector3 worldPosition, Vector2Int size)
    {
        Debug.Log($"건물 배치 확인 시작 - 크기: {size}, 위치: {worldPosition}");

        ChunkPosition chunkPos = WorldManager.Instance.GetChunkPositionFromId(chunkId);
        Vector3 localPosition = WorldManager.Instance.GetLocalPositionInChunk(worldPosition, chunkPos);
        var buildings = GetLoadedBuildings(chunkId);

        // 중심 좌표 기준으로 크기만큼 영역 계산
        float halfSizeX = size.x * 0.5f;
        float halfSizeZ = size.y * 0.5f;

        Vector3 startPosition = new Vector3(
            localPosition.x - halfSizeX + 0.5f,
            localPosition.y,
            localPosition.z - halfSizeZ + 0.5f
        );

        Debug.Log($"시작 위치: {startPosition}, 로컬 위치: {localPosition}");

        // 먼저 건물 크기만큼의 영역이 평지인지 확인
        if (!IsAreaFlat(chunkPos, startPosition, size))
        {
            Debug.Log("배치 실패 - 건물 영역이 평지가 아님");
            return false;
        }

        // 배치하려는 영역의 건물 충돌 체크
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                var checkPos = startPosition + new Vector3(x * gridSize, 0, z * gridSize);

                // 다른 건물과의 충돌 확인
                if (IsPositionOccupied(buildings, checkPos))
                {
                    Debug.Log($"배치 실패 - 위치 ({checkPos.x}, {checkPos.z})가 점유됨");
                    return false;
                }
            }
        }

        Debug.Log("건물 배치 가능!");
        return true;
    }

    private bool IsAreaFlat(ChunkPosition chunkPos, Vector3 startPosition, Vector2Int size)
    {
        float baseHeight = -999f;
        bool baseHeightSet = false;

        // 건물 크기만큼의 모든 그리드 높이 확인
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                var checkPos = startPosition + new Vector3(x * gridSize, 0, z * gridSize);
                Vector3 worldCheckPos = WorldManager.Instance.GetWorldPositionFromChunkLocal(chunkPos, checkPos);
                float height = WorldManager.Instance.GetGroundHeight(worldCheckPos);

                if (height < -100f) return false;

                if (!baseHeightSet)
                {
                    baseHeight = height;
                    baseHeightSet = true;
                }
                else if (Mathf.Abs(height - baseHeight) > 0.25f)
                {
                    Debug.Log($"높이 차이 발견: 기준({baseHeight}) vs 현재({height}) = {Mathf.Abs(height - baseHeight)}");
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
            // 중심 좌표 기준으로 영역 계산
            Vector3 buildingCenter = building.Position;
            float halfSizeX = building.Size.x * 0.5f;
            float halfSizeZ = building.Size.y * 0.5f;

            Vector2 buildingStart = new Vector2(
                buildingCenter.x - halfSizeX + 0.5f,
                buildingCenter.z - halfSizeZ + 0.5f
            );

            for (int x = 0; x < building.Size.x; x++)
            {
                for (int z = 0; z < building.Size.y; z++)
                {
                    Vector2Int occupiedGrid = new Vector2Int(
                        Mathf.RoundToInt(buildingStart.x + x),
                        Mathf.RoundToInt(buildingStart.y + z)
                    );

                    if (occupiedGrid == checkGrid)
                    {
                        Debug.Log($"충돌! 체크:{checkGrid} vs 점유:{occupiedGrid}");
                        Debug.Log($"건물중심:{buildingCenter}, 시작위치:{buildingStart}, 크기:{building.Size}");
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

        // 로딩된 건물에 대해 게임오브젝트 생성
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
        // 해당 청크의 건물 오브젝트 삭제
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

    public Vector2Int? GetBuildingSizeAtPosition(Vector3 worldPosition, float searchRadius = 0.5f)
    {
        foreach (var chunkBuildings in loadedBuildings.Values)
        {
            foreach (var building in chunkBuildings)
            {
                ChunkPosition chunkPos = WorldManager.Instance.GetChunkPositionFromId(building.ChunkId);
                Vector3 buildingWorldPos = WorldManager.Instance.GetWorldPositionFromChunkLocal(chunkPos, building.Position);

                Vector3 flatWorldPos = new Vector3(worldPosition.x, 0, worldPosition.z);
                Vector3 flatBuildingPos = new Vector3(buildingWorldPos.x, 0, buildingWorldPos.z);

                if (Vector3.Distance(flatWorldPos, flatBuildingPos) <= searchRadius)
                {
                    SO_Building buildingInfo = GetBuildingInfo(building.Type);
                    if (buildingInfo != null)
                    {
                        return buildingInfo.Size;
                    }
                }
            }
        }

        return null;
    }
}
