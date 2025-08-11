using UnityEngine;

public class ChickenFarmFunction : IBuildingFunction
{
    private BuildingObject _buildingObject;
    
    private float _lastHarvestTime = -1f;
    private const float HARVEST_INTERVAL_HOURS = 15f;
    private bool _isReadyToHarvest = false;
    
    public bool IsReadyToHarvest => _isReadyToHarvest;
    public float HoursUntilReady
    {
        get
        {
            if (_lastHarvestTime < 0) return 0f; // 처음에는 바로 수확 가능
            
            float gameHoursPassed = (GameTimeManager.Instance.TotalGameTime - _lastHarvestTime) / 3600f;
            return Mathf.Max(0f, HARVEST_INTERVAL_HOURS - gameHoursPassed);
        }
    }

    public ChickenFarmFunction(BuildingObject buildingObject)
    {
        _buildingObject = buildingObject;
    }

    public void Execute()
    {
        if (CanHarvest())
        {
            _=InventoryManager.Instance.TryAddItem(EItemType.Egg, 1);
            _lastHarvestTime = GameTimeManager.Instance.TotalGameTime;
            _isReadyToHarvest = false;
            UpdateVfxVisibility();
        }
    }

    public void Update()
    {
        bool previousState = _isReadyToHarvest;
        
        if (_lastHarvestTime < 0)
        {
            _isReadyToHarvest = true; // 처음에는 바로 수확 가능
        }
        else
        {
            float gameHoursPassed = (GameTimeManager.Instance.TotalGameTime - _lastHarvestTime) / 3600f;
            _isReadyToHarvest = gameHoursPassed >= HARVEST_INTERVAL_HOURS;
        }

        // 상태가 변경되었을 때만 VFX 업데이트
        if (previousState != _isReadyToHarvest)
        {
            UpdateVfxVisibility();
        }
    }
    
    private bool CanHarvest()
    {
        if (_lastHarvestTime < 0) return true; // 처음에는 바로 수확 가능
        
        float gameHoursPassed = (GameTimeManager.Instance.TotalGameTime - _lastHarvestTime) / 3600f;
        return gameHoursPassed >= HARVEST_INTERVAL_HOURS;
    }
    
    private void UpdateVfxVisibility()
    {
        if (_buildingObject != null && _buildingObject.VfxPos != null)
        {
            _buildingObject.VfxPos.gameObject.SetActive(_isReadyToHarvest);
        }
    }
    
}
