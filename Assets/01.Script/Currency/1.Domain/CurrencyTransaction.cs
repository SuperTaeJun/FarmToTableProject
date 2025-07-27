using System;

[Serializable]
public class CurrencyTransaction
{
    public ECurrencyType CurrencyType { get; private set; }
    public int Amount { get; private set; }
    public ETransactionType TransactionType { get; private set; }
    public string Reason { get; private set; }
    public DateTime Timestamp { get; private set; }

    public CurrencyTransaction(ECurrencyType currencyType, int amount, ETransactionType transactionType, string reason = "")
    {
        CurrencyType = currencyType;
        Amount = amount;
        TransactionType = transactionType;
        Reason = reason;
        Timestamp = DateTime.Now;
    }

    public override string ToString()
    {
        string action = TransactionType == ETransactionType.Buy ? "지출" : "획득";
        return $"[{CurrencyType}] {action}: {Amount} - {Reason} ({Timestamp:HH:mm:ss})";
    }
}