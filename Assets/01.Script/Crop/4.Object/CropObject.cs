using UnityEngine;

public class CropObject : MonoBehaviour
{
    [SerializeField] private SO_Crop _data;
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

        // �۹� ������ ��������
        _crop = CropsManager.Instance.GetCrop(_chunkId, _localPosition);

        if (_crop != null)
        {
            _currentStage = _crop.GrowthStage;
            SetGrowthMesh(_currentStage);

            if (_currentStage == ECropGrowthStage.Harvest) ShowReadyToHarvestIndicator();
        }
        else
        {
            Debug.LogWarning($"�۹� �����͸� ã�� �� �����ϴ�: {_chunkId}, {_localPosition}");
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
            Debug.LogWarning("���� �ܰ� ������Ʈ�� �������� �ʾҽ��ϴ�.");
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
            Debug.Log($"���� �ܰ� {stage}�� �����߽��ϴ�.");
            _growthStageObjects[stageIndex].SetActive(true);
        }
        else
        {
            Debug.LogWarning($"���� �ܰ� {stage}�� �ش��ϴ� ������Ʈ�� �����ϴ�.");
        }
    }
    private void OnCropHarvested(Crop harvestedCrop)
    {
        if (IsThisCrop(harvestedCrop))
        {
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

                // �ܰ躰 �α�
                Debug.Log($"�۹� ����: {_currentStage}");
            }
        }
    }

    private void OnCropGrowthStopped(Crop crop)
    {
        if (IsThisCrop(crop))
        {
            Debug.Log($"�۹� ���� �ߴ� - ���� �ʿ��մϴ�: {crop.GrowthStage}");
        }
    }

    private void OnCropNeedsWater(Crop crop)
    {
        if (IsThisCrop(crop))
        {
            ShowNeedsWaterIndicator();
            Debug.Log($"���� �ʿ��մϴ� - ���� �ܰ�: {crop.GrowthStage}");
        }
    }

    private void OnCropReadyToHarvest(Crop crop)
    {
        if (IsThisCrop(crop))
        {
            ShowReadyToHarvestIndicator();
            Debug.Log($"��Ȯ �غ� �Ϸ�!");
        }
    }

    private void OnCropWatered(Crop crop)
    {
        if (IsThisCrop(crop))
        {
            ShowWateredEffect();
            HideNeedsWaterIndicator();
            Debug.Log($"���� �־����ϴ� - �ܰ�: {crop.GrowthStage}");
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
        // �� �� ȿ�� (��ƼŬ, ���� ��)
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
