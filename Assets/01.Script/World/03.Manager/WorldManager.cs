using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
public class WorldManager : MonoBehaviourSingleton<WorldManager>
{
    [Header("Dynamic Generation")]
    public ChunkGenerator dynamicGenerator;

    [Header("World Settings")]
    public int worldWidth = 5;  // ûũ ����
    public int worldDepth = 5;

    // �ε� �����Ȳ �̺�Ʈ
    public event Action<float> OnLoadingProgress;
    public event Action OnLoadingComplete;

    private WorldRepository repo;

    // ���� �޸𸮿� �ε�� Chunk ������
    private Dictionary<ChunkPosition, Chunk> loadedChunks = new Dictionary<ChunkPosition, Chunk>();

    // ���� �����ϴ� ûũ GameObject ����
    private Dictionary<ChunkPosition, GameObject> chunkObjects = new Dictionary<ChunkPosition, GameObject>();

    protected override void Awake()
    {
        base.Awake();
        repo = new WorldRepository();

        // DynamicChunkGenerator�� ������ �ڵ����� ����
        if (dynamicGenerator == null)
        {
            var go = new GameObject("DynamicChunkGenerator");
            go.transform.SetParent(this.transform);
            dynamicGenerator = go.AddComponent<ChunkGenerator>();
        }
    }

