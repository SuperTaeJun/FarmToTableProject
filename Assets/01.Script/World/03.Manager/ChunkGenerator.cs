using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    public static ChunkGenerator Instance { private set; get; }

    [Header("World Settings")]
    public int worldHeight = 16;
    public Vector3 blockOffset = new Vector3(1, 0.5f, 1);

    [Header("Block Prefabs")]
    public GameObject grassPrefab;
    public GameObject dirtPrefab;

    private Dictionary<string, Mesh> blockMeshes;
    private Dictionary<string, Material> blockMaterials;

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
        InitializeBlockAssets();
    }

    private void InitializeBlockAssets()
    {
        blockMeshes = new Dictionary<string, Mesh>();
        blockMaterials = new Dictionary<string, Material>();

        LoadMeshAndMaterial(grassPrefab, "Grass", blockMeshes, blockMaterials);
        LoadMeshAndMaterial(dirtPrefab, "Dirt", blockMeshes, blockMaterials);

    }
    public IEnumerator GenerateDynamicChunkCoroutine(ChunkPosition chunkPos, string[,,] chunkData, Transform parentTransform, System.Action<GameObject> onComplete)
    {
        GameObject chunkParent = new GameObject($"DynamicChunk_{chunkPos.X}_{chunkPos.Z}");
        chunkParent.transform.SetParent(parentTransform);

        Dictionary<string, List<CombineInstance>> blockCombineInstances = new Dictionary<string, List<CombineInstance>>();
        int chunkSize = Chunk.ChunkSize;

        System.Diagnostics.Stopwatch frameTimer = new System.Diagnostics.Stopwatch();

        // 1단계: 블록 데이터 처리 (서브 청크 단위로)
        const int subChunkSize = 2; // subChunkSize x subChunkSize 크기로 청크를 처리

        for (int subX = 0; subX < chunkSize; subX += subChunkSize)
        {
            for (int subZ = 0; subZ < chunkSize; subZ += subChunkSize)
            {
                frameTimer.Restart();

                // 4x4 서브 청크 처리
                int endX = Mathf.Min(subX + subChunkSize, chunkSize);
                int endZ = Mathf.Min(subZ + subChunkSize, chunkSize);

                for (int x = subX; x < endX; x++)
                {
                    for (int z = subZ; z < endZ; z++)
                    {
                        if (x >= chunkData.GetLength(0) || z >= chunkData.GetLength(2))
                            continue;

                        bool topBlockFound = false;
                        for (int y = worldHeight - 1; y >= 0; y--)
                        {
                            string blockName = chunkData[x, y, z];
                            if (blockName == null)
                                continue;

                            bool shouldDraw = false;
                            if (!topBlockFound)
                            {
                                shouldDraw = true;
                                topBlockFound = true;
                            }
                            else
                            {
                                if (IsBlockVisible(chunkData, x, y, z))
                                    shouldDraw = true;
                            }

                            if (shouldDraw)
                            {
                                Vector3 pos = new Vector3(
                                    (chunkPos.X * chunkSize + x) * blockOffset.x,
                                    y * blockOffset.y,
                                    (chunkPos.Z * chunkSize + z) * blockOffset.z
                                );

                                if (!blockMeshes.ContainsKey(blockName))
                                    continue;

                                Mesh blockMesh = blockMeshes[blockName];
                                CombineInstance ci = new CombineInstance
                                {
                                    mesh = blockMesh,
                                    transform = Matrix4x4.TRS(pos, Quaternion.identity, blockOffset)
                                };

                                if (!blockCombineInstances.ContainsKey(blockName))
                                    blockCombineInstances[blockName] = new List<CombineInstance>();
                                blockCombineInstances[blockName].Add(ci);
                            }
                        }
                    }
                }

                // 서브 청크마다 yield
                yield return null;
            }
        }

        // 2단계: 메시 결합 (더 작은 단위로)
        List<GameObject> createdChunkObjects = new List<GameObject>();
        const int maxCombinePerFrame = 150; // 더 작게 설정

        foreach (var kvp in blockCombineInstances)
        {
            string blockName = kvp.Key;
            var combineList = kvp.Value;

            if (combineList.Count == 0)
                continue;

            // 메시를 작은 단위로 분할해서 처리
            for (int i = 0; i < combineList.Count; i += maxCombinePerFrame)
            {
                frameTimer.Restart();

                int count = Mathf.Min(maxCombinePerFrame, combineList.Count - i);
                var subList = combineList.GetRange(i, count);

                Mesh subMesh = new Mesh();
                subMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                subMesh.CombineMeshes(subList.ToArray(), true, true);

                GameObject chunkObj = new GameObject($"Chunk_{chunkPos.X}_{chunkPos.Z}_{blockName}_{i / maxCombinePerFrame}");
                chunkObj.transform.parent = chunkParent.transform;

                var mf = chunkObj.AddComponent<MeshFilter>();
                mf.mesh = subMesh;

                var mr = chunkObj.AddComponent<MeshRenderer>();
                mr.material = blockMaterials[blockName];

                createdChunkObjects.Add(chunkObj);

                // 메시 결합 후 무조건 yield
                yield return null;
            }
        }

        // 3단계: 콜라이더 추가 (배치로 처리)
        const int collidersPerFrame = 3; // 프레임당 콜라이더 생성 수

        for (int i = 0; i < createdChunkObjects.Count; i += collidersPerFrame)
        {
            frameTimer.Restart();

            int endIndex = Mathf.Min(i + collidersPerFrame, createdChunkObjects.Count);
            for (int j = i; j < endIndex; j++)
            {
                var chunkObj = createdChunkObjects[j];
                var meshFilter = chunkObj.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    var mc = chunkObj.AddComponent<MeshCollider>();
                    mc.sharedMesh = meshFilter.mesh;
                    mc.convex = false;
                }
            }

            // 콜라이더 배치 생성 후 yield
            yield return null;
        }

        Debug.Log($"[ChunkGenerator] 청크 {chunkPos.X},{chunkPos.Z} 렌더링 완료 - {blockCombineInstances.Count}개 타입");
        onComplete?.Invoke(chunkParent);
    }
    public GameObject GenerateDynamicChunk(ChunkPosition chunkPos, string[,,] chunkData)
    {
        GameObject chunkParent = new GameObject($"DynamicChunk_{chunkPos.X}_{chunkPos.Z}");
        chunkParent.transform.SetParent(this.transform);

        Dictionary<string, List<CombineInstance>> blockCombineInstances =
            new Dictionary<string, List<CombineInstance>>();

        int chunkSize = Chunk.ChunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                // chunkData는 청크 로컬 좌표계를 사용
                if (x >= chunkData.GetLength(0) || z >= chunkData.GetLength(2))
                    continue;

                bool topBlockFound = false;

                for (int y = worldHeight - 1; y >= 0; y--)
                {
                    // chunkData[x, y, z] - 청크 로컬 좌표 사용
                    string blockName = chunkData[x, y, z];
                    if (blockName == null)
                        continue;

                    bool shouldDraw = false;

                    if (!topBlockFound)
                    {
                        shouldDraw = true;
                        topBlockFound = true;
                    }
                    else
                    {
                        if (IsBlockVisible(chunkData, x, y, z))
                            shouldDraw = true;
                    }

                    if (shouldDraw)
                    {
                        // 월드 좌표로 변환하여 위치 계산
                        Vector3 pos = new Vector3(
                            (chunkPos.X * chunkSize + x) * blockOffset.x,
                            y * blockOffset.y,
                            (chunkPos.Z * chunkSize + z) * blockOffset.z
                        );

                        if (!blockMeshes.ContainsKey(blockName))
                            continue;

                        Mesh blockMesh = blockMeshes[blockName];

                        CombineInstance ci = new CombineInstance
                        {
                            mesh = blockMesh,
                            transform = Matrix4x4.TRS(pos, Quaternion.identity, blockOffset)
                        };

                        if (!blockCombineInstances.ContainsKey(blockName))
                            blockCombineInstances[blockName] = new List<CombineInstance>();

                        blockCombineInstances[blockName].Add(ci);
                    }
                }
            }
        }

        // 블록 타입별로 메시 결합 및 GameObject 생성
        foreach (var kvp in blockCombineInstances)
        {
            string blockName = kvp.Key;
            var combineList = kvp.Value;

            if (combineList.Count == 0)
                continue;

            Mesh combinedMesh = new Mesh();
            combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            combinedMesh.CombineMeshes(combineList.ToArray(), true, true);

            GameObject chunkObj = new GameObject($"Chunk_{chunkPos.X}_{chunkPos.Z}_{blockName}");
            chunkObj.transform.parent = chunkParent.transform;

            var mf = chunkObj.AddComponent<MeshFilter>();
            mf.mesh = combinedMesh;

            var mr = chunkObj.AddComponent<MeshRenderer>();
            mr.material = blockMaterials[blockName];

            var mc = chunkObj.AddComponent<MeshCollider>();
            mc.sharedMesh = combinedMesh;
            mc.convex = false;
        }

        Debug.Log($"[ChunkGenerator] 청크 {chunkPos.X},{chunkPos.Z} 렌더링 완료 - {blockCombineInstances.Count}개 타입");

        return chunkParent;
    }
    public string[,,] ConvertChunkToWorldData(Chunk chunk)
    {
        int chunkSize = Chunk.ChunkSize;
        string[,,] worldData = new string[chunkSize, worldHeight, chunkSize];

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < worldHeight; y++)
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

    private void LoadMeshAndMaterial(GameObject prefab, string name, Dictionary<string, Mesh> blockMeshes, Dictionary<string, Material> blockMaterials)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"{name} 프리팹이 설정되지 않았습니다.");
            return;
        }

        Mesh mesh = null;
        Material mat = null;

        var lodGroup = prefab.GetComponent<LODGroup>();
        if (lodGroup != null)
        {
            var lods = lodGroup.GetLODs();
            if (lods.Length > 0 && lods[0].renderers.Length > 0)
            {
                var renderer = lods[0].renderers[0];
                var mf = renderer.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    mesh = mf.sharedMesh;
                    mat = renderer.sharedMaterial;
                }
            }
        }
        else
        {
            var mf = prefab.GetComponent<MeshFilter>();
            var mr = prefab.GetComponent<MeshRenderer>();

            if (mf && mr)
            {
                mesh = mf.sharedMesh;
                mat = mr.sharedMaterial;
            }
        }

        if (mesh != null && mat != null)
        {
            blockMeshes[name] = mesh;
            blockMaterials[name] = mat;
        }
        else
        {
            Debug.LogWarning($"[{name}] 프리팹에서 Mesh/Material을 찾을 수 없습니다.");
        }
    }
    private bool IsBlockVisible(string[,,] chunkData, int x, int y, int z)
    {
        Vector3Int[] directions = {
            Vector3Int.left,   // -X
            Vector3Int.right,  // +X
            Vector3Int.down,   // -Y
            Vector3Int.up,     // +Y
            Vector3Int.back,   // -Z
            Vector3Int.forward // +Z
        };

        foreach (var dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;
            int nz = z + dir.z;

            // 청크 경계 체크
            if (nx < 0 || nx >= chunkData.GetLength(0) ||
                ny < 0 || ny >= chunkData.GetLength(1) ||
                nz < 0 || nz >= chunkData.GetLength(2))
            {
                // 청크 경계 밖은 비어있다고 가정 (visible)
                return true;
            }

            // 인접 블록이 없으면 visible
            if (chunkData[nx, ny, nz] == null)
                return true;
        }

        return false;
    }
}
