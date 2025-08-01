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
    private Dictionary<string, GameObject> buildingGameObjects = new Dictionary<string, GameObject>(); // 빌딩 게임오브젝트 캐시

    [SerializeField] private SO_Building[] buildingData; // SO_Building 배열 데이터

    // 빌딩 타입별 프리팹 캐시
    private Dictionary<EBuildingType, GameObject> cachedPrefabs = new Dictionary<EBuildingType, GameObject>();
    private Dictionary<EBuildingType, GameObject> cachedPreviewPrefabs = new Dictionary<EBuildingType, GameObject>();

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

    private void Start()
    {
        // 프리팹 로딩은 LoadAllBuilding()에서 처리됨
    }

    private async Task LoadAllPrefabs()
    {
        Debug.Log("모든 Building 프리팹 로드 시작...");

        foreach (var data in buildingData)
        {
            // 메인 프리팹 로드
            if (data.Prefab != null)
            {
                var prefab = await data.Prefab.LoadAssetAsync<GameObject>().Task;
                cachedPrefabs[data.Type] = prefab;
            }

            // 프리뷰 프리팹 로드
            if (data.PreviewPrefab != null)
            {
                var previewPrefab = await data.PreviewPrefab.LoadAssetAsync<GameObject>().Task;
                cachedPreviewPrefabs[data.Type] = previewPrefab;
            }
        }

        Debug.Log($"프리팹 로드 완료: {cachedPrefabs.Count}개");
    }

    public async Task LoadAllBuilding()
    {
        await LoadAllPrefabs();

        var loadedChunks = WorldManager.Instance.LoadedChunkPositions;
        Debug.Log($"로딩할 청크 수: {loadedChunks?.Count() ?? 0}");

        if (loadedChunks == null || !loadedChunks.Any())
        {
            Debug.Log("로딩할 청크가 없습니다.");
            return;
        }

        foreach (var chunkPos in loadedChunks)
        {
            string chunkId = chunkPos.ToChunkId();
            Debug.Log($"청크 로딩 시도: {chunkId}");

            try
            {
                var buildings = await LoadBuildingsForChunk(chunkId);
                Debug.Log($"청크 {chunkId}에 {buildings.Count}개의 빌딩 로드됨");
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
        if (!cachedPrefabs.TryGetValue(building.Type, out GameObject prefab))
        {
            Debug.LogError($"프리팹을 찾을 수 없습니다: {building.Type}");
            return null;
        }

        ChunkPosition chunkPos = WorldManager.Instance.GetChunkPositionFromId(building.ChunkId);
        Vector3 worldPosition = WorldManager.Instance.GetWorldPositionFromChunkLocal(chunkPos, building.Position);

        GameObject buildingObj = Instantiate(prefab, worldPosition, Quaternion.Euler(building.Rotation));
        buildingObj.transform.SetParent(transform);
        buildingObj.name = $"{building.Type}_{building.GetBuildingId()}";

        return buildingObj;
    }

    public GameObject CreatePreviewInstance(EBuildingType type)
    {
        if (!cachedPreviewPrefabs.TryGetValue(type, out GameObject previewPrefab))
        {
            Debug.LogWarning($"프리뷰 프리팹이 없습니다: {type}");
            return null;
        }

        GameObject previewInstance = Instantiate(previewPrefab);
        return previewInstance;
    }

    public Vector3 SnapToGrid(Vector3 position, Vector2Int size)
    {
        // 빌딩을 격자 단위로 스냅
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

        // 기준 위치 계산
        float halfSizeX = size.x * ChunkGenerator.Instance.blockOffset.x * 0.5f;
        float halfSizeZ = size.y * ChunkGenerator.Instance.blockOffset.z * 0.5f;

        Vector3 startPosition = new Vector3(
            localPosition.x - halfSizeX + (ChunkGenerator.Instance.blockOffset.x * 0.5f),
            localPosition.y,
            localPosition.z - halfSizeZ + (ChunkGenerator.Instance.blockOffset.z * 0.5f)
        );


        if (!IsAreaFlat(chunkPos, startPosition, size))
        {
            return false;
        }

        // 건물 전체 영역을 박스로 한 번에 검사 (성능 최적화)
        if (IsAreaOccupied(chunkId, localPosition, size))
        {
            return false;
        }

        return true;
    }

    private bool IsAreaFlat(ChunkPosition chunkPos, Vector3 startPosition, Vector2Int size)
    {
        float baseHeight = -999f;
        bool baseHeightSet = false;

        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                var checkPos = startPosition + new Vector3(x * ChunkGenerator.Instance.blockOffset.x, 0, z * ChunkGenerator.Instance.blockOffset.z);
                Vector3 worldCheckPos = WorldManager.Instance.GetWorldPositionFromChunkLocal(chunkPos, checkPos);
                float height = WorldManager.Instance.GetGroundHeight(worldCheckPos);

                if (height < -100f) return false;

                if (!baseHeightSet)
                {
                    baseHeight = height;
                    baseHeightSet = true;
                }
                else if (Mathf.Abs(height - baseHeight) > (ChunkGenerator.Instance.blockOffset.y * 0.5f))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool IsAreaOccupied(string chunkId, Vector3 centerPosition, Vector2Int size)
    {
        ChunkPosition chunkPos = WorldManager.Instance.GetChunkPositionFromId(chunkId);
        Vector3 worldPosition = WorldManager.Instance.GetWorldPositionFromChunkLocal(chunkPos, centerPosition);
        
        Vector3 boxSize = new Vector3(
            size.x * ChunkGenerator.Instance.blockOffset.x,
            2f, // 높이는 고정값
            size.y * ChunkGenerator.Instance.blockOffset.z
        );
        
        Collider[] overlapping = Physics.OverlapBox(worldPosition, boxSize * 0.5f, Quaternion.identity);
        
        foreach (var collider in overlapping)
        {
            if (collider.GetComponent<BuildingObject>() != null)
            {
                return true;
            }
        }
        
        return false;
    }

    public async Task<bool> TryPlaceBuilding(EBuildingType type, string chunkId, Vector3 worldPosition, Vector3 rotation)
    {
        var buildingData = GetBuildingData(type);
        if (buildingData == null)
        {
            Debug.LogWarning($"{type}이 타입의 건물 데이터가 없습니다.");
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

        // 로드된 빌딩을 게임오브젝트로 생성
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
