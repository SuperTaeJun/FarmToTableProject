using System.Collections;
using System;
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
                        if (IsBlockVisible(chunkData, x, y, z))
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

            // ûũ ��� üũ
            if (nx < 0 || nx >= chunkData.GetLength(0) ||
                ny < 0 || ny >= chunkData.GetLength(1) ||
                nz < 0 || nz >= chunkData.GetLength(2))
            {
                // ûũ ��� ���� ����ִٰ� ���� (visible)
                return true;
            }

            // ���� ����� ������ visible
            if (chunkData[nx, ny, nz] == null)
                return true;
        }

        return false;
    }
}
