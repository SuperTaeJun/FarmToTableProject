using UnityEngine;

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
        FadeManager.Instance.FadeToScene("CharacterSelectScene");
    }
}