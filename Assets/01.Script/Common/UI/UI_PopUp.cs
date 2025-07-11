using System;
using DG.Tweening;
using UnityEngine;

public class UI_PopUp : MonoBehaviour
{
    public Action _closeCallback;

    public void Open(Action callback = null)
    {
        _closeCallback = callback;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        gameObject.transform.DOKill();
        gameObject.SetActive(true);
        gameObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        gameObject.transform.DOScale(1f, 0.2f).SetEase(Ease.OutCirc).SetUpdate(true);
        //GameManager.instance.ChangeState(GameState.Pause);

    }

    public void Close()
    {
        _closeCallback?.Invoke();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
        gameObject.transform.DOKill();
        gameObject.transform.DOScale(0f, 0.2f).SetEase(Ease.OutCirc).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
