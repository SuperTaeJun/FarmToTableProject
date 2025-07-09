using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Firestore;
public class CharacterCustomizationRepository : FirebaseRepositoryBase
{
    private const string COLLECTION_NAME = "CharacterCustomizations";

    public async Task SaveCustomizationAsync(string userId, CharacterCustomization customization)
    {
        await ExecuteAsync(async () =>
        {
            DocumentReference docRef = Firestore
                .Collection(COLLECTION_NAME)
                .Document(userId);

            Dictionary<string, object> data = customization.ToDictionary();

            await docRef.SetAsync(data, SetOptions.MergeAll);

        }, $"SaveCustomizationAsync for UserId: {userId}");
    }

    public async Task<CharacterCustomization> LoadCustomizationAsync(string userId)
    {
        return await ExecuteAsync(async () =>
        {
            DocumentReference docRef = Firestore
                .Collection(COLLECTION_NAME)
                .Document(userId);

            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();

                var customization = CharacterCustomization.FromDictionary(data);

                Debug.Log($"[Firebase] Loaded customization for {userId}");

                return customization;
            }
            else
            {
                Debug.LogWarning($"[Firebase] No customization data found for {userId}. Returning default customization.");

                return new CharacterCustomization();
            }

        }, $"LoadCustomizationAsync for UserId: {userId}");
    }
}
