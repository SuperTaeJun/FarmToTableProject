using UnityEngine;
using System.Threading.Tasks;

public class AutoWateringFunction : IBuildingFunction
{
    private BuildingObject _buildingObject;
    private bool _isActive = false;
    private int _startDay = -1;
    private float _lastWateringTime = 0f;

    private const int WATERING_RANGE = 2;
    private const float WATERING_INTERVAL_HOURS = 5f;
    private const int OPERATION_DAYS = 2;

    public AutoWateringFunction(BuildingObject buildingObject)
    {
        _buildingObject = buildingObject;
    }

    public void Execute()
    {
        if (_isActive)
        {
            return;
        }

        StartAutoWatering();
    }

    private void StartAutoWatering()
    {
        _isActive = true;
        _startDay = GameTimeManager.Instance.CurrentDay;
        _lastWateringTime = GameTimeManager.Instance.TotalGameTime;
        _buildingObject.ExecuteInfoTransform.gameObject.SetActive(true);
    }

    public void Update()
    {
        if (!_isActive) return;
        if (GameTimeManager.Instance == null) return;

        // 4일이 지났는지 체크
        if (GameTimeManager.Instance.CurrentDay >= _startDay + OPERATION_DAYS)
        {
            StopAutoWatering();
            return;
        }

        // 급수 인터벌 체크
        CheckAndWater();
    }

    private void CheckAndWater()
    {
        float currentTime = GameTimeManager.Instance.TotalGameTime;
        float gameHoursPerSecond = 24f / GameTimeManager.Instance.secondsPerDay;
        float hoursElapsed = (currentTime - _lastWateringTime) * gameHoursPerSecond;

        if (hoursElapsed >= WATERING_INTERVAL_HOURS)
        {
            WaterCropsInRange();
            _lastWateringTime = currentTime;
        }
    }

    private async void WaterCropsInRange()
    {
        Vector3 centerPos = _buildingObject.transform.position;

        for (int x = -WATERING_RANGE; x <= WATERING_RANGE; x++)
        {
            for (int z = -WATERING_RANGE; z <= WATERING_RANGE; z++)
            {
                //블럭 높이 오프셋 y 0.5
                Vector3 waterPosition = centerPos + new Vector3(x,0.5f, z);

                string chunkId = WorldManager.GetChunkId(waterPosition);

                if (CropsManager.Instance != null)
                {
                    Chunk chunk = WorldManager.Instance.GetChunkAtWorldPosition(waterPosition);
                    Vector3 localPos = WorldManager.Instance.GetLocalPositionInChunk(waterPosition, chunk.Position);
                    await CropsManager.Instance.WaterCrop(chunkId, localPos);
                }

            }
        }
    }

    private void StopAutoWatering()
    {
        _isActive = false;
        _buildingObject.ExecuteInfoTransform.gameObject.SetActive(false);
    }
}
