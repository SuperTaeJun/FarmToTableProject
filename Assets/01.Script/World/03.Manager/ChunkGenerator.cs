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
    [SerializeField] private GameObject _grassPrefab;
    [SerializeField] private GameObject _dirtPrefab;
    [SerializeField] private GameObject _farmlandPrefab;

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

        LoadMeshAndMaterial(_grassPrefab, "Grass", blockMeshes, blockMaterials);
        LoadMeshAndMaterial(_dirtPrefab, "Dirt", blockMeshes, blockMaterials);
        LoadMeshAndMaterial(_farmlandPrefab, "Farmland", blockMeshes, blockMaterials);
    }
    public IEnumerator GenerateDynamicChunkCoroutine(ChunkPosition chunkPos, string[,,] chunkData, Transform parentTransform, System.Action<GameObject> onComplete)
    {
        GameObject chunkParent = new GameObject($"DynamicChunk_{chunkPos.X}_{chunkPos.Z}");
        chunkParent.transform.SetParent(parentTransform);

        Dictionary<string, List<CombineInstance>> blockCombineInstances = new Dictionary<string, List<CombineInstance>>();
        int chunkSize = Chunk.ChunkSize;

        System.Diagnostics.Stopwatch frameTimer = new System.Diagnostics.Stopwatch();

        // 1�ܰ�: ��� ������ ó�� (���� ûũ ������)
        const int subChunkSize = 2; // subChunkSize x subChunkSize ũ��� ûũ�� ó��

        for (int subX = 0; subX < chunkSize; subX += subChunkSize)
        {
            for (int subZ = 0; subZ < chunkSize; subZ += subChunkSize)
            {
                frameTimer.Restart();

                // 4x4 ���� ûũ ó��
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
                                if (IsBlockVisible(chunkData, x, y, z, chunkPos))
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

                // ���� ûũ���� yield
                yield return null;
            }
        }

        // 2�ܰ�: �޽� ���� (�� ���� ������)
        List<GameObject> createdChunkObjects = new List<GameObject>();
        const int maxCombinePerFrame = 150; // �� �۰� ����

        foreach (var kvp in blockCombineInstances)
        {
            string blockName = kvp.Key;
            var combineList = kvp.Value;

            if (combineList.Count == 0)
                continue;

            // �޽ø� ���� ������ �����ؼ� ó��
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

                // �޽� ���� �� ������ yield
                yield return null;
            }
        }

        // 3�ܰ�: �ݶ��̴� �߰� (��ġ�� ó��)
        const int collidersPerFrame = 3; // �����Ӵ� �ݶ��̴� ���� ��

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

            // �ݶ��̴� ��ġ ���� �� yield
            yield return null;
        }

        Debug.Log($"[ChunkGenerator] ûũ {chunkPos.X},{chunkPos.Z} ������ �Ϸ� - {blockCombineInstances.Count}�� Ÿ��");
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
                // chunkData�� ûũ ���� ��ǥ�踦 ���
                if (x >= chunkData.GetLength(0) || z >= chunkData.GetLength(2))
                    continue;

                bool topBlockFound = false;

                for (int y = worldHeight - 1; y >= 0; y--)
                {
                    // chunkData[x, y, z] - ûũ ���� ��ǥ ���
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
                        if (IsBlockVisible(chunkData, x, y, z, chunkPos))
                            shouldDraw = true;
                    }

                    if (shouldDraw)
                    {
                        // ���� ��ǥ�� ��ȯ�Ͽ� ��ġ ���
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

        // ��� Ÿ�Ժ��� �޽� ���� �� GameObject ����
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

        Debug.Log($"[ChunkGenerator] ûũ {chunkPos.X},{chunkPos.Z} ������ �Ϸ� - {blockCombineInstances.Count}�� Ÿ��");

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
            Debug.LogWarning($"{name} �������� �������� �ʾҽ��ϴ�.");
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
            Debug.LogWarning($"[{name}] �����տ��� Mesh/Material�� ã�� �� �����ϴ�.");
        }
    }

    private bool IsBlockVisible(string[,,] chunkData, int x, int y, int z, ChunkPosition currentChunkPos)
    {
        Vector3Int[] directions = {
        Vector3Int.left,    // -X
        Vector3Int.right,   // +X
        Vector3Int.up,      // +Y
        Vector3Int.back,    // -Z
        Vector3Int.forward  // +Z
    };

        int chunkSizeX = chunkData.GetLength(0);
        int chunkSizeY = chunkData.GetLength(1);
        int chunkSizeZ = chunkData.GetLength(2);

        foreach (var dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;
            int nz = z + dir.z;

            // ���� ûũ ������ ���
            if (nx >= 0 && nx < chunkSizeX &&
                ny >= 0 && ny < chunkSizeY &&
                nz >= 0 && nz < chunkSizeZ)
            {
                // ������ ��ġ�� ����� ������ ���� ����� ����
                if (chunkData[nx, ny, nz] == null)
                {
                    return true;
                }
            }
            else
            {
                // ûũ ��踦 ����� ��� ���� ûũ Ȯ��
                if (IsAdjacentBlockEmpty(currentChunkPos, x, y, z, dir))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsAdjacentBlockEmpty(ChunkPosition currentChunkPos, int x, int y, int z, Vector3Int direction)
    {
        // ���� ûũ�� ��ġ ���
        ChunkPosition adjacentChunkPos = currentChunkPos;
        int adjacentX = x + direction.x;
        int adjacentY = y + direction.y;
        int adjacentZ = z + direction.z;

        // ûũ ��踦 �Ѿ�� ��� ûũ ��ǥ ����
        if (adjacentX < 0)
        {
            adjacentChunkPos.X -= 1;
            adjacentX = Chunk.ChunkSize - 1;
        }
        else if (adjacentX >= Chunk.ChunkSize)
        {
            adjacentChunkPos.X += 1;
            adjacentX = 0;
        }

        if (adjacentY < 0)
        {
            adjacentChunkPos.Y -= 1;
            adjacentY = Chunk.ChunkSize - 1;
        }
        else if (adjacentY >= Chunk.ChunkSize)
        {
            adjacentChunkPos.Y += 1;
            adjacentY = 0;
        }

        if (adjacentZ < 0)
        {
            adjacentChunkPos.Z -= 1;
            adjacentZ = Chunk.ChunkSize - 1;
        }
        else if (adjacentZ >= Chunk.ChunkSize)
        {
            adjacentChunkPos.Z += 1;
            adjacentZ = 0;
        }

        // WorldManager�� ���� ���� ûũ�� ��� ���� Ȯ��
        // ���� ûũ�� ���ų� �ش� ��ġ�� ����� ������ true ��ȯ
        return !WorldManager.Instance.HasBlockAt(adjacentChunkPos, adjacentX, adjacentY, adjacentZ);
    }
    //private bool IsBlockVisible(string[,,] chunkData, int x, int y, int z)
    //{
    //    Vector3Int[] directions = {
    //    Vector3Int.left,    // -X
    //    Vector3Int.right,   // +X
    //    Vector3Int.up,      // +Y
    //    Vector3Int.back,    // -Z
    //    Vector3Int.forward  // +Z
    //    // Vector3Int.down ���� (�ظ��� �� �׸�)
    //};

    //    int chunkSizeX = chunkData.GetLength(0);
    //    int chunkSizeY = chunkData.GetLength(1);
    //    int chunkSizeZ = chunkData.GetLength(2);

    //    foreach (var dir in directions)
    //    {
    //        int nx = x + dir.x;
    //        int ny = y + dir.y;
    //        int nz = z + dir.z;

    //        // ûũ ������ ����� ���
    //        if (nx < 0 || nx >= chunkSizeX ||
    //            ny < 0 || ny >= chunkSizeY ||
    //            nz < 0 || nz >= chunkSizeZ)
    //        {
    //            // ûũ ��� ���� ����ִٰ� �����Ͽ� visible
    //            continue;
    //        }

    //        // ������ ��ġ�� ����� ������ ���� ����� ����
    //        if (chunkData[nx, ny, nz] == null)
    //        {
    //            return true;
    //        }
    //    }

    //    // ��� ���� ���������� ������ ����
    //    return false;
    //}
}
