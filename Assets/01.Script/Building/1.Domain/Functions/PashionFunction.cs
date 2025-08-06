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

    public void Update()
    {
        // PashionFunction은 업데이트가 필요 없음
    }
}