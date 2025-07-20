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
    [SerializeField] private float _baseGrowthRate = 360f; // 기본 성장률 (시간당)

    //플레이어가 호출하면 발동s
    public DebugEvent<Crop> OnCropPlanted = new DebugEvent<Crop>();
    public DebugEvent<Crop> OnCropHarvested = new DebugEvent<Crop>();
    public DebugEvent<Crop> OnCropWatered = new DebugEvent<Crop>();

    //일정시간마다 성장 업데이트 , 물없으면 성장 스탑
    public DebugEvent<Crop> OnCropGrowthUpdated = new DebugEvent<Crop>();
    public DebugEvent<Crop> OnCropGrowthStopped = new DebugEvent<Crop>(); // 성장 중단
    
    // 농작물한테 보내는 이벤트 이벤트에따라 인디케이터 온오프
    public DebugEvent<Crop> OnCropNeedsWater = new DebugEvent<Crop>(); // 물이 필요할 때
    public DebugEvent<Crop> OnCropReadyToHarvest = new DebugEvent<Crop>(); // 수확 준비됨

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

            GameObject cropObject = GameObject.Instantiate(_cropPrefabs.Find((t) => t.type == crop.Type).Prefab,gameObject.transform);
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
        InvokeRepeating(nameof(UpdateCropGrowth), 1f, 5f); // 1분마다 성장 업데이트
    }
    private async void UpdateCropGrowth()
    {
        var cropsToUpdate = new List<Crop>();

        foreach (var crop in _crops.Values)
        {
            if (crop.GrowthStage == ECropGrowthStage.Harvest)
                continue;

            // 씨앗 단계는 항상 성장 가능, 다른 단계는 물이 필요
            if (crop.GrowthStage != ECropGrowthStage.Seed && !crop.IsWateredForCurrentStage())
            {
                OnCropGrowthStopped.Invoke(crop); // 성장 중단 이벤트
                OnCropNeedsWater.Invoke(crop); // 물 필요 이벤트
                continue; // 성장 업데이트 건너뛰기
            }

            var previousStage = crop.GrowthStage;
            crop.UpdateGrowth(_baseGrowthRate / 3600f); // 초 단위로 계산 (1시간 = 3600초)
            cropsToUpdate.Add(crop);

            // 성장 단계가 변경되었으면 이벤트 발생
            if (previousStage != crop.GrowthStage)
            {
                OnCropGrowthUpdated.Invoke(crop);

                if (crop.GrowthStage == ECropGrowthStage.Harvest)
                {
                    // 수확 가능 상태
                    OnCropReadyToHarvest.Invoke(crop);
                }
                else if (crop.GrowthStage == ECropGrowthStage.Vegetative ||
                         crop.GrowthStage == ECropGrowthStage.Mature)
                {
                    // Vegetative나 Mature 단계에 도달하면 물이 필요
                    OnCropNeedsWater.Invoke(crop);
                }
            }
        }

        // 성장이 업데이트된 작물들을 DB에 저장
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

        // 씨앗 단계나 수확 단계에서는 물을 줄 수 없음
        if (crop.GrowthStage == ECropGrowthStage.Seed ||
            crop.GrowthStage == ECropGrowthStage.Harvest)
        {
            Debug.Log("이 단계에서는 물을 줄 수 없습니다.");
            return;
        }

        // 이미 물을 주었는지 확인
        if (crop.IsWateredForCurrentStage())
        {
            Debug.Log("이미 이 단계에서 물을 주었습니다.");
            return;
        }

        crop.WaterCurrentStage(); // 현재 단계에 물 주기
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

    //외부 공개 메서드
    public Crop GetCropAtWorldPosition(Vector3 worldPosition)
    {
        // 월드 포지션을 청크 ID와 로컬 포지션으로 변환
        string chunkId = WorldManager.GetChunkId(worldPosition);
        Chunk currentChunk = WorldManager.Instance.GetChunkAtWorldPosition(worldPosition);
        Vector3 localPos = WorldManager.Instance.GetLocalPositionInChunk(worldPosition, currentChunk.Position);

        // 기존 GetCrop 메서드 사용
        return GetCrop(chunkId, localPos);
    }

    // 추가로 작물 상태를 확인하는 유틸리티 메서드들도 만들면 좋을 것 같아요
    public bool CanPlantAtWorldPosition(Vector3 worldPosition)
    {
        // 해당 위치에 이미 작물이 있는지 확인
        var existingCrop = GetCropAtWorldPosition(worldPosition);
        if (existingCrop != null) return false;

        // 블럭 타입 확인 (흙이나 잔디인지)
        EBlockType blockType = WorldManager.Instance.GetBlockType(worldPosition);
        return blockType == EBlockType.Dirt || blockType == EBlockType.Grass;
    }

    public bool CanWaterAtWorldPosition(Vector3 worldPosition)
    {
        var crop = GetCropAtWorldPosition(worldPosition);
        if (crop == null) return false;

        return crop.GrowthStage != ECropGrowthStage.Seed &&
               crop.GrowthStage != ECropGrowthStage.Harvest &&
               !crop.IsWateredForCurrentStage();
    }

    public bool CanHarvestAtWorldPosition(Vector3 worldPosition)
    {
        var crop = GetCropAtWorldPosition(worldPosition);
        return crop != null && crop.CanHarvest();
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
