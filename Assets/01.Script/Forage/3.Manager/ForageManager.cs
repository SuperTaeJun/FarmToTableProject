using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Unity.VisualScripting;

[System.Serializable]
public class ForageSpawnBatch
{
    [Header("타입 설정")]
    public EForageType forageType;
    
    [Header("가중치 및 개수")]
    [Range(0f, 100f)]
    public float weight = 10f;
    
    [Range(0, 20)]
    public int minCount = 1;
    
    [Range(0, 20)]
    public int maxCount = 5;
}
public class ForageManager : MonoBehaviour
{
    public static ForageManager Instance { private set; get; }

    private ForageRepository _repo;
    private List<ForageObject> _forages;
    public List<ForageObject> Forages => _forages;

    [Header("참조")]
    [SerializeField] private ForageObject _treePrefab;
    [SerializeField] private ForageObject _plant1Prefab;
    [SerializeField] private ForageObject _plant2Prefab;
    [SerializeField] private ForageObject _stonePrefab;

    [Header("스폰 설정")]
    [SerializeField] private ForageSpawnBatch[] spawnBatches = new ForageSpawnBatch[]
    {
        new ForageSpawnBatch { forageType = EForageType.Tree, weight = 20f, minCount = 2, maxCount = 5 },
        new ForageSpawnBatch { forageType = EForageType.Stone, weight = 15f, minCount = 1, maxCount = 3 },
        new ForageSpawnBatch { forageType = EForageType.Plant1, weight = 30f, minCount = 3, maxCount = 8 },
        new ForageSpawnBatch { forageType = EForageType.Plant2, weight = 25f, minCount = 2, maxCount = 6 }
    };

    private Dictionary<string, List<Forage>> _chunkForages = new Dictionary<string, List<Forage>>();

