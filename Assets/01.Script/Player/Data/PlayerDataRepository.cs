using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerDataRepository : FirebaseRepositoryBase
{
    private const string COLLECTION_NAME = "PlayerData";
    
    public async Task SavePlayerDataAsync(PlayerDataDto playerData)
    {
        await ExecuteAsync(async () =>
        {
            await Firestore.Collection(COLLECTION_NAME)
                          .Document(UserId)
                          .SetAsync(playerData);
        }, "플레이어 데이터 저장");
    }

    public async Task<PlayerDataDto> LoadPlayerDataAsync()
    {
        return await ExecuteAsync(async () =>
        {
            var snapshot = await Firestore.Collection(COLLECTION_NAME)
                                         .Document(UserId)
                                         .GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<PlayerDataDto>();
            }
            return null;
        }, "플레이어 데이터 로드");
    }
}