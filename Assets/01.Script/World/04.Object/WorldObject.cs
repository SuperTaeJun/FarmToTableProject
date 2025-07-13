using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class BlockPrefabInfo
{
    public EBlockType BlockType;
    public GameObject Prefab;
}
public class WorldObject : MonoBehaviour
{
    [Header("Block Prefabs")]
    public BlockPrefabInfo[] blockTypes;


    [Header("Block Offset")]
    public Vector3 blockOffset = new Vector3(1, 1, 1);

    private Dictionary<EBlockType, Mesh> blockMeshes;
    private Dictionary<EBlockType, Material> blockMaterials;

    private void Awake()
    {
        PreloadBlockMeshes();
    }

    public void BuildChunk(Chunk chunk)
    {
        Dictionary<EBlockType, List<CombineInstance>> combineInstances
            = new Dictionary<EBlockType, List<CombineInstance>>();

        for (int x = 0; x < Chunk.ChunkSize; x++)
        {
            for (int y = 0; y < Chunk.ChunkSize; y++)
            {
                for (int z = 0; z < Chunk.ChunkSize; z++)
                {
                    var block = chunk.GetBlock(x, y, z);
                    if (block == null)
                        continue;

                    var blockType = (EBlockType)block.Type;

                    Vector3 pos = new Vector3(
                        chunk.Position.X * Chunk.ChunkSize * blockOffset.x + block.Position.X * blockOffset.x,
                        chunk.Position.Y * Chunk.ChunkSize * blockOffset.y + block.Position.Y * blockOffset.y,
                        chunk.Position.Z * Chunk.ChunkSize * blockOffset.z + block.Position.Z * blockOffset.z
                    );

                    var mesh = blockMeshes[blockType];

                    var ci = new CombineInstance
                    {
                        mesh = mesh,
                        transform = Matrix4x4.TRS(pos, Quaternion.identity, blockOffset)
                    };

                    if (!combineInstances.ContainsKey(blockType))
                        combineInstances[blockType] = new List<CombineInstance>();

                    combineInstances[blockType].Add(ci);
                }
            }
        }

        // BlockType 별로 Chunk GameObject 생성
        foreach (var kvp in combineInstances)
        {
            var combinedMesh = new Mesh
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };
            combinedMesh.CombineMeshes(kvp.Value.ToArray(), true, true);

            GameObject go = new GameObject($"{chunk.Position.X}_{chunk.Position.Z}_{kvp.Key}");
            go.transform.SetParent(this.transform);

            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = combinedMesh;

            var mr = go.AddComponent<MeshRenderer>();
            mr.material = blockMaterials[kvp.Key];

            var mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = combinedMesh;
        }
    }

    private void PreloadBlockMeshes()
    {
        blockMeshes = new Dictionary<EBlockType, Mesh>();
        blockMaterials = new Dictionary<EBlockType, Material>();

        foreach (var block in blockTypes)
        {
            Mesh mesh = null;
            Material mat = null;

            var lodGroup = block.Prefab.GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                var lods = lodGroup.GetLODs();
                if (lods.Length > 0 && lods[0].renderers.Length > 0)
                {
                    var renderer = lods[0].renderers[0];
                    var meshFilter = renderer.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        mesh = meshFilter.sharedMesh;
                        mat = renderer.sharedMaterial;
                    }
                }
            }
            else
            {
                var mf = block.Prefab.GetComponent<MeshFilter>();
                var mr = block.Prefab.GetComponent<MeshRenderer>();

                if (mf && mr)
                {
                    mesh = mf.sharedMesh;
                    mat = mr.sharedMaterial;
                }
            }

            if (mesh != null && mat != null)
            {
                blockMeshes[block.BlockType] = mesh;
                blockMaterials[block.BlockType] = mat;
            }
            else
            {
                Debug.LogWarning($"[{block.BlockType}] Mesh/Material 찾을 수 없음.");
            }
        }
    }
}
