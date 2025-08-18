using TMPro;
using UnityEngine;
using System.Collections;
public enum EPlayerNotificationType
{
    Chunk,
    LackOfMoney,
    LackOfSeed,
    BuildingInteraction,
    ReadyImage,
    DontHavePaint,
}
public class PlayerNotificationAbility : PlayerAbility
{
    [SerializeField] private GameObject _dialogBox;
    [SerializeField] private TextMeshProUGUI _dialogText;

    private Coroutine _autoHideCoroutine;

    private void Start()
    {
        ImageGenerationManager.Instance.OnImageGenerationComplete += (Texture2D texture) => ActiveDialogBox(EPlayerNotificationType.ReadyImage);
    }

    public void DisActiveDialogBox()
    {
        _dialogBox.SetActive(false);
    }
    public void ActiveDialogBox(EPlayerNotificationType type)
    {
        _dialogBox.SetActive(true);

        switch (type)
        {
            case EPlayerNotificationType.Chunk:
                _dialogText.text = "<color=red>F</color>를 눌러\n 땅을 구매 할까?";
                break;
            case EPlayerNotificationType.LackOfMoney:
                _dialogText.text = "<color=red>돈</color>이 부족해서 구매할 수 없어...";
                SetAutoHide(2f);
                break;
            case EPlayerNotificationType.LackOfSeed:
                _dialogText.text = "<color=red>씨앗</color>이 부족해서 심을 수 없어...";
                SetAutoHide(2f);
                break;
            case EPlayerNotificationType.BuildingInteraction:
                _dialogText.text = "<color=red>F</color>를 눌러서\n 상호작용하기";
                break;
            case EPlayerNotificationType.ReadyImage:
                SetAutoHide(2f);
                _dialogText.text = "주문했던 그림이\n 준비 된거같아!";
                break;
            case EPlayerNotificationType.DontHavePaint:
                SetAutoHide(2f);
                _dialogText.text = "가지고있는 그림이 없어...";
                break;
        }
    }


    private void SetAutoHide(float time)
    {
        if (_autoHideCoroutine != null)
            StopCoroutine(_autoHideCoroutine);

        _autoHideCoroutine = StartCoroutine(AutoHideDialogBox(time));
    }

    private IEnumerator AutoHideDialogBox(float time)
    {
        yield return new WaitForSeconds(time);

        if (_dialogBox.activeSelf)
            _dialogBox.SetActive(false);
    }
}
