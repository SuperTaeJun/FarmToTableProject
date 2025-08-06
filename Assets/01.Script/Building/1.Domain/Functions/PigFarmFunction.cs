using UnityEngine;

public class PigFarmFunction : IBuildingFunction
{
    public PigFarmFunction()
    {
        
    }
    public void Execute()
    {
        _ = InventoryManager.Instance.TryAddItem(EItemType.Meat, 1);
    }

    public void Update()
    {
        //할거없ㅇ므
    }
}
