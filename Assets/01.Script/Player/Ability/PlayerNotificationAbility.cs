using TMPro;
using UnityEngine;
using System.Collections;
public enum EPlayerNotificationType
{
    Chunk,
    LackOfMoney,
    LackOfSeed
}
public class PlayerNotificationAbility : PlayerAbility
{
    [SerializeField] private GameObject _dialogBox;
    [SerializeField] private TextMeshProUGUI _dialogText;

    private Coroutine _autoHideCoroutine;

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
                _dialogText.text = "<Color=red>F</Color>를 눌러\n땅을 구매할까...";
                break;
            case EPlayerNotificationType.LackOfMoney:
                _dialogText.text = "<Color=red>돈</Color>이 부족한거 같아...";
                SetAutoHide(2f);
                break;
            case EPlayerNotificationType.LackOfSeed:
                _dialogText.text = "<Color=red>씨앗</Color>이 부족한거 같아...";
                SetAutoHide(2f);
                break;
        }

    }

    private void SetAutoHide(float time)
    {
        // 기존 코루틴이 돌고 있으면 중단하고 다시 시작
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
