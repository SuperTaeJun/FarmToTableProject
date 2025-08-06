using UnityEngine;

public class ChickenFarmFunction : IBuildingFunction
{
    private BuildingObject _buildingObject;
    private bool _isActive = false;
    private int _startDay = -1;

    private const int OPERATION_DAYS = 2;

    public void Execute()
    {
        _=InventoryManager.Instance.TryAddItem(EItemType.Egg, 1);
    }

    public void Update()
    {

    }

}
