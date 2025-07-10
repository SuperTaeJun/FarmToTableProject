using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering.LookDev;

public class CustomizationUI : MonoBehaviour
{
    [Header("Buttons")]
    public PartButtonInfo[] PartButtons;

    public Button SaveButton;
    public Button DanceAnimationButton;
    public Button IdleAnimationButton;
    public Button RandomButton;
    private void Start()
    {
        foreach (var partButtonInfo in PartButtons)
        {
            //이벤트 구독
            CustomizationPart part = partButtonInfo.Part;
            partButtonInfo.PrevButton.onClick.AddListener(() => ChangePart(part, isNext: false));
            partButtonInfo.NextButton.onClick.AddListener(() => ChangePart(part, isNext: true));

            CustomizationManager.Instance.OnPartChanged += ChangePartText;
        }

        SaveButton.onClick.AddListener(() => OnSaveButtonClicked());
        DanceAnimationButton.onClick.AddListener(() => OnDanceAnimationButtonClicked());
        IdleAnimationButton.onClick.AddListener(() => OnIdleAnimationButtonClicked());
        RandomButton.onClick.AddListener(() => OnRandomButtonClicked());
        SetPartText();

    }

    private void ChangePart(CustomizationPart part, bool isNext)
    {
        int currentIndex = CustomizationManager.Instance.GetPartCurrentIndex(part);
        int maxIndex = CustomizationManager.Instance.GetPartMaxIndex(part);
        List<CustomizationPart> essentialParts = CustomizationManager.Instance.GetEssentialParts();

        bool isEssential = essentialParts.Contains(part);

        int minIndex = isEssential ? 1 : 0;

        int newIndex;

        if (isNext)
        {
            newIndex = currentIndex + 1;
            if (newIndex > maxIndex)
                newIndex = minIndex; // 필수 파츠는 최소 1부터
        }
        else
        {
            newIndex = currentIndex - 1;
            if (newIndex < minIndex)
                newIndex = maxIndex;
        }

        CustomizationManager.Instance.ChangePart(part, newIndex);
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
    private void ChangePartText(CustomizationPart part, int newindex)
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
    public CustomizationPart Part;
    public Button PrevButton;
    public Button NextButton;
    public TextMeshProUGUI CountText;
}