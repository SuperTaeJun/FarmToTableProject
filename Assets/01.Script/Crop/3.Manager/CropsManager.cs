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
    private Dictionary<ECropType, SO_Crop> _cropDataDict = new Dictionary<ECropType, SO_Crop>(); // �⺻ ����� (�ð���)

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

    private async void Start()
    {
        await PreloadAllCropPrefabs();
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
    
    public SO_Crop GetCropData(ECropType cropType)
    {
        return _cropDataDict.TryGetValue(cropType, out SO_Crop cropData) ? cropData : null;
    }
    
    private async Task PreloadAllCropPrefabs()
    {
        Debug.Log("농작물 프리팹 사전 로딩 시작...");
        
        foreach (ECropType cropType in System.Enum.GetValues(typeof(ECropType)))
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
            catch (System.Exception e)
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

            // ���� �ܰ�� �׻� ���� ����, �ٸ� �ܰ�� ���� �ʿ�
            if (crop.GrowthStage != ECropGrowthStage.Seed && !crop.IsWateredForCurrentStage())
            {
                OnCropGrowthStopped.Invoke(crop); // ���� �ߴ� �̺�Ʈ
                OnCropNeedsWater.Invoke(crop); // �� �ʿ� �̺�Ʈ
                continue; // ���� ������Ʈ �ǳʶٱ�
            }

            var previousStage = crop.GrowthStage;
            // SO_Crop 데이터에서 성장 시간 가져오기
            SO_Crop cropData = GetCropData(crop.Type);
            if (cropData == null)
            {
                Debug.LogWarning($"Crop data not found for type: {crop.Type}");
                continue;
            }
            
            // GameTimeManager의 시간을 기반으로 성장률 계산
            float gameHoursPerSecond = GameTimeManager.Instance != null ? 
                24f / GameTimeManager.Instance.secondsPerDay : 1f / 3600f;
            
            float growthRatePerSecond = gameHoursPerSecond / cropData.GrowthTimeInHours;
            crop.UpdateGrowthWithCropData(growthRatePerSecond * 5f, cropData); // 5초마다 업데이트하므로 5를 곱함
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

