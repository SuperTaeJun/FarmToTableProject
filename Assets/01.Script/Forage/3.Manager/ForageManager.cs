using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Unity.VisualScripting;
public class ForageManager : MonoBehaviour
{
    public static ForageManager Instance { private set; get; }

    private ForageRepository _repo;
    private List<ForageObject> _forages;
    public List<ForageObject> Forages => _forages;

    [Header("채집물 프리팹")]
    [SerializeField] private ForageObject _treePrefab;
    [SerializeField] private ForageObject _stonePrefab;


    private Dictionary<string, List<Forage>> _chunkForages = new Dictionary<string, List<Forage>>();

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

    private async void Start()
    {
        // 예시 호출
        await LoadAllForages();
    }
    public async Task GenerateAndSaveAllChunks()
    {
        int worldWidth = WorldManager.Instance.worldWidth;
        int worldDepth = WorldManager.Instance.worldDepth;

        ClearForages();

        for (int cx = 0; cx < worldWidth; cx++)
        {
            for (int cz = 0; cz < worldDepth; cz++)
            {
                string chunkId = $"{cx}_0_{cz}";
                var randomForages = GenerateRandomForages(chunkId);
                await _repo.SaveForages(chunkId, randomForages);
                InstantiateForages(randomForages);
            }
        }
    }
    public async Task LoadAllForages()
    {
        var loadedChunks = WorldManager.Instance.LoadedChunkPositions;

        foreach (var chunkPos in loadedChunks)
        {
            string chunkId = $"{chunkPos.X}_{chunkPos.Y}_{chunkPos.Z}";
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
                Debug.LogWarning($"Prefab not found for type: {forage.Type}");
                continue;
            }

            var obj = Instantiate(prefab,transform);
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

        Debug.Log($"[ForageManager] Generated and saved forages in chunk {chunkId}");
    }
    private List<Forage> GenerateRandomForages(string chunkId)
    {
        var result = new List<Forage>();

        int count = UnityEngine.Random.Range(3, 10);

        var split = chunkId.Split('_');
        int chunkX = int.Parse(split[0]);
        int chunkZ = int.Parse(split[2]);

        float blockOffsetX = WorldManager.Instance.dynamicGenerator.blockOffset.x;
        float blockOffsetZ = WorldManager.Instance.dynamicGenerator.blockOffset.z;

        float chunkWorldOriginX = chunkX * Chunk.ChunkSize;
        float chunkWorldOriginZ = chunkZ * Chunk.ChunkSize;

        for (int i = 0; i < count; i++)
        {
            var type = GetRandomType();

            float localX = UnityEngine.Random.Range(0f, Chunk.ChunkSize) * blockOffsetX;
            float localZ = UnityEngine.Random.Range(0f, Chunk.ChunkSize) * blockOffsetZ;

            float worldX = Mathf.Floor(chunkWorldOriginX + localX);
            float worldZ = Mathf.Floor(chunkWorldOriginZ + localZ);

            float groundY = WorldManager.Instance.GetGroundHeight(new Vector3(worldX, 0, worldZ));

            var pos = new Vector3(worldX, groundY - 0.7f, worldZ);

            var rot = new Vector3(
                0,
                UnityEngine.Random.Range(0f, 360f),
                0
            );

            var forage = new Forage(type, chunkId, pos, rot);
            result.Add(forage);
        }

        return result;
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
            Destroy(obj.gameObject);
        }

        if (_chunkForages.TryGetValue(obj.ChunkId, out var forageList))
        {
            var domain = forageList.FirstOrDefault(f => f.Position == obj.transform.position);

            if (domain != null)
            {
                forageList.Remove(domain);

                Debug.Log($"[ForageManager] Forage removed from domain list: {domain.Type}");

                // 즉시 Firebase 반영
                await SaveForages(obj.ChunkId);
            }
        }
    }

    private void ClearForages()
    {
        foreach (var obj in _forages)
        {
            Destroy(obj.gameObject);
        }
        _forages.Clear();
    }

    private ForageObject GetPrefab(EForageType type)
    {
        switch (type)
        {
            case EForageType.Tree:
                return _treePrefab;
            case EForageType.Stone:
                return _stonePrefab;
            //case EForageType.Flower:
            //    return _flowerPrefab;
            default:
                return null;
        }
    }
}
