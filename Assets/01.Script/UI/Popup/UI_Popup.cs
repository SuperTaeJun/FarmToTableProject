using System;
using DG.Tweening;
using UnityEngine;

public class UI_Popup : MonoBehaviour
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
    }

    public void Close()
    {
        _closeCallback?.Invoke();

        PopupManager.Instance.PopUpClose();

        gameObject.transform.DOKill();
        gameObject.transform.DOScale(0f, 0.2f).SetEase(Ease.OutCirc).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
