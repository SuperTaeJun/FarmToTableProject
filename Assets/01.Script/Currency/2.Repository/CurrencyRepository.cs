using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class CurrencyRepository : FirebaseRepositoryBase
{
    private const string DEFAULT_USER_ID = "DefaultUser";
    private const string COLLECTION_NAME = "currencies";

    private string GetUserCurrencyPath()
    {
        return $"{COLLECTION_NAME}/{DEFAULT_USER_ID}";
    }

    public async Task SaveCurrencies(List<Currency> currencies)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Document(GetUserCurrencyPath());

            var currencyDtoList = new List<CurrencyDto>();
            foreach (var currency in currencies)
            {
                currencyDtoList.Add(new CurrencyDto(currency));
            }

            var docData = new Dictionary<string, object>
            {
                { "currencies", currencyDtoList }
            };

            await docRef.SetAsync(docData);
        }, "Save User Currencies");
    }

    public async Task<List<Currency>> LoadCurrencies()
    {
        return await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Document(GetUserCurrencyPath());
            var snapshot = await docRef.GetSnapshotAsync();

            var result = new List<Currency>();

            if (snapshot.Exists && snapshot.ContainsField("currencies"))
            {
                var currencyDtos = snapshot.ConvertTo<Dictionary<string, List<CurrencyDto>>>()["currencies"];

                foreach (var currencyDto in currencyDtos)
                {
                    result.Add(currencyDto.ToCurrency());
                }
            }
            else
            {
                // 기본 재화 초기화
                result.Add(new Currency(ECurrencyType.Money, 1000, 999999, false));
                result.Add(new Currency(ECurrencyType.Gem, 0, 99999, false));
            }

            return result;
        }, "Load User Currencies");
    }

    public async Task UpdateCurrency(ECurrencyType currencyType, int newAmount)
    {
        await ExecuteAsync(async () =>
        {
            var currencies = await LoadCurrencies();
            var targetCurrency = currencies.FirstOrDefault(c => c.CurrencyType == currencyType);

            if (targetCurrency != null)
            {
                targetCurrency.SetAmount(newAmount);
                await SaveCurrencies(currencies);
            }
        }, $"Update Currency [{currencyType}] to {newAmount}");
    }

    public async Task<int> GetCurrencyAmount(ECurrencyType currencyType)
    {
        return await ExecuteAsync(async () =>
        {
            var currencies = await LoadCurrencies();
            var targetCurrency = currencies.FirstOrDefault(c => c.CurrencyType == currencyType);
            return targetCurrency?.Amount ?? 0;
        }, $"Get Currency Amount [{currencyType}]");
    }
}