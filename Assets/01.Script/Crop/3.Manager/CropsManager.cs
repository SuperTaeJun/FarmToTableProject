using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CropsManager : MonoBehaviour
{
    [Header("프리팹")]
    [SerializeField] private List<CropPrefabs> _cropPrefabs;

    public static CropsManager Instance;
    private CropRepository _repo;
    private Dictionary<string, Crop> _crops = new Dictionary<string, Crop>();
    public Dictionary<string, Crop> Crops => _crops;

    [Header("Growth Settings")]
    [SerializeField] private float _baseGrowthRate = 0.1f; // 기본 성장률 (시간당)

    public DebugEvent<Crop> OnCropPlanted = new DebugEvent<Crop>();
    public DebugEvent<Crop> OnCropHarvested = new DebugEvent<Crop>();
    public DebugEvent<Crop> OnCropWatered = new DebugEvent<Crop>();
    public DebugEvent<Crop> OnCropGrowthUpdated = new DebugEvent<Crop>();

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

            GameObject cropObject = GameObject.Instantiate(_cropPrefabs.Find((t) => t.type == crop.Type).Prefab);
            cropObject.transform.position = worldPos;
        }
    }

    public async Task PlantCrop(ECropType cropType, string chunkId, Vector3 worldPos)
    {
        Debug.Log(chunkId + "_" + worldPos);

        Chunk currentChunk = WorldManager.Instance.GetChunkAtWorldPosition(worldPos);
        Vector3 localPos = WorldManager.Instance.GetLocalPositionInChunk(worldPos, currentChunk.Position);
        string cropKey = GetCropKey(chunkId, localPos);

        // 이미 해당 위치에 작물이 있는지 확인
        if (_crops.ContainsKey(cropKey))
        {
            return;
        }

        var newCrop = new Crop(cropType, chunkId, localPos);
        _crops[cropKey] = newCrop;
        GameObject crop = GameObject.Instantiate(_cropPrefabs.Find((t) => t.type == cropType).Prefab);
        crop.transform.position = worldPos;
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

    public async Task WaterCrop(string chunkId, Vector3 position)
    {
        string cropKey = GetCropKey(chunkId, position);

        if (!_crops.TryGetValue(cropKey, out Crop crop))
        {
            return;
        }

        if (!crop.NeedsWater())
        {
            return;
        }

        crop.Water();
        await _repo.WaterCrop(chunkId, position);
        OnCropWatered.Invoke(crop);
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
        InvokeRepeating(nameof(UpdateCropGrowth), 1f, 60f); // 1분마다 성장 업데이트
    }

    private async void UpdateCropGrowth()
    {
        var cropsToUpdate = new List<Crop>();

        foreach (var crop in _crops.Values)
        {
            if (crop.GrowthStage == ECropGrowthStage.Harvest)
                continue;

            float growthRate = _baseGrowthRate;
            //if (crop.IsWatered)
            //    growthRate *= _wateredGrowthMultiplier;

            if (crop.IsWatered && (DateTime.Now - crop.LastWateredTime).TotalHours > 24)
            {
            }

            crop.UpdateGrowth(growthRate / 60f); // 분 단위로 계산
            cropsToUpdate.Add(crop);
        }

        // 성장이 업데이트된 작물들을 DB에 저장
        var updatedChunks = new HashSet<string>();
        foreach (var crop in cropsToUpdate)
        {
            await _repo.UpdateCropGrowth(crop.ChunkId, crop.Position, crop.GrowthProgress);
            updatedChunks.Add(crop.ChunkId);
            OnCropGrowthUpdated.Invoke(crop);
        }

        if (cropsToUpdate.Count > 0)
        {
            Debug.Log($"Updated growth for {cropsToUpdate.Count} crops across {updatedChunks.Count} chunks");
        }
    }

    private string GetCropKey(string chunkId, Vector3 position)
    {
        return $"{chunkId}_{position.x:F1}_{position.y:F1}_{position.z:F1}";
    }

    public void UnloadChunk(string chunkId)
    {
        var keysToRemove = new List<string>();
        foreach (var kvp in _crops)
        {
            if (kvp.Value.ChunkId == chunkId)
                keysToRemove.Add(kvp.Key);
        }

        foreach (var key in keysToRemove)
        {
            _crops.Remove(key);
        }

    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(UpdateCropGrowth));
    }

}

[Serializable]
public struct CropPrefabs
{
    public ECropType type;
    public GameObject Prefab;
}
