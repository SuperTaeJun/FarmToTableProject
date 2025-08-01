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

    private Dictionary<ChunkPosition, Chunk> _loadedChunks = new Dictionary<ChunkPosition, Chunk>();
    public Dictionary<ChunkPosition, Chunk> LoadedChunks => _loadedChunks;

    private Dictionary<ChunkPosition, GameObject> _chunkObjects = new Dictionary<ChunkPosition, GameObject>();
    public IEnumerable<ChunkPosition> LoadedChunkPositions => _loadedChunks.Keys;

    // �޽� ������Ʈ�� �ʿ��� ûũ���� �����ϴ� ť
    private HashSet<ChunkPosition> _chunksNeedMeshUpdate = new HashSet<ChunkPosition>();
    private bool _isUpdatingMeshes = false;

    public static string GetChunkId(Vector3 worldPosition)
    {
        Chunk chunk = WorldManager.Instance.GetChunkAtWorldPosition(worldPosition);
        return $"{chunk.Position.X}_{chunk.Position.Y}_{chunk.Position.Z}";
    }

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
    private void Start()
    {
        SoundManager.Instance.PlayBGM(BGMType.Loading);
    }
    private void Update()
    {
        ProcessMeshUpdates();
    }

    public async void SaveWorld()
    {
        foreach (var chunk in LoadedChunks.Values)
        {
            await _repo.SaveChunkAsync(chunk);
        }
    }


    private void ProcessMeshUpdates()
    {
        if (_chunksNeedMeshUpdate.Count > 0 && !_isUpdatingMeshes)
        {
            StartCoroutine(UpdateChunkMeshes());
        }
    }

    private IEnumerator UpdateChunkMeshes()
    {
        _isUpdatingMeshes = true;

        var chunksToUpdate = new List<ChunkPosition>(_chunksNeedMeshUpdate);
        _chunksNeedMeshUpdate.Clear();

        foreach (var chunkPos in chunksToUpdate)
        {
            if (_loadedChunks.ContainsKey(chunkPos))
            {
                RebuildChunkWithNeighborCheck(chunkPos);
                yield return null; // ������ �л�
            }
        }

        _isUpdatingMeshes = false;
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

        if (!_loadedChunks.TryGetValue(chunkPos, out var chunk))
        {
            // ûũ�� �ε���� ���� ��� �⺻ ���� ��ȯ
            return dynamicGenerator.worldHeight * 0.5f * dynamicGenerator.blockOffset.y;
        }

        // ���������� �Ʒ��� Ž���Ͽ� ù ��° ���� ���� ã��
        for (int y = dynamicGenerator.worldHeight - 1; y >= 0; y--)
        {
            var block = chunk.GetBlock(localX, y, localZ);
            if (block != null && block.Type != EBlockType.Air)
            {
                // ���� ������ ��� ��ġ ��ȯ
                return (y + 1) * dynamicGenerator.blockOffset.y;
            }
        }

        // ���� ������ ���� ��� �⺻ ����
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
        _loadedChunks[pos] = firebaseChunk;

        // �������� ���� ������
        await BuildChunkInScene(pos, firebaseChunk);

        // ���� ûũ���� �޽� ������Ʈ ����
        ScheduleAdjacentChunkMeshUpdates(pos);
    }

    // ���� ûũ���� �޽� ������Ʈ�� �����ϴ� �޼���
    private void ScheduleAdjacentChunkMeshUpdates(ChunkPosition newChunkPos)
    {
        ChunkPosition[] adjacentPositions = {
            new ChunkPosition(newChunkPos.X - 1, newChunkPos.Y, newChunkPos.Z), // Left
            new ChunkPosition(newChunkPos.X + 1, newChunkPos.Y, newChunkPos.Z), // Right
            new ChunkPosition(newChunkPos.X, newChunkPos.Y - 1, newChunkPos.Z), // Down
            new ChunkPosition(newChunkPos.X, newChunkPos.Y + 1, newChunkPos.Z), // Up
            new ChunkPosition(newChunkPos.X, newChunkPos.Y, newChunkPos.Z - 1), // Back
            new ChunkPosition(newChunkPos.X, newChunkPos.Y, newChunkPos.Z + 1)  // Forward
        };

        foreach (var adjacentPos in adjacentPositions)
        {
            if (_loadedChunks.ContainsKey(adjacentPos))
            {
                _chunksNeedMeshUpdate.Add(adjacentPos);
            }
        }
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

                // ��ü ���̿� ���� ���� ����
                for (int y = 0; y < dynamicGenerator.worldHeight; y++)
                {
                    var blockPos = new BlockPosition(x, y, z);
                    EBlockType blockType;

                    if (y < height)
                    {
                        // ���� ���� ���ϴ� ���� ����
                        blockType = (y == height - 1) ? EBlockType.Grass : EBlockType.Dirt;
                    }
                    else
                    {
                        // ���� ���� �̻��� Air ����
                        blockType = EBlockType.Air;
                    }

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
        var tcs = new TaskCompletionSource<bool>();

        //����ȭ ����
        StartCoroutine(dynamicGenerator.GenerateDynamicChunkCoroutine
(
    pos,
    chunkWorldData,
    this.transform,
    chunkObject => { _chunkObjects[pos] = chunkObject; tcs.SetResult(true); }
));

        ////����ȭ �� ����
        //// DynamicChunkGenerator�� ���� ������
        //GameObject chunkObject = dynamicGenerator.GenerateDynamicChunk(pos, chunkWorldData);


        //// ûũ ������Ʈ ���
        //chunkObjects[pos] = chunkObject;

        await tcs.Task;
        //await Task.Yield(); // ������ �л�
    }

    // ���� ûũ ������ ������ ûũ �����
    private void RebuildChunkWithNeighborCheck(ChunkPosition chunkPos)
    {
        if (!_loadedChunks.TryGetValue(chunkPos, out Chunk chunk))
            return;

        // ���� ûũ ������Ʈ ����
        if (_chunkObjects.TryGetValue(chunkPos, out GameObject oldChunkObject))
        {
            Destroy(oldChunkObject);
            _chunkObjects.Remove(chunkPos);
        }

        // ���� ûũ ������ ������ ���� ������ ����
        string[,,] chunkWorldData = ConvertChunkToWorldDataWithNeighbors(chunk);
        GameObject chunkObject = dynamicGenerator.GenerateDynamicChunk(chunkPos, chunkWorldData);
        _chunkObjects[chunkPos] = chunkObject;
    }

    // ���� ûũ ������ ������ ���� ������ ��ȯ
    private string[,,] ConvertChunkToWorldDataWithNeighbors(Chunk chunk)
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
                    if (block != null && block.Type != EBlockType.Air)
                    {
                        // �� ������ �������Ǿ�� �ϴ��� Ȯ��
                        if (IsBlockVisibleWithNeighbors(chunk.Position, x, y, z))
                        {
                            worldData[x, y, z] = block.Type.ToString();
                        }
                    }
                }
            }
        }

        return worldData;
    }

    // ���� ûũ�� ������ ���� ���ü� üũ
    private bool IsBlockVisibleWithNeighbors(ChunkPosition chunkPos, int x, int y, int z)
    {
        Vector3Int[] directions = {
            Vector3Int.left,    // -X
            Vector3Int.right,   // +X
            Vector3Int.up,      // +Y
            //Vector3Int.down,    // -Y
            Vector3Int.back,    // -Z
            Vector3Int.forward  // +Z
        };

        foreach (var dir in directions)
        {
            int adjacentX = x + dir.x;
            int adjacentY = y + dir.y;
            int adjacentZ = z + dir.z;

            // ���� ��ġ�� ������ �ִ��� Ȯ�� (ûũ ��� ����)
            if (!HasBlockAtPosition(chunkPos, adjacentX, adjacentY, adjacentZ))
            {
                return true; // ���� ��ġ�� ��������� �� ���� �׸�
            }
        }

        return false; // ��� ���� ��������
    }

    // ûũ ��踦 �ѳ���� ���� ���� ���� Ȯ��
    private bool HasBlockAtPosition(ChunkPosition baseChunkPos, int x, int y, int z)
    {
        ChunkPosition targetChunkPos = baseChunkPos;
        int localX = x;
        int localY = y;
        int localZ = z;

        // ûũ ��踦 �Ѿ�� ��� ��ǥ ����
        if (x < 0)
        {
            targetChunkPos = new ChunkPosition(baseChunkPos.X - 1, baseChunkPos.Y, baseChunkPos.Z);
            localX = Chunk.ChunkSize - 1;
        }
        else if (x >= Chunk.ChunkSize)
        {
            targetChunkPos = new ChunkPosition(baseChunkPos.X + 1, baseChunkPos.Y, baseChunkPos.Z);
            localX = 0;
        }

        if (y < 0)
        {
            targetChunkPos = new ChunkPosition(targetChunkPos.X, baseChunkPos.Y - 1, targetChunkPos.Z);
            localY = dynamicGenerator.worldHeight - 1;
        }
        else if (y >= dynamicGenerator.worldHeight)
        {
            targetChunkPos = new ChunkPosition(targetChunkPos.X, baseChunkPos.Y + 1, targetChunkPos.Z);
            localY = 0;
        }

        if (z < 0)
        {
            targetChunkPos = new ChunkPosition(targetChunkPos.X, targetChunkPos.Y, baseChunkPos.Z - 1);
            localZ = Chunk.ChunkSize - 1;
        }
        else if (z >= Chunk.ChunkSize)
        {
            targetChunkPos = new ChunkPosition(targetChunkPos.X, targetChunkPos.Y, baseChunkPos.Z + 1);
            localZ = 0;
        }

        return HasBlockAt(targetChunkPos, localX, localY, localZ);
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
                        // Air ������ ���������� ���� (null�� �α�)
                        if (block.Type != EBlockType.Air)
                        {
                            worldData[x, y, z] = block.Type == EBlockType.Grass ? "Grass" : "Dirt";
                        }
                    }
                }
            }
        }

        return worldData;
    }

    private void ClearExistingWorld()
    {
        // ���� ûũ ������Ʈ�� ����
        foreach (var kvp in _chunkObjects)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value);
            }
        }

        _chunkObjects.Clear();
        _loadedChunks.Clear();

        Debug.Log("[WorldManager] ���� ���� ���� �Ϸ�");
    }

    public bool HasChunk(ChunkPosition pos)
    {
        return _loadedChunks.ContainsKey(pos);
    }

    public async void GenerateAndBuildChunk(ChunkPosition pos)
    {
        var newChunk = GenerateDynamicChunk(pos);
        _loadedChunks[pos] = newChunk;

        await BuildChunkInScene(pos, newChunk);
        await _repo.SaveChunkAsync(_loadedChunks[pos]);
        await ForageManager.Instance.GenerateForagesInChunk(pos);

        // ���� ûũ���� �޽� ������Ʈ ����
        ScheduleAdjacentChunkMeshUpdates(pos);
    }

    public Chunk GetChunkAtWorldPosition(Vector3 worldPosition)
    {
        float blockOffsetX = dynamicGenerator.blockOffset.x;
        float blockOffsetZ = dynamicGenerator.blockOffset.z;

        int chunkSizeX = Chunk.ChunkSize;
        int chunkSizeZ = Chunk.ChunkSize;

        float chunkWorldSizeX = chunkSizeX * blockOffsetX;
        float chunkWorldSizeZ = chunkSizeZ * blockOffsetZ;

        int chunkX = Mathf.FloorToInt(worldPosition.x / chunkWorldSizeX);
        int chunkZ = Mathf.FloorToInt(worldPosition.z / chunkWorldSizeZ);

        var chunkPos = new ChunkPosition(chunkX, 0, chunkZ);

        if (_loadedChunks.TryGetValue(chunkPos, out Chunk chunk))
        {
            return chunk;
        }

        return null;
    }

    public Vector3 GetWorldPositionFromChunkLocal(ChunkPosition chunkPos, Vector3 localPosition)
    {
        float blockOffsetX = dynamicGenerator.blockOffset.x;
        float blockOffsetZ = dynamicGenerator.blockOffset.z;
        int chunkSizeX = Chunk.ChunkSize;
        int chunkSizeZ = Chunk.ChunkSize;

        float chunkWorldSizeX = chunkSizeX * blockOffsetX;
        float chunkWorldSizeZ = chunkSizeZ * blockOffsetZ;

        // ûũ�� ���� ���� ��ǥ
        float chunkWorldStartX = chunkPos.X * chunkWorldSizeX;
        float chunkWorldStartZ = chunkPos.Z * chunkWorldSizeZ;

        // ���� ��ǥ�� ���� ��ǥ�� ��ȯ
        float worldX = chunkWorldStartX + (localPosition.x * blockOffsetX);
        float worldY = localPosition.y; // Y�� ��ȯ ���ʿ�
        float worldZ = chunkWorldStartZ + (localPosition.z * blockOffsetZ);

        return new Vector3(worldX, worldY, worldZ);
    }

    // ûũ id�ε� �����ϰ� �������̵�
    public Vector3 GetWorldPositionFromChunkLocal(string chunkId, Vector3 localPosition)
    {
        var chunkPos = GetChunkPositionFromId(chunkId);
        return GetWorldPositionFromChunkLocal(chunkPos, localPosition);
    }

    public ChunkPosition GetChunkPositionFromId(string chunkId)
    {
        string[] parts = chunkId.Split('_');
        if (parts.Length != 3)
        {
            Debug.LogError($"Invalid chunkId format: {chunkId}");
            return new ChunkPosition(0, 0, 0);
        }

        int chunkX = int.Parse(parts[0]);
        int chunkY = int.Parse(parts[1]);
        int chunkZ = int.Parse(parts[2]);

        return new ChunkPosition(chunkX, chunkY, chunkZ);
    }


    public Vector3 GetLocalPositionInChunk(Vector3 worldPosition, ChunkPosition chunkPos)
    {
        float blockOffsetX = dynamicGenerator.blockOffset.x;
        float blockOffsetZ = dynamicGenerator.blockOffset.z;
        int chunkSizeX = Chunk.ChunkSize;
        int chunkSizeZ = Chunk.ChunkSize;

        float chunkWorldSizeX = chunkSizeX * blockOffsetX;
        float chunkWorldSizeZ = chunkSizeZ * blockOffsetZ;

        // ûũ�� ���� ���� ��ǥ
        float chunkWorldStartX = chunkPos.X * chunkWorldSizeX;
        float chunkWorldStartZ = chunkPos.Z * chunkWorldSizeZ;

        // ���� ��ǥ���� ûũ ���� ��ǥ�� ��ȯ
        float localX = (worldPosition.x - chunkWorldStartX) / blockOffsetX;
        float localY = worldPosition.y;
        float localZ = (worldPosition.z - chunkWorldStartZ) / blockOffsetZ;

        return new Vector3(localX, localY, localZ);
    }
    public bool SetBlock(Vector3 worldPosition, EBlockType blockType, int size = 1)
    {
        HashSet<ChunkPosition> affectedChunks = new HashSet<ChunkPosition>();
        bool success = true;

        // 중심 기준으로 size x size 영역 계산 (블록 오프셋 적용)
        int halfSize = size / 2;
        Vector3 blockOffset = dynamicGenerator.blockOffset;
        
        for (int x = -halfSize; x <= halfSize; x++)
        {
            for (int z = -halfSize; z <= halfSize; z++)
            {
                Vector3 currentWorldPos = worldPosition + new Vector3(x * blockOffset.x, 0, z * blockOffset.z);

                if (!SetSingleBlock(currentWorldPos, blockType, affectedChunks))
                {
                    success = false;
                }
            }
        }

        foreach (var chunkPos in affectedChunks)
        {
            RebuildChunk(chunkPos);
        }

        return success;
    }

    private bool SetSingleBlock(Vector3 worldPosition, EBlockType blockType, HashSet<ChunkPosition> affectedChunks)
    {
        var chunk = GetChunkAtWorldPosition(worldPosition);
        if (chunk == null)
        {
            Debug.LogWarning($"���� ��ġ {worldPosition}���� ûũ�� ã�� �� �����ϴ�.");
            return false;
        }

        var localPos = GetLocalPositionInChunk(worldPosition, chunk.Position);
        int blockX = Mathf.FloorToInt(localPos.x);
        int blockY = Mathf.FloorToInt(localPos.y / dynamicGenerator.blockOffset.y);
        int blockZ = Mathf.FloorToInt(localPos.z);
        var localBlockPos = new BlockPosition(blockX, blockY, blockZ);

        if (!IsValidBlockPosition(localBlockPos))
        {
            Debug.LogWarning($"�߸��� ���� ��ġ: {localBlockPos.X}, {localBlockPos.Y}, {localBlockPos.Z}");
            return false;
        }

        var newBlock = new Block(blockType, localBlockPos);
        chunk.SetBlock(newBlock);

        // ������� ûũ �߰�
        affectedChunks.Add(chunk.Position);

        return true;
    }
    public EBlockType GetBlockType(Vector3 worldPosition)
    {
        // ���� ��ǥ���� ûũ ã��
        var chunk = GetChunkAtWorldPosition(worldPosition);

        // ûũ �� ���� ���� ��ǥ ���
        var localPos = GetLocalPositionInChunk(worldPosition, chunk.Position);
        int blockX = Mathf.FloorToInt(localPos.x);
        int blockY = Mathf.FloorToInt(localPos.y / dynamicGenerator.blockOffset.y);
        int blockZ = Mathf.FloorToInt(localPos.z);

        var localBlockPos = new BlockPosition(blockX, blockY, blockZ);

        var block = chunk.GetBlock(localBlockPos.X, localBlockPos.Y, localBlockPos.Z);
        return block.Type;
    }

    private bool IsValidBlockPosition(BlockPosition pos)
    {
        return pos.X >= 0 && pos.X < Chunk.ChunkSize &&
               pos.Y >= 0 && pos.Y < dynamicGenerator.worldHeight &&
               pos.Z >= 0 && pos.Z < Chunk.ChunkSize;
    }

    private void RebuildChunk(ChunkPosition chunkPos)
    {
        RebuildChunkWithNeighborCheck(chunkPos);
    }

    public bool HasBlockAt(ChunkPosition chunkPos, int x, int y, int z)
    {
        // ûũ�� �ε�Ǿ� �ִ��� Ȯ��
        if (!_loadedChunks.ContainsKey(chunkPos))
        {
            return false; // ûũ�� ������ ���ϵ� ����
        }

        var chunk = _loadedChunks[chunkPos];
        var block = chunk.GetBlock(x, y, z);
        return block != null && block.Type != EBlockType.Air;
    }
}
