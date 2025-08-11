using UnityEngine;

public class StoreFunction : IBuildingFunction
{

    public StoreFunction()
    {

    }

    public void Execute()
    {
        OpenStoreUI();
    }

    private void OpenStoreUI()
    {

        FadeManager.Instance.FadeScreenWithEvent(() => PopupManager.Instance.Open(EPopupType.UI_ShopPopup));
    }

    public void Update()
    {

    }
}