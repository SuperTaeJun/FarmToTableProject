using System.Collections.Generic;
using UnityEngine;

public class UI_CurrencyPanel : MonoBehaviour
{
    [Header("재화 디스플레이 설정")]
    [SerializeField] private List<UI_CurrencyDisplay> _currencyDisplays = new List<UI_CurrencyDisplay>();
    [SerializeField] private Transform _currencyContainer;
    [SerializeField] private GameObject _currencyDisplayPrefab;
    
    [Header("자동 생성 설정")]
    [SerializeField] private bool _autoCreateDisplays = true;
    
    private void Start()
    {
        InitializePanel();
    }
    
    private void InitializePanel()
    {
        if (_autoCreateDisplays && _currencyContainer != null && _currencyDisplayPrefab != null)
        {
            CreateCurrencyDisplays();
        }
        
        RefreshAllDisplays();
    }
    
    private void CreateCurrencyDisplays()
    {
        // 기존 디스플레이 제거
        foreach (Transform child in _currencyContainer)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
        
        _currencyDisplays.Clear();
        
        // 각 재화 타입별로 디스플레이 생성
        var currencyTypes = System.Enum.GetValues(typeof(ECurrencyType));
        
        foreach (ECurrencyType currencyType in currencyTypes)
        {
            GameObject displayObj = Instantiate(_currencyDisplayPrefab, _currencyContainer);
            UI_CurrencyDisplay display = displayObj.GetComponent<UI_CurrencyDisplay>();
            
            if (display != null)
            {
                display.SetCurrencyType(currencyType);
                _currencyDisplays.Add(display);
            }
        }
    }
    
    public void RefreshAllDisplays()
    {
        foreach (var display in _currencyDisplays)
        {
            if (display != null)
            {
                // 디스플레이 강제 업데이트
                display.gameObject.SetActive(false);
                display.gameObject.SetActive(true);
            }
        }
    }
    
    public UI_CurrencyDisplay GetCurrencyDisplay(ECurrencyType currencyType)
    {
        return _currencyDisplays.Find(display => 
        {
            // 리플렉션을 사용해서 private 필드에 접근하거나
            // 또는 CurrencyDisplay에 public getter 추가 필요
            return display.name.Contains(currencyType.ToString());
        });
    }
    
    public void ShowCurrency(ECurrencyType currencyType, bool show = true)
    {
        var display = GetCurrencyDisplay(currencyType);
        if (display != null)
        {
            display.gameObject.SetActive(show);
        }
    }
    
    public void HideCurrency(ECurrencyType currencyType)
    {
        ShowCurrency(currencyType, false);
    }
}