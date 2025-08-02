using UnityEngine;

public class StoreFunction : IBuildingFunction
{
    private BuildingObject _buildingObject;

    public StoreFunction(BuildingObject buildingObject)
    {
        _buildingObject = buildingObject;
    }

    public void Execute()
    {
        OpenStoreUI();
    }

    private void OpenStoreUI()
    {

        FadeManager.Instance.FadeScreenWithEvent(() => PopupManager.Instance.Open(EPopupType.UI_ShopPopup));
    }
}