    private const float Y_OFFSET = 0.3f;
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
        }

        _repo = new ForageRepository();
        _forages = new List<ForageObject>();

    }

    public async Task LoadAllForages()
    {
        var loadedChunks = WorldManager.Instance.LoadedChunks;

        foreach (var chunk in loadedChunks)
        {
            string chunkId = chunk.Key.ToChunkId();//$"{chunk.Key.X} _ {chunk.Key.Y} _ {chunk.Key.Z}";
            await LoadOrCreateForages(chunkId);
        }
    }
    public async Task LoadOrCreateForages(string chunkId)
    {
        var forages = await _repo.LoadForagesByChunk(chunkId);

        if (forages.Count > 0)
        {
            _chunkForages[chunkId] = forages;
            InstantiateForages(forages);
        }
        else
        {
            var randomForages = GenerateRandomForages(chunkId);
            _chunkForages[chunkId] = randomForages;
            InstantiateForages(randomForages);
            await _repo.SaveForages(chunkId, randomForages);
        }
    }
    private void InstantiateForages(List<Forage> forages)
    {
        foreach (var forage in forages)
        {
            var prefab = GetPrefab(forage.Type);
            if (prefab == null)
            {
                continue;
            }

            var obj = Instantiate(prefab, transform);
            obj.Init(forage);
            _forages.Add(obj);
        }
    }
    public async Task GenerateForagesInChunk(ChunkPosition pos)
    {
        string chunkId = $"{pos.X}_{pos.Y}_{pos.Z}";

        var randomForages = GenerateRandomForages(chunkId);

        _chunkForages[chunkId] = randomForages;

        InstantiateForages(randomForages);

        await _repo.SaveForages(chunkId, randomForages);
    }
    private List<Forage> GenerateRandomForages(string chunkId)
    {
        var result = new List<Forage>();
        var usedPositions = new HashSet<Vector2Int>(); // 사용된 위치 추적

        // 청크 좌표 계산
        var split = chunkId.Split('_');
        int chunkX = int.Parse(split[0]);
        int chunkZ = int.Parse(split[2]);

        float blockOffsetX = WorldManager.Instance.dynamicGenerator.blockOffset.x;
        float blockOffsetZ = WorldManager.Instance.dynamicGenerator.blockOffset.z;

        float chunkWorldOriginX = chunkX * Chunk.ChunkSize;
        float chunkWorldOriginZ = chunkZ * Chunk.ChunkSize;

        if (spawnBatches != null && spawnBatches.Length > 0)
        {
            // 가중치 기반 배치별 생성
            foreach (var batch in spawnBatches)
            {
                int batchCount = UnityEngine.Random.Range(batch.minCount, batch.maxCount + 1);
                
                for (int i = 0; i < batchCount; i++)
                {
                    var forage = CreateForageAtUniquePosition(batch.forageType, chunkId, chunkWorldOriginX, chunkWorldOriginZ, blockOffsetX, blockOffsetZ, usedPositions);
                    if (forage != null) result.Add(forage);
                }
            }
        }

        return result;
    }

    private Forage CreateForageAtUniquePosition(EForageType type, string chunkId, float chunkWorldOriginX, float chunkWorldOriginZ, float blockOffsetX, float blockOffsetZ, HashSet<Vector2Int> usedPositions)
    {
        int attempts = 0;
        int maxAttempts = 50; // 최대 시도 횟수
        
        while (attempts < maxAttempts)
        {
            int localX = UnityEngine.Random.Range(0, Chunk.ChunkSize);
            int localZ = UnityEngine.Random.Range(0, Chunk.ChunkSize);
            
            var gridPos = new Vector2Int(localX, localZ);
            
            // 이미 사용된 위치가 아니면 배치
            if (!usedPositions.Contains(gridPos))
            {
                usedPositions.Add(gridPos);
                
                // 그리드에 맞는 월드 좌표 계산
                float worldX = chunkWorldOriginX * blockOffsetX + localX * blockOffsetX;
                float worldZ = chunkWorldOriginZ * blockOffsetZ + localZ * blockOffsetZ;

                float groundY = WorldManager.Instance.GetGroundHeight(new Vector3(worldX, 0, worldZ));
                var pos = new Vector3(worldX, groundY - Y_OFFSET, worldZ);
                var rot = new Vector3(0, UnityEngine.Random.Range(0f, 360f), 0);

                return new Forage(type, chunkId, pos, rot);
            }
            
            attempts++;
        }
        
        // 최대 시도 횟수 초과시 null 반환 (배치 실패)
        return null;
    }

    private EForageType GetRandomType()
    {
        var values = Enum.GetValues(typeof(EForageType));
        return (EForageType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }
    public async Task SaveForages(string chunkId)
    {
        if (_chunkForages.TryGetValue(chunkId, out var forages))
        {
            await _repo.SaveForages(chunkId, forages);
        }
    }

    public async void RemoveForage(ForageObject obj)
    {
        if (_forages.Contains(obj))
        {
            _forages.Remove(obj);
            obj.RemoveWithAnim();
        }

        if (_chunkForages.TryGetValue(obj.ChunkId, out var forageList))
        {
            var domain = forageList.FirstOrDefault(f => f.Position == obj.transform.position);

            if (domain != null)
            {
                forageList.Remove(domain);

                await SaveForages(obj.ChunkId);
            }
        }
    }
    public ForageObject GetForageAtWorldPosition(Vector3 worldPosition)
    {
        // 가장 가까운 거리의 ForageObject 찾기
        float tolerance = 0.1f; // 부동소수점 오차 허용
        ForageObject closestForage = null;
        float closestDistance = float.MaxValue;

        foreach (var forageObject in _forages)
        {
            float distance = Vector3.Distance(new Vector3(forageObject.transform.position.x, 0, forageObject.transform.position.z), new Vector3(worldPosition.x, 0, worldPosition.z));

            if (distance < tolerance && distance < closestDistance)
            {
                closestDistance = distance;
                closestForage = forageObject;
            }
        }

        return closestForage;
    }

    public List<ForageObject> GetForagesInChunk(string chunkId)
    {
        List<ForageObject> chunkForages = new List<ForageObject>();
        
        foreach (var forageObject in _forages)
        {
            if (forageObject != null && forageObject.ChunkId == chunkId)
            {
                chunkForages.Add(forageObject);
            }
        }
        
        return chunkForages;
    }

    private ForageObject GetPrefab(EForageType type)
    {
        switch (type)
        {
            case EForageType.Tree:
                return _treePrefab;
            case EForageType.Stone:
                return _stonePrefab;
            case EForageType.Plant1:
                return _plant1Prefab;
            case EForageType.Plant2:
                return _plant2Prefab;
            default:
                return null;
        }
    }
}
