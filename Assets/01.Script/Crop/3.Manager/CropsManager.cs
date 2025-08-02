using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CropsManager : MonoBehaviour
{
    public static CropsManager Instance;
    private CropRepository _repo;
    private Dictionary<string, Crop> _crops = new Dictionary<string, Crop>();
    public Dictionary<string, Crop> Crops => _crops;

    // Addressables 캐시
    private Dictionary<ECropType, GameObject> _cachedCropPrefabs = new Dictionary<ECropType, GameObject>();

    [Header("Crop Data")]
    [SerializeField] private List<SO_Crop> _cropDataList;
    private Dictionary<ECropType, SO_Crop> _cropDataDict = new Dictionary<ECropType, SO_Crop>(); // 기본 성장 비율 (시간 기준)

    // 플레이어가 호출하면 발동됨
    public DebugEvent<Crop> OnCropPlanted = new DebugEvent<Crop>();
    public DebugEvent<Crop> OnCropHarvested = new DebugEvent<Crop>();
    public DebugEvent<Crop> OnCropWatered = new DebugEvent<Crop>();

    // 성장 시간에 따라 호출되는 이벤트, 성장 중단 등도 포함
    public DebugEvent<Crop> OnCropGrowthUpdated = new DebugEvent<Crop>();
    public DebugEvent<Crop> OnCropGrowthStopped = new DebugEvent<Crop>(); // 성장이 멈췄을 때

    // 물이 필요한 상태, 수확 가능한 상태 등 추가 알림
    public DebugEvent<Crop> OnCropNeedsWater = new DebugEvent<Crop>(); // 물 필요
    public DebugEvent<Crop> OnCropReadyToHarvest = new DebugEvent<Crop>(); // 수확 가능

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
        InitializeCropDataDict();
    }

    private void InitializeCropDataDict()
    {
        _cropDataDict.Clear();
        foreach (var cropData in _cropDataList)
        {
            if (cropData != null)
            {
                _cropDataDict[cropData.Type] = cropData;
            }
        }
    }

    private void Start()
    {
        StartGrowthUpdate();
    }

    public async Task LoadAllCrops()
    {
        await PreloadAllCropPrefabs();

        var loadedChunks = WorldManager.Instance.LoadedChunks;
        foreach (var chunk in loadedChunks)
        {

            string chunkId = chunk.Key.ToChunkId();
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

            GameObject prefab = GetCropPrefab(crop.Type);
            if (prefab != null)
            {
                GameObject cropObject = GameObject.Instantiate(prefab, gameObject.transform);
                cropObject.transform.position = worldPos;
            }
        }
    }

    public async Task PlantCrop(ECropType cropType, string chunkId, Vector3 worldPos)
    {
        if (!InventoryManager.Instance.HasSeed(cropType)) return;
        bool canUse = await InventoryManager.Instance.TryUseSeed(cropType);
        if (!canUse) return;

        Chunk currentChunk = WorldManager.Instance.GetChunkAtWorldPosition(worldPos);
        Vector3 localPos = WorldManager.Instance.GetLocalPositionInChunk(worldPos, currentChunk.Position);
        string cropKey = GetCropKey(chunkId, localPos);
        if (_crops.ContainsKey(cropKey)) return;

        ICurrentGameTimeProvider timeProvider = GameTimeManager.Instance;
        var newCrop = new Crop(cropType, chunkId, localPos, timeProvider);
        _crops[cropKey] = newCrop;

        GameObject prefab = GetCropPrefab(cropType);
        if (prefab == null)
        {
            Debug.LogError($"농작물 프리팹을 찾을 수 없습니다: {cropType}");
            return;
        }

        GameObject cropObject = GameObject.Instantiate(prefab, gameObject.transform);
        cropObject.transform.position = worldPos;

        await _repo.SaveSingleCrop(newCrop);
        OnCropPlanted.Invoke(newCrop);
    }

    public async Task HarvestCrop(string chunkId, Vector3 position)
    {
        string cropKey = GetCropKey(chunkId, position);
        if (!_crops.TryGetValue(cropKey, out Crop crop)) return;
        if (!crop.CanHarvest()) return;

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
        InvokeRepeating(nameof(UpdateCropGrowth), 1f, 5f); // 1초 후 시작, 5초마다 반복
    }

    public SO_Crop GetCropData(ECropType cropType)
    {
        return _cropDataDict.TryGetValue(cropType, out SO_Crop cropData) ? cropData : null;
    }

    private async Task PreloadAllCropPrefabs()
    {
        Debug.Log("농작물 프리팹 사전 로딩 시작...");

        foreach (ECropType cropType in Enum.GetValues(typeof(ECropType)))
        {
            try
            {
                string address = $"Crop_{cropType}";
                var handle = Addressables.LoadAssetAsync<GameObject>(address);
                GameObject prefab = await handle.Task;

                if (prefab != null)
                {
                    _cachedCropPrefabs[cropType] = prefab;
                    Debug.Log($"농작물 프리팹 로드 완료: {cropType}");
                }
                else
                {
                    Debug.LogWarning($"농작물 프리팹을 찾을 수 없습니다: {address}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"농작물 프리팹 로드 실패: {cropType} - {e.Message}");
            }
        }

        Debug.Log($"농작물 프리팹 사전 로딩 완료: {_cachedCropPrefabs.Count}개");
    }

    private GameObject GetCropPrefab(ECropType cropType)
    {
        return _cachedCropPrefabs.TryGetValue(cropType, out GameObject prefab) ? prefab : null;
    }

    private async void UpdateCropGrowth()
    {
        var cropsToUpdate = new List<Crop>();

        foreach (var crop in _crops.Values)
        {
            if (crop.GrowthStage == ECropGrowthStage.Harvest)
                continue;

            if (crop.GrowthStage != ECropGrowthStage.Seed && !crop.IsWateredForCurrentStage())
            {
                OnCropGrowthStopped.Invoke(crop);
                OnCropNeedsWater.Invoke(crop);
                continue;
            }

            var previousStage = crop.GrowthStage;
            SO_Crop cropData = GetCropData(crop.Type);
            if (cropData == null)
            {
                Debug.LogWarning($"Crop data not found for type: {crop.Type}");
                continue;
            }

            float gameHoursPerSecond = GameTimeManager.Instance != null ?
                24f / GameTimeManager.Instance.secondsPerDay : 1f / 3600f;

            float growthRatePerSecond = gameHoursPerSecond / cropData.GrowthTimeInHours;
            crop.UpdateGrowthWithCropData(growthRatePerSecond * 5f, cropData);
            cropsToUpdate.Add(crop);

            if (previousStage != crop.GrowthStage)
            {
                OnCropGrowthUpdated.Invoke(crop);

                if (crop.GrowthStage == ECropGrowthStage.Harvest)
                {
                    OnCropReadyToHarvest.Invoke(crop);
                }
                else if (crop.GrowthStage == ECropGrowthStage.Vegetative ||
                         crop.GrowthStage == ECropGrowthStage.Mature)
                {
                    OnCropNeedsWater.Invoke(crop);
                }
            }
        }

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

        if (!_crops.TryGetValue(cropKey, out Crop crop)) return;
        if (crop.GrowthStage == ECropGrowthStage.Seed || crop.GrowthStage == ECropGrowthStage.Harvest) return;
        if (crop.IsWateredForCurrentStage()) return;

        ICurrentGameTimeProvider timeProvider = GameTimeManager.Instance;
        crop.WaterCurrentStage(timeProvider);
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

    public Crop GetCropAtWorldPosition(Vector3 worldPosition)
    {
        string chunkId = WorldManager.GetChunkId(worldPosition);
        Chunk currentChunk = WorldManager.Instance.GetChunkAtWorldPosition(worldPosition);
        Vector3 localPos = WorldManager.Instance.GetLocalPositionInChunk(worldPosition, currentChunk.Position);

        return GetCrop(chunkId, localPos);
    }

    public ECropGrowthStage? GetCropGrowthStageAtWorldPosition(Vector3 worldPosition)
    {
        var crop = GetCropAtWorldPosition(worldPosition);
        return crop?.GrowthStage;
    }
}
