using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;
public class UI_TouchBounce : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] float _hoverSize = 1.2f;

    private RectTransform _rectTransform;
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void OnDisable()
    {
        _rectTransform.localScale = new Vector3(1, 1, 1f);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        ButtonDown(_rectTransform);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ButtonHoverEnter(_rectTransform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ButtonHoverExit(_rectTransform);
    }
    private void ButtonDown(RectTransform button)
    {
        button.transform.DOKill();

        button.transform.DOPunchScale
            (
                new Vector3(0.3f, 0.3f, 0), // 커졌다 작아질 크기
                0.3f,                      // 지속시간
                10,                        // 진동 횟수
                1                          // 탄성
            );
    }
    private void ButtonHoverEnter(RectTransform button)
    {
        button.transform.DOKill();
        button.transform.DOScale(_hoverSize, 0.2f).SetEase(Ease.OutBack);
    }

    private void ButtonHoverExit(RectTransform button)
    {
        button.transform.DOKill();
        button.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
    }
}
