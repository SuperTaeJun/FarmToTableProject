using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
public class WorldManager : MonoBehaviourSingleton<WorldManager>
{
    [Header("Dynamic Generation")]
    public ChunkGenerator dynamicGenerator;

    [Header("World Settings")]
    public int worldWidth = 5;  // 청크 단위
    public int worldDepth = 5;

    // 로딩 진행상황 이벤트
    public event Action<float> OnLoadingProgress;
    public event Action OnLoadingComplete;

    private WorldRepository repo;

    // 현재 메모리에 로드된 Chunk 데이터
    private Dictionary<ChunkPosition, Chunk> loadedChunks = new Dictionary<ChunkPosition, Chunk>();

    // 씬에 존재하는 청크 GameObject 참조
    private Dictionary<ChunkPosition, GameObject> chunkObjects = new Dictionary<ChunkPosition, GameObject>();

    protected override void Awake()
    {
        base.Awake();
        repo = new WorldRepository();

        // DynamicChunkGenerator가 없으면 자동으로 생성
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
            Debug.LogWarning("[WorldManager] 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다!");
            return;
        }

        // 월드 중앙 계산 (청크 단위 → 월드 좌표)
        float worldCenterX = (worldWidth * Chunk.ChunkSize * dynamicGenerator.blockOffset.x) / 2f;
        float worldCenterZ = (worldDepth * Chunk.ChunkSize * dynamicGenerator.blockOffset.z) / 2f;

        // 중앙 위치의 지면 높이 찾기
        int centerChunkX = worldWidth / 2;
        int centerChunkZ = worldDepth / 2;
        int centerLocalX = Chunk.ChunkSize / 2;
        int centerLocalZ = Chunk.ChunkSize / 2;

        float groundHeight = FindGroundHeight(centerChunkX, centerChunkZ, centerLocalX, centerLocalZ);

        // 플레이어를 지면 위에 배치 (약간의 여유 높이 추가)
        Vector3 playerPosition = new Vector3(
            worldCenterX,
            groundHeight + 2f, // 지면에서 2유닛 위
            worldCenterZ
        );

        player.transform.position = playerPosition;

