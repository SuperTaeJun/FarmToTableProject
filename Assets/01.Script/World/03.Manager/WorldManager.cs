using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { private set; get; }

    [Header("Dynamic Generation")]
    public ChunkGenerator dynamicGenerator;

    [Header("World Settings")]
    public int worldWidth = 5;  // ûũ ����
    public int worldDepth = 5;

    // �ε� �����Ȳ �̺�Ʈ
    public event Action<float> OnLoadingProgress;
    public event Action OnLoadingComplete;

    private WorldRepository _repo;

    private Dictionary<ChunkPosition, Chunk> loadedChunks = new Dictionary<ChunkPosition, Chunk>();

    private Dictionary<ChunkPosition, GameObject> chunkObjects = new Dictionary<ChunkPosition, GameObject>();
    public IEnumerable<ChunkPosition> LoadedChunkPositions => loadedChunks.Keys;
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

        _repo = new WorldRepository();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainScene")
        {
            PositionPlayerAtCenter();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void PositionPlayerAtCenter()
    {
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        if (player == null)
        {
            Debug.LogWarning("�÷��̾� ��ã��");
            return;
        }

        float worldCenterX = (worldWidth * Chunk.ChunkSize * dynamicGenerator.blockOffset.x) / 2f;
        float worldCenterZ = (worldDepth * Chunk.ChunkSize * dynamicGenerator.blockOffset.z) / 2f;

        int centerChunkX = worldWidth / 2;
        int centerChunkZ = worldDepth / 2;
        int centerLocalX = Chunk.ChunkSize / 2;
        int centerLocalZ = Chunk.ChunkSize / 2;

        float groundHeight = FindGroundHeight(centerChunkX, centerChunkZ, centerLocalX, centerLocalZ);

        Vector3 playerPosition = new Vector3(
            worldCenterX,
            groundHeight + 1f, // ���鿡�� 2���� ��
            worldCenterZ
        );

        player.SetPositionForCharacterController(playerPosition);
        Debug.Log($"[WorldManager] �÷��̾ ���� �߾ӿ� ��ġ: {playerPosition}");
    }
    private float FindGroundHeight(int chunkX, int chunkZ, int localX, int localZ)
    {
        var chunkPos = new ChunkPosition(chunkX, 0, chunkZ);

        if (!loadedChunks.TryGetValue(chunkPos, out var chunk))
        {
            // ûũ�� �ε���� ���� ��� �⺻ ���� ��ȯ
            return dynamicGenerator.worldHeight * 0.5f * dynamicGenerator.blockOffset.y;
        }

        // ���������� �Ʒ��� Ž���Ͽ� ù ��° ��� ã��
        for (int y = dynamicGenerator.worldHeight - 1; y >= 0; y--)
        {
            var block = chunk.GetBlock(localX, y, localZ);
            if (block != null)
            {
                // ����� ��� ��ġ ��ȯ
                return (y + 1) * dynamicGenerator.blockOffset.y;
            }
        }

        // ����� ���� ��� �⺻ ����
        return dynamicGenerator.blockOffset.y;
    }
    public float GetGroundHeight(Vector3 worldPos)
    {
        float blockOffsetX = dynamicGenerator.blockOffset.x;
        float blockOffsetZ = dynamicGenerator.blockOffset.z;

        int chunkSizeX = Chunk.ChunkSize;
        int chunkSizeZ = Chunk.ChunkSize;

        float chunkWorldSizeX = chunkSizeX * blockOffsetX;
        float chunkWorldSizeZ = chunkSizeZ * blockOffsetZ;

        int chunkX = Mathf.FloorToInt(worldPos.x / chunkWorldSizeX);
        int chunkZ = Mathf.FloorToInt(worldPos.z / chunkWorldSizeZ);

        float chunkOriginX = chunkX * chunkWorldSizeX;
        float chunkOriginZ = chunkZ * chunkWorldSizeZ;

        float localXf = (worldPos.x - chunkOriginX) / blockOffsetX;
        float localZf = (worldPos.z - chunkOriginZ) / blockOffsetZ;

        int localX = Mathf.Clamp(Mathf.FloorToInt(localXf), 0, chunkSizeX - 1);
        int localZ = Mathf.Clamp(Mathf.FloorToInt(localZf), 0, chunkSizeZ - 1);

        return FindGroundHeight(chunkX, chunkZ, localX, localZ);
    }
    public async Task LoadWorldFromFirebase()
    {
        Debug.Log("[WorldManager] Firebase���� ���� �ε� ����!");

        ClearExistingWorld();

        var chunkPositions = await _repo.GetAllChunkPositionsFromFirebase();

        if (chunkPositions.Count == 0)
        {
            Debug.Log("[WorldManager] DB�� ûũ ����. �⺻ ���� ����.");

            for (int cx = 0; cx < worldWidth; cx++)
            {
                for (int cz = 0; cz < worldDepth; cz++)
                {
                    chunkPositions.Add(new ChunkPosition(cx, 0, cz));
                }
            }
        }

        int totalChunks = chunkPositions.Count;
        int loadedChunkCount = 0;

        foreach (var pos in chunkPositions)
        {
            await LoadChunkFromFirebase(pos);

            loadedChunkCount++;
            float progress = (float)loadedChunkCount / totalChunks;

            if (loadedChunkCount % 2 == 0)
            {
                await Task.Yield();
            }
            OnLoadingProgress?.Invoke(progress);

            await Task.Yield();
        }

        Debug.Log("[WorldManager] Firebase ���� �ε� �Ϸ�!");

        OnLoadingComplete?.Invoke();
    }

    private async Task LoadChunkFromFirebase(ChunkPosition pos)
    {
        // Firebase���� ûũ ������ �ε�
        Chunk firebaseChunk = await _repo.LoadChunkAsync(pos);

        if (firebaseChunk == null)
        {
            Debug.Log($"[WorldManager] Firebase�� ûũ ����, ���� ����: {pos.X},{pos.Z}");
            firebaseChunk = GenerateDynamicChunk(pos);

            // ���� ������ ûũ�� Firebase�� ����
            await _repo.SaveChunkAsync(firebaseChunk);
        }
        else
        {
            Debug.Log($"[WorldManager] Firebase���� ûũ �ε� �Ϸ�: {pos.X},{pos.Z}");
        }

        // �޸𸮿� �ε�
        loadedChunks[pos] = firebaseChunk;

        // �������� ���� ������
        await BuildChunkInScene(pos, firebaseChunk);
    }

    private Chunk GenerateDynamicChunk(ChunkPosition pos)
    {
        var chunk = new Chunk(pos);

        for (int x = 0; x < Chunk.ChunkSize; x++)
        {
            for (int z = 0; z < Chunk.ChunkSize; z++)
            {
                int worldX = pos.X * Chunk.ChunkSize + x;
                int worldZ = pos.Z * Chunk.ChunkSize + z;

                int height = GetDynamicHeight(worldX, worldZ);

                for (int y = 0; y < Mathf.Clamp(height, 1, Chunk.ChunkSize); y++)
                {
                    var blockPos = new BlockPosition(x, y, z);
                    var blockType = (y == height - 1) ? EBlockType.Grass : EBlockType.Dirt;

                    var block = new Block(blockType, blockPos);
                    chunk.SetBlock(block);
                }
            }
        }

        return chunk;
    }

    private int GetDynamicHeight(int worldX, int worldZ)
    {
        float frequency = 0.02f;
        float amplitude = dynamicGenerator.worldHeight * 0.4f;

        float noise = Mathf.PerlinNoise(worldX * frequency, worldZ * frequency);
        int baseHeight = Mathf.FloorToInt(noise * amplitude) + (int)(dynamicGenerator.worldHeight * 0.3f);

        return Mathf.Clamp(baseHeight, 1, dynamicGenerator.worldHeight);
    }

    private async Task BuildChunkInScene(ChunkPosition pos, Chunk chunk)
    {
        // Chunk �����͸� ���� �����ͷ� ��ȯ
        string[,,] chunkWorldData = ConvertChunkToWorldData(chunk);

        // DynamicChunkGenerator�� ���� ������
        GameObject chunkObject = dynamicGenerator.GenerateDynamicChunk(pos, chunkWorldData);

        // ûũ ������Ʈ ���
        chunkObjects[pos] = chunkObject;

        await Task.Yield(); // ������ �л�
    }

    private string[,,] ConvertChunkToWorldData(Chunk chunk)
    {
        int chunkSize = Chunk.ChunkSize;
        string[,,] worldData = new string[chunkSize, dynamicGenerator.worldHeight, chunkSize];

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < dynamicGenerator.worldHeight; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    var block = chunk.GetBlock(x, y, z);
                    if (block != null)
                    {
                        worldData[x, y, z] = block.Type == EBlockType.Grass ? "Grass" : "Dirt";
                    }
                }
            }
        }

        return worldData;
    }
    private void ClearExistingWorld()
    {
        // ���� ûũ ������Ʈ�� ����
        foreach (var kvp in chunkObjects)
        {
            if (kvp.Value != null)
            {
                DestroyImmediate(kvp.Value);
            }
        }

        chunkObjects.Clear();
        loadedChunks.Clear();

        Debug.Log("[WorldManager] ���� ���� ���� �Ϸ�");
    }
    public bool HasChunk(ChunkPosition pos)
    {
        return loadedChunks.ContainsKey(pos);
    }
    public async void GenerateAndBuildChunk(ChunkPosition pos)
    {
        var newChunk = GenerateDynamicChunk(pos);
        loadedChunks[pos] = newChunk;

        await BuildChunkInScene(pos, newChunk);
        await _repo.SaveChunkAsync(loadedChunks[pos]);
        await ForageManager.Instance.GenerateForagesInChunk(pos);
    }
   
}
