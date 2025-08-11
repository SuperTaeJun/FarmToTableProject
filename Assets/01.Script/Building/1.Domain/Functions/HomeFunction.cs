using UnityEngine;

public class HomeFunction : IBuildingFunction
{
    public HomeFunction()
    {
    }

    public void Execute()
    {
        FadeManager.Instance.FadeScreenWithEvent(() => GameTimeManager.Instance.GoToNextDayMorning());
    }

    public void Update()
    {
    }
}