    private async void Start()
    {
        await LoadWorldFromFirebase();

    }
    private void PositionPlayerAtCenter()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("[WorldManager] 'Player' �±׸� ���� ������Ʈ�� ã�� �� �����ϴ�!");
            return;
        }

        // ���� �߾� ��� (ûũ ���� �� ���� ��ǥ)
        float worldCenterX = (worldWidth * Chunk.ChunkSize * dynamicGenerator.blockOffset.x) / 2f;
        float worldCenterZ = (worldDepth * Chunk.ChunkSize * dynamicGenerator.blockOffset.z) / 2f;

        // �߾� ��ġ�� ���� ���� ã��
        int centerChunkX = worldWidth / 2;
        int centerChunkZ = worldDepth / 2;
        int centerLocalX = Chunk.ChunkSize / 2;
        int centerLocalZ = Chunk.ChunkSize / 2;

        float groundHeight = FindGroundHeight(centerChunkX, centerChunkZ, centerLocalX, centerLocalZ);

        // �÷��̾ ���� ���� ��ġ (�ణ�� ���� ���� �߰�)
        Vector3 playerPosition = new Vector3(
            worldCenterX,
            groundHeight + 2f, // ���鿡�� 2���� ��
            worldCenterZ
        );

        player.transform.position = playerPosition;

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
    #region Firebase ��� ���� �ε�

    public async Task LoadWorldFromFirebase()
    {
        Debug.Log("[WorldManager] Firebase���� ���� �ε� ����!");

        ClearExistingWorld();

        int totalChunks = worldWidth * worldDepth;
        int loadedChunkCount = 0;

        for (int cx = 0; cx < worldWidth; cx++)
        {
            for (int cz = 0; cz < worldDepth; cz++)
            {
                var pos = new ChunkPosition(cx, 0, cz);
                await LoadChunkFromFirebase(pos);

                loadedChunkCount++;
                float progress = (float)loadedChunkCount / totalChunks;

                // �ε� �����Ȳ �˸�
                OnLoadingProgress?.Invoke(progress);

                await Task.Yield();
            }
        }

        Debug.Log("[WorldManager] Firebase ���� �ε� �Ϸ�!");

        // �ε� �Ϸ� �˸�
        OnLoadingComplete?.Invoke();
        PositionPlayerAtCenter();
    }

    private async Task LoadChunkFromFirebase(ChunkPosition pos)
    {
        // Firebase���� ûũ ������ �ε�
        Chunk firebaseChunk = await repo.LoadChunkAsync(pos);

        if (firebaseChunk == null)
        {
            Debug.Log($"[WorldManager] Firebase�� ûũ ����, ���� ����: {pos.X},{pos.Z}");
            firebaseChunk = GenerateDynamicChunk(pos);

            // ���� ������ ûũ�� Firebase�� ����
            await repo.SaveChunkAsync(firebaseChunk);
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

    #endregion

    #region ���� ûũ ����

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

    #endregion

    #region ���� ������

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

    #endregion

    #region ���� ����

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

    public async Task SaveWorldToFirebase()
    {
        Debug.Log("[WorldManager] Firebase�� ���� ���� ����!");

        foreach (var kvp in loadedChunks)
        {
            await repo.SaveChunkAsync(kvp.Value);
        }

        Debug.Log("[WorldManager] Firebase ���� ���� �Ϸ�!");
    }

    public async Task ReloadWorldFromFirebase()
    {
        Debug.Log("[WorldManager] Firebase���� ���� �ٽ� �ε�!");
        await LoadWorldFromFirebase();
    }

    #endregion

    #region ûũ ���� �ε� (���� �����)

    public async Task LoadChunkIfNeeded(ChunkPosition pos)
    {
        if (loadedChunks.ContainsKey(pos))
            return;

        await LoadChunkFromFirebase(pos);
    }

    public void UnloadChunk(ChunkPosition pos)
    {
        if (chunkObjects.TryGetValue(pos, out var chunkObj))
        {
            DestroyImmediate(chunkObj);
            chunkObjects.Remove(pos);
        }

        loadedChunks.Remove(pos);

        Debug.Log($"[WorldManager] ûũ ��ε�: {pos.X},{pos.Z}");
    }

    public async Task<Chunk> GetOrLoadChunk(ChunkPosition pos)
    {
        if (loadedChunks.TryGetValue(pos, out var chunk))
        {
            return chunk;
        }

        await LoadChunkFromFirebase(pos);
        return loadedChunks.TryGetValue(pos, out chunk) ? chunk : null;
    }

    #endregion

    #region ��� ����

    public async Task SetBlock(int worldX, int worldY, int worldZ, EBlockType blockType)
    {
        // ���� ��ǥ�� ûũ ��ǥ�� ��ȯ
        int chunkX = worldX / Chunk.ChunkSize;
        int chunkZ = worldZ / Chunk.ChunkSize;
        int localX = worldX % Chunk.ChunkSize;
        int localZ = worldZ % Chunk.ChunkSize;

        var chunkPos = new ChunkPosition(chunkX, 0, chunkZ);
        var chunk = await GetOrLoadChunk(chunkPos);

        if (chunk != null)
        {
            var blockPos = new BlockPosition(localX, worldY, localZ);
            var block = new Block(blockType, blockPos);

            chunk.SetBlock(block);

            // Firebase�� ����
            await repo.SaveChunkAsync(chunk);

            // ������ ûũ �ٽ� ������
            await RefreshChunkInScene(chunkPos);
        }
    }

    public async Task RemoveBlock(int worldX, int worldY, int worldZ)
    {
        int chunkX = worldX / Chunk.ChunkSize;
        int chunkZ = worldZ / Chunk.ChunkSize;
        int localX = worldX % Chunk.ChunkSize;
        int localZ = worldZ % Chunk.ChunkSize;

        var chunkPos = new ChunkPosition(chunkX, 0, chunkZ);
        var chunk = await GetOrLoadChunk(chunkPos);

        if (chunk != null)
        {
            // ��� ���� (null�� ����)
            chunk.Blocks[localX, worldY, localZ] = null;

            // Firebase�� ����
            await repo.SaveChunkAsync(chunk);

            // ������ ûũ �ٽ� ������
            await RefreshChunkInScene(chunkPos);
        }
    }

    private async Task RefreshChunkInScene(ChunkPosition pos)
    {
        // ���� ûũ ������Ʈ ����
        if (chunkObjects.TryGetValue(pos, out var oldChunkObj))
        {
            DestroyImmediate(oldChunkObj);
        }

        // ���� ������
        if (loadedChunks.TryGetValue(pos, out var chunk))
        {
            await BuildChunkInScene(pos, chunk);
        }
    }

    #endregion

    #region ���� API

    /// <summary>
    /// ���� �ε�� ûũ �� ��ȯ
    /// </summary>
    public int GetLoadedChunkCount()
    {
        return loadedChunks.Count;
    }

    /// <summary>
    /// Ư�� ��ġ�� ûũ�� �ε�Ǿ� �ִ��� Ȯ��
    /// </summary>
    public bool IsChunkLoaded(ChunkPosition pos)
    {
        return loadedChunks.ContainsKey(pos);
    }

    /// <summary>
    /// ��� �ε�� ûũ�� ��ġ ��ȯ
    /// </summary>
    public List<ChunkPosition> GetLoadedChunkPositions()
    {
        return new List<ChunkPosition>(loadedChunks.Keys);
    }

    /// <summary>
    /// Ư�� ���� ��ǥ�� ��� Ÿ�� ��ȯ
    /// </summary>
    public async Task<EBlockType?> GetBlockType(int worldX, int worldY, int worldZ)
    {
        int chunkX = worldX / Chunk.ChunkSize;
        int chunkZ = worldZ / Chunk.ChunkSize;
        int localX = worldX % Chunk.ChunkSize;
        int localZ = worldZ % Chunk.ChunkSize;

        var chunkPos = new ChunkPosition(chunkX, 0, chunkZ);
        var chunk = await GetOrLoadChunk(chunkPos);

        if (chunk != null)
        {
            var block = chunk.GetBlock(localX, worldY, localZ);
            return block?.Type;
        }

        return null;
    }

    #endregion
}
