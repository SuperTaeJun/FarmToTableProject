using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PashionFunction : IBuildingFunction
{
    private BuildingObject _buildingObject;

    public PashionFunction(BuildingObject buildingObject)
    {
        _buildingObject = buildingObject;
    }

    public void Execute()
    {
        OpenPashionUI();
    }

    private void OpenPashionUI()
    {
        PlayerDataHolder.Instance.SavedData(_buildingObject.Player.position, _buildingObject.Player.rotation);

        FadeManager.Instance.FadeToScene("CharacterSelectScene");
    }
}