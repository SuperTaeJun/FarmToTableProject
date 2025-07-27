using UnityEngine;

[CreateAssetMenu(fileName = "New Currency Data", menuName = "Currency/Currency Data")]
public class SO_CurrencyData : ScriptableObject
{
    [Header("재화 기본 정보")]
    public ECurrencyType currencyType;
    public string currencyName;
    [TextArea(2, 3)]
    public string description;
    
    [Header("시각적 요소")]
    public Sprite icon;
    public Color displayColor = Color.yellow;
    
    [Header("재화 설정")]
    public int defaultAmount = 0;
    public int maxAmount = 999999;
    public bool canGoNegative = false;
    
    [Header("표시 형식")]
    public string displayFormat = "{0:N0}";
    public string currencySymbol = "G";
    
    public string GetFormattedAmount(int amount)
    {
        return string.Format(displayFormat, amount) + currencySymbol;
    }
    
    public bool IsValidAmount(int amount)
    {
        if (!canGoNegative && amount < 0) return false;
        return amount <= maxAmount;
    }
}