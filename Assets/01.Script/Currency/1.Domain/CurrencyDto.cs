using Firebase.Firestore;

[FirestoreData]
public class CurrencyDto
{
    [FirestoreProperty]
    public string CurrencyType { get; set; }
    
    [FirestoreProperty]
    public int Amount { get; set; }
    
    [FirestoreProperty]
    public int MaxAmount { get; set; }
    
    [FirestoreProperty]
    public bool CanGoNegative { get; set; }

    public CurrencyDto() { }

    public CurrencyDto(Currency currency)
    {
        CurrencyType = currency.CurrencyType.ToString();
        Amount = currency.Amount;
        MaxAmount = currency.MaxAmount;
        CanGoNegative = currency.CanGoNegative;
    }

    public Currency ToCurrency()
    {
        if (System.Enum.TryParse<ECurrencyType>(CurrencyType, out var currencyType))
        {
            return new Currency(currencyType, Amount, MaxAmount, CanGoNegative);
        }
        
        return new Currency(ECurrencyType.Money, 0);
    }
}