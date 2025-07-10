using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
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
        foreach (var partButtonInfo in PartButtons)
        {
            //이벤트 구독
            CustomizationPart part = partButtonInfo.Part;
            partButtonInfo.PrevButton.onClick.AddListener(() => ChangePart(part, isNext: false));
            partButtonInfo.PrevButton.onClick.AddListener(() => ButtonDoTween(partButtonInfo.PrevButton.gameObject));
            partButtonInfo.NextButton.onClick.AddListener(() => ChangePart(part, isNext: true));
            partButtonInfo.NextButton.onClick.AddListener(() => ButtonDoTween(partButtonInfo.NextButton.gameObject));
        }

        CustomizationManager.Instance.OnPartChanged += ChangePartText;
        NextButton.onClick.AddListener(() => OnSaveButtonClicked());
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

        SceneManager.LoadScene("MainScene");
    }
    private void OnDanceAnimationButtonClicked()
    {
        ButtonDoTween(DanceAnimationButton.gameObject);
        CustomizationManager.Instance.PlayAnim(ECustomizeCharacterAnimType.Dance_2);
    }
    private void OnIdleAnimationButtonClicked()
    {
        ButtonDoTween(IdleAnimationButton.gameObject);
        CustomizationManager.Instance.PlayAnim(ECustomizeCharacterAnimType.Idle);
    }
    private void OnRandomButtonClicked()
    {
        ButtonDoTween(RandomButton.gameObject);
        CustomizationManager.Instance.GenerateRandomPart();
    }
    private void ButtonDoTween(GameObject button)
    {
        button.transform.DOKill(); // 기존 트윈 초기화 (중복 방지)

        button.transform
            .DOPunchScale(
                new Vector3(0.2f, 0.2f, 0), // 커졌다 작아질 크기
                0.3f,                      // 지속시간
                10,                        // 진동 횟수
                1                          // 탄성
            );
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