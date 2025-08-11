using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class CurrencyRepository : FirebaseRepositoryBase
{
    private const string COLLECTION_NAME = "currencies";

    private string GetUserCurrencyPath()
    {
        return $"{COLLECTION_NAME}/{UserId}";
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
        }, "사용자 통화 저장");
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
        }, "사용자 통화 로드");
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
        }, $"통화 업데이트 [{currencyType}] to {newAmount}");
    }

    public async Task<int> GetCurrencyAmount(ECurrencyType currencyType)
    {
        return await ExecuteAsync(async () =>
        {
            var currencies = await LoadCurrencies();
            var targetCurrency = currencies.FirstOrDefault(c => c.CurrencyType == currencyType);
            return targetCurrency?.Amount ?? 0;
        }, $"통화 수량 조회 [{currencyType}]");
    }

    public async Task DeleteAllData()
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Document(GetUserCurrencyPath());
            await docRef.DeleteAsync();
        }, "모든 통화 데이터 삭제");
    }
}