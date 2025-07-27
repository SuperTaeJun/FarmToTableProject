using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UI_CurrencyDisplay : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private ECurrencyType _currencyType;
    [SerializeField] private Image _currencyIcon;
    [SerializeField] private TextMeshProUGUI _amountText;

    [Header("애니메이션 설정")]
    [SerializeField] private bool _enableChangeAnimation = true;
    [SerializeField] private float _animationDuration = 0.3f;
    [SerializeField] private Color _increaseColor = Color.green;
    [SerializeField] private Color _decreaseColor = Color.red;
    
    private SO_CurrencyData _currencyData;
    private int _currentAmount;
    private Color _originalColor;
    
    private void Start()
    {
        InitializeCurrency();
        SubscribeToEvents();
        UpdateDisplay();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void InitializeCurrency()
    {
        if (CurrencyManager.Instance != null)
        {
            _currencyData = CurrencyManager.Instance.GetCurrencyData(_currencyType);
            
            if (_currencyData != null && _currencyIcon != null)
            {
                _currencyIcon.sprite = _currencyData.icon;
                _currencyIcon.color = _currencyData.displayColor;
            }
        }
        
        if (_amountText != null)
        {
            _originalColor = _amountText.color;
        }
    }
    
    private void SubscribeToEvents()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged.AddListener(OnCurrencyChanged);
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged.RemoveListener(OnCurrencyChanged);
        }
    }
    
    private void OnCurrencyChanged(ECurrencyType currencyType, int oldAmount, int newAmount)
    {
        if (currencyType == _currencyType)
        {
            int previousAmount = _currentAmount;
            _currentAmount = newAmount;
            
            UpdateDisplay();
            
            if (_enableChangeAnimation)
            {
                PlayChangeAnimation(previousAmount, newAmount);
            }
        }
    }
    
    private void UpdateDisplay()
    {
        if (_amountText != null && CurrencyManager.Instance != null)
        {
            string formattedAmount = CurrencyManager.Instance.GetFormattedAmount(_currencyType);
            _amountText.text = formattedAmount;
        }
    }
    
    private void PlayChangeAnimation(int oldAmount, int newAmount)
    {
        if (_amountText == null) return;
        
        // 색상 변경 애니메이션
        Color targetColor = newAmount > oldAmount ? _increaseColor : _decreaseColor;
        
        // 기존 트윈 정지
        _amountText.DOKill();
        
        // 색상 변경 시퀀스
        Sequence colorSequence = DOTween.Sequence();
        colorSequence.Append(_amountText.DOColor(targetColor, _animationDuration * 0.3f))
                    .Append(_amountText.DOColor(_originalColor, _animationDuration * 0.7f));
        
        // 스케일 애니메이션 시퀀스
        Vector3 originalScale = _amountText.transform.localScale;
        Sequence scaleSequence = DOTween.Sequence();
        scaleSequence.Append(_amountText.transform.DOScale(originalScale * 1.2f, _animationDuration * 0.3f))
                    .Append(_amountText.transform.DOScale(originalScale, _animationDuration * 0.7f));
    }
    
    public void SetCurrencyType(ECurrencyType currencyType)
    {
        UnsubscribeFromEvents();
        _currencyType = currencyType;
        InitializeCurrency();
        SubscribeToEvents();
        UpdateDisplay();
    }
    
}