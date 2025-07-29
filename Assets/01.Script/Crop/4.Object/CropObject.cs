using UnityEngine;

public class CropObject : MonoBehaviour
{
    [SerializeField] private GameObject[] _growthStageObjects;

    private Crop _crop;
    private string _chunkId;
    private Vector3 _localPosition;
    private ECropGrowthStage _currentStage;

    [SerializeField] GameObject _wateringIndicator;
    [SerializeField] GameObject _harvestIndicator;

    void Start()
    {
        InitializeCrop();
        SubscribeToEvents();
    }

    private void InitializeCrop()
    {
        // 청크 정보 설정
        ChunkPosition chunkPosition = WorldManager.Instance.GetChunkAtWorldPosition(transform.position).Position;
        _localPosition = WorldManager.Instance.GetLocalPositionInChunk(transform.position, chunkPosition);
        _chunkId = $"{chunkPosition.X}_{chunkPosition.Y}_{chunkPosition.Z}";

        // 작물 데이터를 CropsManager에서 불러옴
        _crop = CropsManager.Instance.GetCrop(_chunkId, _localPosition);

        if (_crop != null)
        {
            _currentStage = _crop.GrowthStage;
            SetGrowthMesh(_currentStage);

            if (_currentStage == ECropGrowthStage.Harvest) ShowReadyToHarvestIndicator();
        }
        else
        {
            Debug.LogWarning($"작물 데이터를 찾을 수 없습니다: {_chunkId}, {_localPosition}");
        }
    }

    private void SubscribeToEvents()
    {
        if (CropsManager.Instance != null)
        {
            CropsManager.Instance.OnCropGrowthUpdated.AddListener(OnCropGrowthUpdated);
            CropsManager.Instance.OnCropGrowthStopped.AddListener(OnCropGrowthStopped);
            CropsManager.Instance.OnCropNeedsWater.AddListener(OnCropNeedsWater);
            CropsManager.Instance.OnCropReadyToHarvest.AddListener(OnCropReadyToHarvest);
            CropsManager.Instance.OnCropWatered.AddListener(OnCropWatered);
            CropsManager.Instance.OnCropHarvested.AddListener(OnCropHarvested);
        }
    }

    private void SetGrowthMesh(ECropGrowthStage stage)
    {
        if (_growthStageObjects == null || _growthStageObjects.Length == 0)
        {
            Debug.LogWarning("성장 단계 오브젝트가 설정되어 있지 않습니다.");
            return;
        }

        foreach (var obj in _growthStageObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        int stageIndex = (int)stage;
        if (stageIndex < _growthStageObjects.Length && _growthStageObjects[stageIndex] != null)
        {
            Debug.Log($"성장 단계 {stage}를 설정했습니다.");
            _growthStageObjects[stageIndex].SetActive(true);
        }
        else
        {
            Debug.LogWarning($"성장 단계 {stage}에 해당하는 오브젝트가 존재하지 않습니다.");
        }
    }

    private void OnCropHarvested(Crop harvestedCrop)
    {
        if (IsThisCrop(harvestedCrop))
        {
            _ = InventoryManager.Instance.AddCropToInventory(harvestedCrop.Type);
            Destroy(gameObject);
        }
    }

    private void OnCropGrowthUpdated(Crop updatedCrop)
    {
        if (IsThisCrop(updatedCrop))
        {
            if (_currentStage != updatedCrop.GrowthStage)
            {
                _currentStage = updatedCrop.GrowthStage;
                SetGrowthMesh(_currentStage);

                // 성장 단계 로그
                Debug.Log($"작물 성장 단계: {_currentStage}");
            }
        }
    }

    private void OnCropGrowthStopped(Crop crop)
    {
        if (IsThisCrop(crop))
        {
            Debug.Log($"작물 성장이 멈춤 - 물 주기 필요: {crop.GrowthStage}");
        }
    }

    private void OnCropNeedsWater(Crop crop)
    {
        if (IsThisCrop(crop))
        {
            ShowNeedsWaterIndicator();
            Debug.Log($"물 주기 필요 - 현재 성장 단계: {crop.GrowthStage}");
        }
    }

    private void OnCropReadyToHarvest(Crop crop)
    {
        if (IsThisCrop(crop))
        {
            ShowReadyToHarvestIndicator();
            Debug.Log($"수확 준비 완료!");
        }
    }

    private void OnCropWatered(Crop crop)
    {
        if (IsThisCrop(crop))
        {
            ShowWateredEffect();
            HideNeedsWaterIndicator();
            Debug.Log($"물 주기 완료 - 현재 성장 단계: {crop.GrowthStage}");
        }
    }

    private bool IsThisCrop(Crop crop)
    {
        return crop.ChunkId == _chunkId && crop.Position == _localPosition;
    }

    private void ShowNeedsWaterIndicator()
    {
        _wateringIndicator.SetActive(true);
    }

    private void HideNeedsWaterIndicator()
    {
        _wateringIndicator.SetActive(false);
    }

    private void ShowWateredEffect()
    {
        // 물 준 효과 (파티클, 사운드 등)
    }

    private void ShowReadyToHarvestIndicator()
    {
        _harvestIndicator.SetActive(true);
    }

    private void OnDestroy()
    {
        if (CropsManager.Instance != null)
        {
            CropsManager.Instance.OnCropGrowthUpdated.RemoveListener(OnCropGrowthUpdated);
            CropsManager.Instance.OnCropGrowthStopped.RemoveListener(OnCropGrowthStopped);
            CropsManager.Instance.OnCropNeedsWater.RemoveListener(OnCropNeedsWater);
            CropsManager.Instance.OnCropReadyToHarvest.RemoveListener(OnCropReadyToHarvest);
            CropsManager.Instance.OnCropWatered.RemoveListener(OnCropWatered);
        }
    }
}
