using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;

public class GameTimeRepository : FirebaseRepositoryBase
{
    private const string COLLECTION_NAME = "GameTime";
    
    public async Task SaveGameTimeAsync(GameTimeDto gameTime)
    {
        await ExecuteAsync(async () =>
        {
            await Firestore.Collection(COLLECTION_NAME)
                          .Document(UserId)
                          .SetAsync(gameTime);
        }, "게임 시간 저장");
    }

    public async Task<GameTimeDto> LoadGameTimeAsync()
    {
        return await ExecuteAsync(async () =>
        {
            var snapshot = await Firestore.Collection(COLLECTION_NAME)
                                         .Document(UserId)
                                         .GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<GameTimeDto>();
            }
            return null;
        }, "게임 시간 로드");
    }
}