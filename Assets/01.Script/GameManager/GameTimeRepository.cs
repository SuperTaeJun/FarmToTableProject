using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;

public class GameTimeRepository : FirebaseRepositoryBase
{
    private const string COLLECTION_NAME = "DefaultUser";
    private const string GAME_TIME_DOC = "gameTime";

    public async Task SaveGameTimeAsync(GameTimeDto gameTime)
    {
        await ExecuteAsync(async () =>
        {
            await Firestore.Collection(COLLECTION_NAME)
                          .Document(GAME_TIME_DOC)
                          .SetAsync(gameTime);
        }, "게임 시간 저장");
    }

    public async Task<GameTimeDto> LoadGameTimeAsync()
    {
        return await ExecuteAsync(async () =>
        {
            var snapshot = await Firestore.Collection(COLLECTION_NAME)
                                         .Document(GAME_TIME_DOC)
                                         .GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<GameTimeDto>();
            }
            return null;
        }, "게임 시간 로드");
    }
}