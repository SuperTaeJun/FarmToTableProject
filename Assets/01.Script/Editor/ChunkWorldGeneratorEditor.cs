using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class ChunkWorldGeneratorEditor : EditorWindow
{
    // ============================
    // 필드
    // ============================
    private int chunkCountX = 5;
    private int chunkCountZ = 5;
    private int chunkSize = 16;
    private int worldHeight = 16;

    private Vector3 blockOffset = new Vector3(1, 0.5f, 1);

    private GameObject grassPrefab;
    private GameObject dirtPrefab;

    private string[,,] worldData; // ← 핵심 수정: worldData 필드로 보관

    // ============================
    // Editor Window Open
    // ============================
    [MenuItem("Tools/Chunk World Generator")]
    public static void ShowWindow()
    {
        GetWindow<ChunkWorldGeneratorEditor>("Chunk World Generator");
    }

    // ============================
    // OnGUI
    // ============================
    private void OnGUI()
    {
        GUILayout.Label("Chunk World Generator", EditorStyles.boldLabel);

        chunkCountX = EditorGUILayout.IntField("Chunks X", chunkCountX);
        chunkCountZ = EditorGUILayout.IntField("Chunks Z", chunkCountZ);
        chunkSize = EditorGUILayout.IntField("Chunk Size", chunkSize);
        worldHeight = EditorGUILayout.IntField("World Height", worldHeight);

        blockOffset = EditorGUILayout.Vector3Field("Block Offset", blockOffset);

        grassPrefab = (GameObject)EditorGUILayout.ObjectField("Grass Prefab", grassPrefab, typeof(GameObject), false);
        dirtPrefab = (GameObject)EditorGUILayout.ObjectField("Dirt Prefab", dirtPrefab, typeof(GameObject), false);

        GUILayout.Space(10);

        if (GUILayout.Button("Generate World"))
        {
            GenerateWorld();
        }

        if (GUILayout.Button("Save World To JSON"))
        {
            SaveWorldToJson();
        }
    }

    // ============================
    // GenerateWorld
    // ============================
    private void GenerateWorld()
    {
        if (grassPrefab == null || dirtPrefab == null)
        {
            Debug.LogError("Grass Prefab과 Dirt Prefab을 설정하세요!");
            return;
        }

        int worldBlockWidth = chunkCountX * chunkSize;
        int worldBlockDepth = chunkCountZ * chunkSize;

        // 이전 ChunkWorld 삭제
        var existingWorld = GameObject.Find("ChunkWorld");
        if (existingWorld != null)
        {
            Object.DestroyImmediate(existingWorld);
        }

        GameObject worldParent = new GameObject("ChunkWorld");

        var blockMeshes = new Dictionary<string, Mesh>();
        var blockMaterials = new Dictionary<string, Material>();

        LoadMeshAndMaterial(grassPrefab, "Grass", blockMeshes, blockMaterials);
        LoadMeshAndMaterial(dirtPrefab, "Dirt", blockMeshes, blockMaterials);

        // World Data 생성 → 저장할 worldData
        worldData = new string[worldBlockWidth, worldHeight, worldBlockDepth];

        for (int x = 0; x < worldBlockWidth; x++)
        {
            for (int z = 0; z < worldBlockDepth; z++)
            {
                int height = GetHeight(x, z);

                for (int y = 0; y < height; y++)
                {
                    worldData[x, y, z] = (y == height - 1) ? "Grass" : "Dirt";
                }
            }
        }

        for (int cx = 0; cx < chunkCountX; cx++)
        {
            for (int cz = 0; cz < chunkCountZ; cz++)
            {
                GenerateChunk(worldData, cx, cz, blockMeshes, blockMaterials, worldParent.transform);
            }
        }

        Debug.Log($"✅ 에디터에서 월드 생성 완료! ({chunkCountX} x {chunkCountZ} 청크)");
    }

    // ============================
    // Load Mesh and Material
    // ============================
    private void LoadMeshAndMaterial(GameObject prefab, string name, Dictionary<string, Mesh> blockMeshes, Dictionary<string, Material> blockMaterials)
    {
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

    // ============================
    // GetHeight (Perlin Noise)
    // ============================
    private int GetHeight(int x, int z)
    {
        float frequency = 0.02f;
        float amplitude = worldHeight * 0.4f;

        float noise = Mathf.PerlinNoise(x * frequency, z * frequency);

        int baseHeight = Mathf.FloorToInt(noise * amplitude) + (int)(worldHeight * 0.3f);

        return Mathf.Clamp(baseHeight, 1, worldHeight);
    }

    // ============================
    // GenerateChunk
    // ============================
    private void GenerateChunk(
        string[,,] worldData,
        int chunkX,
        int chunkZ,
        Dictionary<string, Mesh> blockMeshes,
        Dictionary<string, Material> blockMaterials,
        Transform parent)
    {
        Dictionary<string, List<CombineInstance>> blockCombineInstances = new Dictionary<string, List<CombineInstance>>();

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                int worldX = chunkX * chunkSize + x;
                int worldZ = chunkZ * chunkSize + z;

                if (worldX >= worldData.GetLength(0) || worldZ >= worldData.GetLength(2))
                    continue;

                bool topBlockFound = false;

                for (int y = worldHeight - 1; y >= 0; y--)
                {
                    string blockName = worldData[worldX, y, worldZ];
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
                        if (IsBlockVisible(worldData, worldX, y, worldZ))
                            shouldDraw = true;
                    }

                    if (shouldDraw)
                    {
                        Vector3 pos = new Vector3(
                            worldX * blockOffset.x,
                            y * blockOffset.y,
                            worldZ * blockOffset.z
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

        foreach (var kvp in blockCombineInstances)
        {
            string blockName = kvp.Key;
            var combineList = kvp.Value;

            if (combineList.Count == 0)
                continue;

            Mesh combinedMesh = new Mesh();
            combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            combinedMesh.CombineMeshes(combineList.ToArray(), true, true);

            GameObject chunkObj = new GameObject($"Chunk_{chunkX}_{chunkZ}_{blockName}");
            chunkObj.transform.parent = parent;

            var mf = chunkObj.AddComponent<MeshFilter>();
            mf.mesh = combinedMesh;

            var mr = chunkObj.AddComponent<MeshRenderer>();
            mr.material = blockMaterials[blockName];

            var mc = chunkObj.AddComponent<MeshCollider>();
            mc.sharedMesh = combinedMesh;
            mc.convex = false;
        }
    }

    // ============================
    // IsBlockVisible
    // ============================
    private bool IsBlockVisible(string[,,] worldData, int x, int y, int z)
    {
        Vector3Int[] directions = {
            Vector3Int.left,
            Vector3Int.right,
            Vector3Int.down,
            Vector3Int.up,
            Vector3Int.back,
            Vector3Int.forward
        };

        foreach (var dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;
            int nz = z + dir.z;

            if (nx < 0 || nx >= worldData.GetLength(0) ||
                ny < 0 || ny >= worldData.GetLength(1) ||
                nz < 0 || nz >= worldData.GetLength(2))
                continue;

            if (worldData[nx, ny, nz] == null)
                return true;
        }

        return false;
    }

    // ============================
    // SaveWorldToJson
    // ============================
    private void SaveWorldToJson()
    {
        if (worldData == null)
        {
            Debug.LogError("먼저 Generate World를 실행하세요.");
            return;
        }

        string folderPath = "Assets/VoxelMaps";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        for (int cx = 0; cx < chunkCountX; cx++)
        {
            for (int cz = 0; cz < chunkCountZ; cz++)
            {
                SaveChunkToJson(worldData, cx, cz, folderPath);
            }
        }

        Debug.Log("✅ JSON 저장 완료!");
    }

    // ============================
    // SaveChunkToJson
    // ============================
    private void SaveChunkToJson(string[,,] worldData, int chunkX, int chunkZ, string folderPath)
    {
        var dto = new JsonWorldDocumentDto
        {
            ChunkX = chunkX,
            ChunkY = 0,
            ChunkZ = chunkZ,
            Blocks = new List<JsonBlockDto>()
        };

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    int worldX = chunkX * chunkSize + x;
                    int worldZ = chunkZ * chunkSize + z;

                    if (worldX >= worldData.GetLength(0) ||
                        worldZ >= worldData.GetLength(2))
                        continue;

                    string blockName = worldData[worldX, y, worldZ];
                    if (blockName == null)
                        continue;

                    int blockType = blockName == "Grass" ? 1 : 0;

                    dto.Blocks.Add(new JsonBlockDto
                    {
                        Type = blockType,
                        X = x,
                        Y = y,
                        Z = z
                    });
                }
            }
        }

        string json = JsonUtility.ToJson(dto, true);
        string path = Path.Combine(folderPath, $"{chunkX}_{chunkZ}.json");
        File.WriteAllText(path, json);
    }
}
[System.Serializable]
public class JsonWorldDocumentDto
{
    public int ChunkX;
    public int ChunkY;
    public int ChunkZ;
    public List<JsonBlockDto> Blocks;
}

[System.Serializable]
public class JsonBlockDto
{
    public int Type;
    public int X;
    public int Y;
    public int Z;
}