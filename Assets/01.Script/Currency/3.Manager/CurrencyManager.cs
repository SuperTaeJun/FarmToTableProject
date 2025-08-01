using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [Header("재화 설정")]
    [SerializeField] private List<SO_CurrencyData> _currencyDataList;
    
    public static CurrencyManager Instance;
    
    private CurrencyRepository _repository;
    private List<Currency> _currencies = new List<Currency>();
    
    // 재화 이벤트들
    public DebugEvent<ECurrencyType, int, int> OnCurrencyChanged = new DebugEvent<ECurrencyType, int, int>(); // 타입, 이전값, 새값
    public DebugEvent<ECurrencyType, int> OnCurrencySpent = new DebugEvent<ECurrencyType, int>();
    public DebugEvent<ECurrencyType, int> OnCurrencyEarned = new DebugEvent<ECurrencyType, int>();
    
    public List<Currency> Currencies => new List<Currency>(_currencies);
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _repository = new CurrencyRepository();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private async void Start()
    {
        await LoadCurrencies();

        //임시로
        _currencies[0].SetAmount(99999);
        OnCurrencyChanged.Invoke(ECurrencyType.Money, 0,_currencies[0].Amount);
    }
    
    #region 재화 로드/저장
    public async Task LoadCurrencies()
    {
        try
        {
            _currencies = await _repository.LoadCurrencies();
            
            // 각 재화 타입별로 이벤트 발생
            foreach (var currency in _currencies)
            {
                OnCurrencyChanged.Invoke(currency.CurrencyType, 0, currency.Amount);
            }
            
            Debug.Log("재화 로드 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"재화 로드 실패: {e.Message}");
            
            // 실패 시 기본값으로 초기화
            InitializeDefaultCurrencies();
        }
    }
    
    public async Task SaveCurrencies()
    {
        try
        {
            await _repository.SaveCurrencies(_currencies);
        }
        catch (Exception e)
        {
            Debug.LogError($"재화 저장 실패: {e.Message}");
        }
    }
    
    private void InitializeDefaultCurrencies()
    {
        _currencies.Clear();
        _currencies.Add(new Currency(ECurrencyType.Money, 2000, 999999, false));
        _currencies.Add(new Currency(ECurrencyType.Gem, 0, 99999, false));
        
        foreach (var currency in _currencies)
        {
            OnCurrencyChanged.Invoke(currency.CurrencyType, 0, currency.Amount);
        }
    }
    #endregion
    
    #region 재화 조회
    public int GetCurrencyAmount(ECurrencyType currencyType)
    {
        var currency = _currencies.FirstOrDefault(c => c.CurrencyType == currencyType);
        return currency?.Amount ?? 0;
    }
    
    public Currency GetCurrency(ECurrencyType currencyType)
    {
        return _currencies.FirstOrDefault(c => c.CurrencyType == currencyType);
    }
    
    public bool CanAfford(ECurrencyType currencyType, int amount)
    {
        var currency = GetCurrency(currencyType);
        return currency?.CanAfford(amount) ?? false;
    }
    
    public SO_CurrencyData GetCurrencyData(ECurrencyType currencyType)
    {
        return _currencyDataList?.FirstOrDefault(data => data.currencyType == currencyType);
    }
    
    public string GetFormattedAmount(ECurrencyType currencyType)
    {
        var amount = GetCurrencyAmount(currencyType);
        var data = GetCurrencyData(currencyType);
        return data?.GetFormattedAmount(amount) ?? amount.ToString();
    }
    #endregion
    
    #region 재화 사용
    public async Task<bool> TrySpendCurrency(ECurrencyType currencyType, int amount)
    {
        if (amount <= 0) return false;
        
        var currency = GetCurrency(currencyType);
        if (currency == null || !currency.CanAfford(amount))
        {
            Debug.LogWarning($"재화 부족: {currencyType} 필요:{amount} 보유:{currency?.Amount ?? 0}");
            return false;
        }
        
        int oldAmount = currency.Amount;
        if (currency.TrySpend(amount))
        {
            OnCurrencySpent.Invoke(currencyType, amount);
            OnCurrencyChanged.Invoke(currencyType, oldAmount, currency.Amount);
            await SaveCurrencies();
            return true;
        }
        
        return false;
    }
    
    public async Task<bool> TryEarnCurrency(ECurrencyType currencyType, int amount)
    {
        if (amount <= 0) return false;
        
        var currency = GetCurrency(currencyType);
        if (currency == null) return false;
        
        int oldAmount = currency.Amount;
        if (currency.TryAdd(amount))
        {
            OnCurrencyEarned.Invoke(currencyType, amount);
            OnCurrencyChanged.Invoke(currencyType, oldAmount, currency.Amount);
            await SaveCurrencies();
            return true;
        }
        
        return false;
    }
    
    public async Task SetCurrencyAmount(ECurrencyType currencyType, int amount)
    {
        var currency = GetCurrency(currencyType);
        if (currency == null) return;
        
        int oldAmount = currency.Amount;
        currency.SetAmount(amount);
        OnCurrencyChanged.Invoke(currencyType, oldAmount, currency.Amount);
        await SaveCurrencies();
    }
    #endregion
    
    #region 유틸리티
    public bool HasSufficientCurrency(Dictionary<ECurrencyType, int> costs)
    {
        foreach (var cost in costs)
        {
            if (!CanAfford(cost.Key, cost.Value))
                return false;
        }
        return true;
    }
    
    public async Task<bool> TrySpendMultipleCurrencies(Dictionary<ECurrencyType, int> costs)
    {
        // 먼저 모든 재화가 충분한지 확인
        if (!HasSufficientCurrency(costs))
            return false;
        
        // 모든 재화 차감
        foreach (var cost in costs)
        {
            if (!await TrySpendCurrency(cost.Key, cost.Value))
            {
                // 실패 시 롤백은 복잡하므로 여기서는 로그만 출력
                Debug.LogError($"재화 차감 중 오류 발생: {cost.Key}");
                return false;
            }
        }
        
        return true;
    }
    #endregion
}