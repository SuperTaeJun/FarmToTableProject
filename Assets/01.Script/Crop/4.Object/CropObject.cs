using UnityEngine;

public class CropObject : MonoBehaviour
{
    [SerializeField] private SO_Crop _data;
    [SerializeField] private GameObject[] _carrotObjects;

    private Crop _crop;
    void Start()
    {
        ChunkPosition chunkPosition = WorldManager.Instance.GetChunkAtWorldPosition(transform.position).Position;
        Vector3 pos = WorldManager.Instance.GetLocalPositionInChunk(transform.position, chunkPosition);
        string chunkId=$"{chunkPosition.X}_{chunkPosition.Y}_{chunkPosition.Z}";

        _crop = CropsManager.Instance.GetCrop(chunkId, pos);

        SetGrowthMesh(_crop.GrowthStage);
    }

    private void Update()
    {
    }

    private void SetGrowthMesh(ECropGrowthStage stage)
    {
        foreach (var obj in _carrotObjects)
        {
            obj.SetActive(false);
        }
        _carrotObjects[(int)stage].SetActive(true);
    }

}
