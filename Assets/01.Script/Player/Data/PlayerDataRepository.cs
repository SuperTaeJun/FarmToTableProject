using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerDataRepository : FirebaseRepositoryBase
{
    private const string COLLECTION_NAME = "DefaultUser";
    private const string PLAYER_DATA_DOC = "playerData";

    public async Task SavePlayerDataAsync(PlayerDataDto playerData)
    {
        await ExecuteAsync(async () =>
        {
            await Firestore.Collection(COLLECTION_NAME)
                          .Document(PLAYER_DATA_DOC)
                          .SetAsync(playerData);
        }, "플레이어 위치 저장");
    }

    public async Task<PlayerDataDto> LoadPlayerDataAsync()
    {
        return await ExecuteAsync(async () =>
        {
            var snapshot = await Firestore.Collection(COLLECTION_NAME)
                                         .Document(PLAYER_DATA_DOC)
                                         .GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<PlayerDataDto>();
            }
            return null;
        }, "플레이어 위치 로드");
    }
}