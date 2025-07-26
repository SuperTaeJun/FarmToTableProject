using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class CustomizationUI : MonoBehaviour
{
    [Header("Buttons")]
    public PartButtonInfo[] PartButtons;
    public Button NextButton;
    public Button DanceAnimationButton;
    public Button IdleAnimationButton;
    public Button RandomButton;
    private void Start()
    {

        // �̺�Ʈ ����
        foreach (var partButtonInfo in PartButtons)
        {
            ECustomizationPartType part = partButtonInfo.Part;

            partButtonInfo.PrevButton.onClick.AddListener(() => {
                CustomizationManager.Instance.CyclePart(part, false);
            });

            partButtonInfo.NextButton.onClick.AddListener(() => {
                CustomizationManager.Instance.CyclePart(part, true);
            });
        }

        CustomizationManager.Instance.OnPartChanged += ChangePartText;
        NextButton.onClick.AddListener(() => OnSaveButtonClicked());
        DanceAnimationButton.onClick.AddListener(() => OnDanceAnimationButtonClicked());
        IdleAnimationButton.onClick.AddListener(() => OnIdleAnimationButtonClicked());
        RandomButton.onClick.AddListener(() => OnRandomButtonClicked());
        SetPartText();

    }
    private void SetPartText()
    {
        CharacterCustomization customInfo = CustomizationManager.Instance.CurrentCustomization;

        foreach (var info in customInfo.PartIndexMap)
        {
            foreach (var partButtonInfo in PartButtons)
            {
                if (partButtonInfo.Part == info.Key)
                {
                    int maxIndex = CustomizationManager.Instance.GetPartMaxIndex(partButtonInfo.Part);

                    partButtonInfo.CountText.text = $"{info.Value} / {maxIndex}";
                }
            }
        }
    }
    private void ChangePartText(ECustomizationPartType part, int newindex)
    {
        int maxIndex = CustomizationManager.Instance.GetPartMaxIndex(part);

        foreach (var partButtonInfo in PartButtons)
        {
            if (partButtonInfo.Part == part)
                partButtonInfo.CountText.text = $"{newindex} / {maxIndex}";
        }
    }

    private async void OnSaveButtonClicked()
    {
        await CustomizationManager.Instance.SaveCustomizationAsync();

        //���࿡ �̹� ���尡 ������ �ٷ� ���̵�
        if (GameObject.FindWithTag("World"))
            FadeManager.Instance.FadeToScene("MainScene");
        else
            FadeManager.Instance.FadeToScene("WorldLodingScene");
    }
    private void OnDanceAnimationButtonClicked()
    {
        CustomizationManager.Instance.PlayAnim(ECustomizeCharacterAnimType.Dance_2);
    }
    private void OnIdleAnimationButtonClicked()
    {
        CustomizationManager.Instance.PlayAnim(ECustomizeCharacterAnimType.Idle);
    }
    private void OnRandomButtonClicked()
    {
        CustomizationManager.Instance.GenerateRandomPart();
    }

}
[System.Serializable]
public class PartButtonInfo
{
    public ECustomizationPartType Part;
    public Button PrevButton;
    public Button NextButton;
    public TextMeshProUGUI CountText;
}