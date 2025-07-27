using System;

[Serializable]
public class Currency
{
    public ECurrencyType CurrencyType { get; private set; }
    public int Amount { get; private set; }
    public int MaxAmount { get; private set; }
    public bool CanGoNegative { get; private set; }

    public Currency(ECurrencyType currencyType, int amount = 0, int maxAmount = 999999, bool canGoNegative = false)
    {
        CurrencyType = currencyType;
        Amount = amount;
        MaxAmount = maxAmount;
        CanGoNegative = canGoNegative;
    }

    public bool CanAfford(int cost)
    {
        return Amount >= cost;
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0) return false;
        
        int newAmount = Amount - amount;
        if (!CanGoNegative && newAmount < 0) return false;
        if (newAmount < 0 && !CanGoNegative) return false;

        Amount = newAmount;
        return true;
    }

    public bool TryAdd(int amount)
    {
        if (amount <= 0) return false;
        
        int newAmount = Amount + amount;
        if (newAmount > MaxAmount) return false;

        Amount = newAmount;
        return true;
    }

    public void SetAmount(int amount)
    {
        if (!CanGoNegative && amount < 0) amount = 0;
        if (amount > MaxAmount) amount = MaxAmount;
        
        Amount = amount;
    }
}