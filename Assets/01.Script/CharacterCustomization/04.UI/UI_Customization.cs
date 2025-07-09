using UnityEngine;
using UnityEngine.UI;
public class CustomizationUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button HairButton;
    public Button HatButton;
    public Button TopButton;


    public Button SaveButton;
    private void Start()
    {
        // ��ư ����
        HairButton.onClick.AddListener(() => OnHairButtonClicked());
        SaveButton.onClick.AddListener(() => OnSaveButtonClicked());
        //HatButton.onClick.AddListener(() => OnHatButtonClicked());
        //TopButton.onClick.AddListener(() => OnTopButtonClicked());
    }
    private async void OnSaveButtonClicked()
    {
        await CustomizationManager.Instance.SaveCustomizationAsync();
    }
    private void OnHairButtonClicked()
    {
        // ���÷� HairIndex 2�� ����
        CustomizationManager.Instance.ChangePart(CustomizationPart.Hair, 2);
    }

    private void OnHatButtonClicked()
    {
        CustomizationManager.Instance.ChangePart(CustomizationPart.Hat, 1);
    }

    private void OnTopButtonClicked()
    {
        CustomizationManager.Instance.ChangePart(CustomizationPart.Top, 3);
    }

}
