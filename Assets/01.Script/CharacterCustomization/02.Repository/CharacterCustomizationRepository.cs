using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Firestore;
using System;
public class CharacterCustomizationRepository : FirebaseRepositoryBase
{
    private const string COLLECTION_NAME = "CharacterCustomizations";

    public async Task SaveCustomizationAsync(CharacterCustomization customization)
    {
        await ExecuteAsync(async () =>
        {
            DocumentReference docRef = Firestore.Collection(COLLECTION_NAME).Document(UserId);

            Dictionary<string, object> data = customization.ToDictionary();
            await docRef.SetAsync(data, SetOptions.MergeAll);

        }, $"커스터마이징 저장 ID: {UserId}");
    }
    public async Task<CharacterCustomization> LoadCustomizationAsync()
    {
        return await ExecuteAsync(async () =>
        {
            DocumentReference docRef = Firestore.Collection(COLLECTION_NAME).Document(UserId);

            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            var customization = new CharacterCustomization();

            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();

                foreach (var kvp in data)
                {
                    if (Enum.TryParse(kvp.Key, out ECustomizationPartType part))
                    {
                        int index = Convert.ToInt32(kvp.Value);
                        customization.ChangePart(part, index);
                    }
                }

                Debug.Log($"[Firebase] 커스터마이징 로드 ID : {UserId}");
            }
            else
            {
                Debug.LogWarning($"[Firebase] 커스터마이징 데이터를 찾지못함 ID : {UserId}. 디폴트 커스터마이징 데이터를 반환");
            }

            return customization;

        }, $"커스터마이징 로드 ID : {UserId}");
    }
}
