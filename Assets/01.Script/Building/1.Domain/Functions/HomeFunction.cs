using UnityEngine;

public class HomeFunction : IBuildingFunction
{
    private BuildingObject _buildingObject;

    public HomeFunction(BuildingObject buildingObject)
    {
        _buildingObject = buildingObject;
    }

    public void Execute()
    {
        FadeManager.Instance.FadeScreenWithEvent(() => GameTimeManager.Instance.GoToNextDayMorning());
    }

    public void Update()
    {
        // HomeFunction은 업데이트가 필요 없음
    }
}