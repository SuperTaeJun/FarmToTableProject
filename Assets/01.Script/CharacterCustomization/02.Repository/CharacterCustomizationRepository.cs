using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Firestore;
using System;
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

        }, $"커스마이징 저장 ID: {userId}");
    }
    public async Task<CharacterCustomization> LoadCustomizationAsync(string userId)
    {
        return await ExecuteAsync(async () =>
        {
            DocumentReference docRef = Firestore
                .Collection(COLLECTION_NAME)
                .Document(userId);

            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            var customization = new CharacterCustomization();

            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();

                foreach (var kvp in data)
                {
                    if (Enum.TryParse(kvp.Key, out CustomizationPart part))
                    {
                        int index = Convert.ToInt32(kvp.Value);
                        customization.ChangePart(part, index);
                    }
                }

                Debug.Log($"[Firebase] 커스마이징 로드 ID : {userId}");
            }
            else
            {
                Debug.LogWarning($"[Firebase] 커스마이징 데이터를 못찾음 ID : {userId}. 디폴트 커스마이징 데이터 반환");
            }

            return customization;

        }, $"커스마이징 로드 ID : {userId}");
    }
}
