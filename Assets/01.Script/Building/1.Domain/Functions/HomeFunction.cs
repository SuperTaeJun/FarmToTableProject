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
}