        Debug.Log($"[WorldManager] 플레이어를 월드 중앙에 배치: {playerPosition}");
    }
    private float FindGroundHeight(int chunkX, int chunkZ, int localX, int localZ)
    {
        var chunkPos = new ChunkPosition(chunkX, 0, chunkZ);

        if (!loadedChunks.TryGetValue(chunkPos, out var chunk))
        {
            // 청크가 로드되지 않은 경우 기본 높이 반환
            return dynamicGenerator.worldHeight * 0.5f * dynamicGenerator.blockOffset.y;
        }

        // 위에서부터 아래로 탐색하여 첫 번째 블록 찾기
        for (int y = dynamicGenerator.worldHeight - 1; y >= 0; y--)
        {
            var block = chunk.GetBlock(localX, y, localZ);
            if (block != null)
            {
                // 블록의 상단 위치 반환
                return (y + 1) * dynamicGenerator.blockOffset.y;
            }
        }

        // 블록이 없는 경우 기본 높이
        return dynamicGenerator.blockOffset.y;
    }
    #region Firebase 기반 월드 로딩

    public async Task LoadWorldFromFirebase()
    {
        Debug.Log("[WorldManager] Firebase에서 월드 로딩 시작!");

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

                // 로딩 진행상황 알림
                OnLoadingProgress?.Invoke(progress);

                await Task.Yield();
            }
        }

        Debug.Log("[WorldManager] Firebase 월드 로딩 완료!");

        // 로딩 완료 알림
        OnLoadingComplete?.Invoke();
        PositionPlayerAtCenter();
    }

    private async Task LoadChunkFromFirebase(ChunkPosition pos)
    {
        // Firebase에서 청크 데이터 로드
        Chunk firebaseChunk = await repo.LoadChunkAsync(pos);

        if (firebaseChunk == null)
        {
            Debug.Log($"[WorldManager] Firebase에 청크 없음, 새로 생성: {pos.X},{pos.Z}");
            firebaseChunk = GenerateDynamicChunk(pos);

            // 새로 생성한 청크를 Firebase에 저장
            await repo.SaveChunkAsync(firebaseChunk);
        }
        else
        {
            Debug.Log($"[WorldManager] Firebase에서 청크 로드 완료: {pos.X},{pos.Z}");
        }

        // 메모리에 로드
        loadedChunks[pos] = firebaseChunk;

        // 동적으로 씬에 렌더링
        await BuildChunkInScene(pos, firebaseChunk);
    }

    #endregion

    #region 동적 청크 생성

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

    #region 동적 렌더링

    private async Task BuildChunkInScene(ChunkPosition pos, Chunk chunk)
    {
        // Chunk 데이터를 월드 데이터로 변환
        string[,,] chunkWorldData = ConvertChunkToWorldData(chunk);

        // DynamicChunkGenerator로 동적 렌더링
        GameObject chunkObject = dynamicGenerator.GenerateDynamicChunk(pos, chunkWorldData);

        // 청크 오브젝트 등록
        chunkObjects[pos] = chunkObject;

        await Task.Yield(); // 프레임 분산
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

    #region 월드 관리

    private void ClearExistingWorld()
    {
        // 기존 청크 오브젝트들 삭제
        foreach (var kvp in chunkObjects)
        {
            if (kvp.Value != null)
            {
                DestroyImmediate(kvp.Value);
            }
        }

        chunkObjects.Clear();
        loadedChunks.Clear();

        Debug.Log("[WorldManager] 기존 월드 정리 완료");
    }

    public async Task SaveWorldToFirebase()
    {
        Debug.Log("[WorldManager] Firebase에 월드 저장 시작!");

        foreach (var kvp in loadedChunks)
        {
            await repo.SaveChunkAsync(kvp.Value);
        }

        Debug.Log("[WorldManager] Firebase 월드 저장 완료!");
    }

    public async Task ReloadWorldFromFirebase()
    {
        Debug.Log("[WorldManager] Firebase에서 월드 다시 로딩!");
        await LoadWorldFromFirebase();
    }

    #endregion

    #region 청크 동적 로딩 (무한 월드용)

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

        Debug.Log($"[WorldManager] 청크 언로드: {pos.X},{pos.Z}");
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

    #region 블록 편집

    public async Task SetBlock(int worldX, int worldY, int worldZ, EBlockType blockType)
    {
        // 월드 좌표를 청크 좌표로 변환
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

            // Firebase에 저장
            await repo.SaveChunkAsync(chunk);

            // 씬에서 청크 다시 렌더링
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
            // 블록 제거 (null로 설정)
            chunk.Blocks[localX, worldY, localZ] = null;

            // Firebase에 저장
            await repo.SaveChunkAsync(chunk);

            // 씬에서 청크 다시 렌더링
            await RefreshChunkInScene(chunkPos);
        }
    }

    private async Task RefreshChunkInScene(ChunkPosition pos)
    {
        // 기존 청크 오브젝트 삭제
        if (chunkObjects.TryGetValue(pos, out var oldChunkObj))
        {
            DestroyImmediate(oldChunkObj);
        }

        // 새로 렌더링
        if (loadedChunks.TryGetValue(pos, out var chunk))
        {
            await BuildChunkInScene(pos, chunk);
        }
    }

    #endregion

    #region 공개 API

    /// <summary>
    /// 현재 로드된 청크 수 반환
    /// </summary>
    public int GetLoadedChunkCount()
    {
        return loadedChunks.Count;
    }

    /// <summary>
    /// 특정 위치의 청크가 로드되어 있는지 확인
    /// </summary>
    public bool IsChunkLoaded(ChunkPosition pos)
    {
        return loadedChunks.ContainsKey(pos);
    }

    /// <summary>
    /// 모든 로드된 청크의 위치 반환
    /// </summary>
    public List<ChunkPosition> GetLoadedChunkPositions()
    {
        return new List<ChunkPosition>(loadedChunks.Keys);
    }

    /// <summary>
    /// 특정 월드 좌표의 블록 타입 반환
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
