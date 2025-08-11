using UnityEngine;

public class PashionFunction : IBuildingFunction
{
    public PashionFunction()
    {

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