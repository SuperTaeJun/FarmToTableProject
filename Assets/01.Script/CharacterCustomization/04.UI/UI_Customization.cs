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

        // �̺�Ʈ ����
        foreach (var partButtonInfo in PartButtons)
        {
            CustomizationPart part = partButtonInfo.Part;

            partButtonInfo.PrevButton.onClick.AddListener(() => {
                CustomizationManager.Instance.CyclePart(part, false);
                ButtonDoTween(partButtonInfo.PrevButton.gameObject);
            });

            partButtonInfo.NextButton.onClick.AddListener(() => {
                CustomizationManager.Instance.CyclePart(part, true);
                ButtonDoTween(partButtonInfo.NextButton.gameObject);
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
        button.transform.DOKill();

        button.transform.DOPunchScale
            (
                new Vector3(0.3f, 0.3f, 0), // Ŀ���� �۾��� ũ��
                0.3f,                      // ���ӽð�
                10,                        // ���� Ƚ��
                1                          // ź��
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