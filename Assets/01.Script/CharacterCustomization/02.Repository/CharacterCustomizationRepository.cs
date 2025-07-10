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

        }, $"Ŀ������¡ ���� ID: {userId}");
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

                Debug.Log($"[Firebase] Ŀ������¡ �ε� ID : {userId}");
            }
            else
            {
                Debug.LogWarning($"[Firebase] Ŀ������¡ �����͸� ��ã�� ID : {userId}. ����Ʈ Ŀ������¡ ������ ��ȯ");
            }

            return customization;

        }, $"Ŀ������¡ �ε� ID : {userId}");
    }
}
