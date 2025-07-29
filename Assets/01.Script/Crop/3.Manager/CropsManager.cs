using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CropsManager : MonoBehaviour
{
    [Header("������")]
    [SerializeField] private List<CropPrefabs> _cropPrefabs;

    public static CropsManager Instance;
    private CropRepository _repo;
    private Dictionary<string, Crop> _crops = new Dictionary<string, Crop>();
    public Dictionary<string, Crop> Crops => _crops;

    [Header("Growth Settings")]
    [SerializeField] private float _baseGrowthRate = 360f; // �⺻ ����� (�ð���)

    //�÷��̾ ȣ���ϸ� �ߵ�s
    public DebugEvent<Crop> OnCropPlanted = new DebugEvent<Crop>();
    public DebugEvent<Crop> OnCropHarvested = new DebugEvent<Crop>();
    public DebugEvent<Crop> OnCropWatered = new DebugEvent<Crop>();

    //�����ð����� ���� ������Ʈ , �������� ���� ��ž
    public DebugEvent<Crop> OnCropGrowthUpdated = new DebugEvent<Crop>();
    public DebugEvent<Crop> OnCropGrowthStopped = new DebugEvent<Crop>(); // ���� �ߴ�

    // ���۹����� ������ �̺�Ʈ �̺�Ʈ������ �ε������� �¿���
    public DebugEvent<Crop> OnCropNeedsWater = new DebugEvent<Crop>(); // ���� �ʿ��� ��
    public DebugEvent<Crop> OnCropReadyToHarvest = new DebugEvent<Crop>(); // ��Ȯ �غ��

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
            return;
        }
        _repo = new CropRepository();
    }

    private async void Start()
    {
        await LoadAllCrops();
        StartGrowthUpdate();
    }

    private async Task LoadAllCrops()
    {
        var loadedChunks = WorldManager.Instance.LoadedChunkPositions;
        foreach (var chunkPos in loadedChunks)
        {
            string chunkId = $"{chunkPos.X}_{chunkPos.Y}_{chunkPos.Z}";
            await LoadCropsFromChunk(chunkId);
        }
    }

    public async Task LoadCropsFromChunk(string chunkId)
    {
        var crops = await _repo.LoadCropsByChunk(chunkId);
        foreach (var crop in crops)
        {
            string cropKey = GetCropKey(crop.ChunkId, crop.Position);
            _crops[cropKey] = crop;
            Vector3 worldPos = WorldManager.Instance.GetWorldPositionFromChunkLocal(chunkId, crop.Position);

            GameObject cropObject = GameObject.Instantiate(_cropPrefabs.Find((t) => t.type == crop.Type).Prefab, gameObject.transform);
            cropObject.transform.position = worldPos;
        }
    }

    public async Task PlantCrop(ECropType cropType, string chunkId, Vector3 worldPos)
    {

        if (!InventoryManager.Instance.HasSeed(cropType)) return;
        bool canUse = await InventoryManager.Instance.TryUseSeed(cropType);
        if (!canUse) return;


        Debug.Log(chunkId + "_" + worldPos);

        Chunk currentChunk = WorldManager.Instance.GetChunkAtWorldPosition(worldPos);
        Vector3 localPos = WorldManager.Instance.GetLocalPositionInChunk(worldPos, currentChunk.Position);
        string cropKey = GetCropKey(chunkId, localPos);

        // �̹� �ش� ��ġ�� �۹��� �ִ��� Ȯ��
        if (_crops.ContainsKey(cropKey))
        {
            return;
        }

        var newCrop = new Crop(cropType, chunkId, localPos);
        _crops[cropKey] = newCrop;
        GameObject cropObject = GameObject.Instantiate(_cropPrefabs.Find((t) => t.type == cropType).Prefab, gameObject.transform);
        cropObject.transform.position = worldPos;
        await _repo.SaveSingleCrop(newCrop);
        OnCropPlanted.Invoke(newCrop);
    }

    public async Task HarvestCrop(string chunkId, Vector3 position)
    {
        string cropKey = GetCropKey(chunkId, position);

        if (!_crops.TryGetValue(cropKey, out Crop crop))
        {
            return;
        }

        if (!crop.CanHarvest())
        {
            return;
        }

        _crops.Remove(cropKey);
        await _repo.RemoveCrop(chunkId, position);
        OnCropHarvested.Invoke(crop);
    }

    public Crop GetCrop(string chunkId, Vector3 position)
    {
        string cropKey = GetCropKey(chunkId, position);
        return _crops.TryGetValue(cropKey, out Crop crop) ? crop : null;
    }

    public List<Crop> GetCropsInChunk(string chunkId)
    {
        var result = new List<Crop>();
        foreach (var crop in _crops.Values)
        {
            if (crop.ChunkId == chunkId)
                result.Add(crop);
        }
        return result;
    }

    public async Task SaveCropsInChunk(string chunkId)
    {
        var cropsInChunk = GetCropsInChunk(chunkId);
        await _repo.SaveCrops(chunkId, cropsInChunk);
    }

    private void StartGrowthUpdate()
    {
        InvokeRepeating(nameof(UpdateCropGrowth), 1f, 5f); // 1�и��� ���� ������Ʈ
    }
    private async void UpdateCropGrowth()
    {
        var cropsToUpdate = new List<Crop>();

        foreach (var crop in _crops.Values)
        {
            if (crop.GrowthStage == ECropGrowthStage.Harvest)
                continue;

            // ���� �ܰ�� �׻� ���� ����, �ٸ� �ܰ�� ���� �ʿ�
            if (crop.GrowthStage != ECropGrowthStage.Seed && !crop.IsWateredForCurrentStage())
            {
                OnCropGrowthStopped.Invoke(crop); // ���� �ߴ� �̺�Ʈ
                OnCropNeedsWater.Invoke(crop); // �� �ʿ� �̺�Ʈ
                continue; // ���� ������Ʈ �ǳʶٱ�
            }

            var previousStage = crop.GrowthStage;
            crop.UpdateGrowth(_baseGrowthRate / 3600f); // �� ������ ��� (1�ð� = 3600��)
            cropsToUpdate.Add(crop);

            // ���� �ܰ谡 ����Ǿ����� �̺�Ʈ �߻�
            if (previousStage != crop.GrowthStage)
            {
                OnCropGrowthUpdated.Invoke(crop);

                if (crop.GrowthStage == ECropGrowthStage.Harvest)
                {
                    // ��Ȯ ���� ����
                    OnCropReadyToHarvest.Invoke(crop);
                }
                else if (crop.GrowthStage == ECropGrowthStage.Vegetative ||
                         crop.GrowthStage == ECropGrowthStage.Mature)
                {
                    // Vegetative�� Mature �ܰ迡 �����ϸ� ���� �ʿ�
                    OnCropNeedsWater.Invoke(crop);
                }
            }
        }

        // ������ ������Ʈ�� �۹����� DB�� ����
        var updatedChunks = new HashSet<string>();
        foreach (var crop in cropsToUpdate)
        {
            await _repo.UpdateCropGrowth(crop.ChunkId, crop.Position, crop.GrowthProgress);
            updatedChunks.Add(crop.ChunkId);
        }

        if (cropsToUpdate.Count > 0)
        {
            Debug.Log($"Updated growth for {cropsToUpdate.Count} crops across {updatedChunks.Count} chunks");
        }
    }
    public async Task WaterCrop(string chunkId, Vector3 localPosition)
    {
        string cropKey = GetCropKey(chunkId, localPosition);

        if (!_crops.TryGetValue(cropKey, out Crop crop))
        {
            return;
        }

        // ���� �ܰ質 ��Ȯ �ܰ迡���� ���� �� �� ����
        if (crop.GrowthStage == ECropGrowthStage.Seed ||
            crop.GrowthStage == ECropGrowthStage.Harvest)
        {
            Debug.Log("�� �ܰ迡���� ���� �� �� �����ϴ�.");
            return;
        }

        // �̹� ���� �־����� Ȯ��
        if (crop.IsWateredForCurrentStage())
        {
            Debug.Log("�̹� �� �ܰ迡�� ���� �־����ϴ�.");
            return;
        }

        crop.WaterCurrentStage(); // ���� �ܰ迡 �� �ֱ�
        await _repo.WaterCrop(chunkId, localPosition);
        OnCropWatered.Invoke(crop);
    }
    private string GetCropKey(string chunkId, Vector3 localPosition)
    {
        return $"{chunkId}_{localPosition.x:F1}_{localPosition.y:F1}_{localPosition.z:F1}";
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(UpdateCropGrowth));
    }

    //�ܺ� ���� �޼���
    public Crop GetCropAtWorldPosition(Vector3 worldPosition)
    {
        // ���� �������� ûũ ID�� ���� ���������� ��ȯ
        string chunkId = WorldManager.GetChunkId(worldPosition);
        Chunk currentChunk = WorldManager.Instance.GetChunkAtWorldPosition(worldPosition);
        Vector3 localPos = WorldManager.Instance.GetLocalPositionInChunk(worldPosition, currentChunk.Position);

        // ���� GetCrop �޼��� ���
        return GetCrop(chunkId, localPos);
    }

    public ECropGrowthStage? GetCropGrowthStageAtWorldPosition(Vector3 worldPosition)
    {
        var crop = GetCropAtWorldPosition(worldPosition);
        return crop?.GrowthStage;
    }

}

[Serializable]
public struct CropPrefabs
{
    public ECropType type;
    public GameObject Prefab;
}
