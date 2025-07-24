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
    private Dictionary<string, GameObject> buildingGameObjects = new Dictionary<string, GameObject>(); // ���ӿ�����Ʈ ĳ��

    [SerializeField] private float gridSize = 1f;
    [SerializeField] private SO_Building[] buildingData; // SO_Building ������

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
        Debug.Log($"�ε�� ûũ ��: {loadedChunks?.Count() ?? 0}");

        if (loadedChunks == null || !loadedChunks.Any())
        {
            Debug.Log("�ε�� ûũ�� �����ϴ�.");
            return;
        }

        foreach (var chunkPos in loadedChunks)
        {
            string chunkId = chunkPos.ToChunkId();
            Debug.Log($"ûũ �ε� �õ�: {chunkId}");

            try
            {
                var buildings = await LoadBuildingsForChunk(chunkId);
                Debug.Log($"ûũ {chunkId}���� {buildings.Count}�� �ǹ� �ε��");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ûũ {chunkId} �ε� ����: {e.Message}");
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

        // ���� ��ǥ�� ��ȯ
        ChunkPosition chunkPos = WorldManager.Instance.GetChunkPositionFromId(building.ChunkId);
        Vector3 worldPosition = WorldManager.Instance.GetWorldPositionFromChunkLocal(chunkPos, building.Position);

        // ���� ������Ʈ ����
        GameObject buildingObj = Instantiate(buildingData.Prefab, worldPosition, Quaternion.Euler(building.Rotation));
        buildingObj.name = $"{building.Type}_{building.GetBuildingId()}";

        return buildingObj;
    }

    public Vector3 SnapToGrid(Vector3 position, Vector2Int size)
    {
        // �ǹ��� �׻� ���� �׸��忡 ��ġ
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

        // �߾� �ǹ� �������� ��ġ�Ϸ��� ������ ������ ���
        float halfSizeX = size.x * 0.5f;
        float halfSizeZ = size.y * 0.5f;

        Vector3 startPosition = new Vector3(
            localPosition.x - halfSizeX + 0.5f,
            localPosition.y,
            localPosition.z - halfSizeZ + 0.5f
        );

        // ��ġ�Ϸ��� ���� üũ
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                var checkPos = startPosition + new Vector3(x * gridSize, 0, z * gridSize);
                if (IsPositionOccupied(buildings, checkPos))
                {
                    Debug.Log($"��ġ ���� - ��ġ ({checkPos.x}, {checkPos.z})�� ������");
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
            // �߾� �ǹ� �ǹ��� ���� ���� ���� ���
            Vector3 buildingCenter = building.Position;

            // �ǹ� ũ���� ���ݸ�ŭ �ڷ� �̵��Ͽ� ������ ���
            float halfSizeX = building.Size.x * 0.5f;
            float halfSizeZ = building.Size.y * 0.5f;

            Vector2 buildingStart = new Vector2(
                buildingCenter.x - halfSizeX + 0.5f,  // 0.5f�� �׸��� �߾� ����
                buildingCenter.z - halfSizeZ + 0.5f
            );

            // �ǹ��� �����ϴ� ��� �׸��� üũ
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
                        Debug.Log($"�浹! üũ:{checkGrid} vs ����:{occupiedGrid}");
                        Debug.Log($"�ǹ��߽�:{buildingCenter}, ������:{buildingStart}, ũ��:{building.Size}");
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

        // �̹� ������ ��ġ�� �����Ƿ� �߰� ���� ���ʿ�
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

        // �ε�� �ǹ����� ���� ������Ʈ ����
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
        // �ش� ûũ�� ���� ������Ʈ�� ����
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