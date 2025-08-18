using UnityEngine;

public class GachaMachineFunction : IBuildingFunction
{
    private BuildingObject _buildingObject;

    private int _gemAmount = 1;

    public GachaMachineFunction(BuildingObject buildingObject)
    {
        _buildingObject = buildingObject;
    }



    public void Execute()
    {
        if (CanPerformGacha())
        {
            FadeManager.Instance.FadeToScene("GachaScene");
        }
        else
        {
            _buildingObject.Player.GetComponent<Player>().GetAbility<PlayerNotificationAbility>().ActiveDialogBox(EPlayerNotificationType.LackOfMoney);
            Debug.Log("가챠를 수행할 수 없습니다. 충분한 재화가 필요합니다.");
        }
    }


    private bool CanPerformGacha()
    {
        return CurrencyManager.Instance.CanAfford(ECurrencyType.Gem, _gemAmount);
    }



    public void Update()
    {

    }
}
