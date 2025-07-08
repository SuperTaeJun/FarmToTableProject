using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Firestore;
public class FirebaseCharacterCustomizationRepository : MonoBehaviour
{
    private readonly FirebaseFirestore _firestore;

    public FirebaseCharacterCustomizationRepository()
    {
        _firestore = FirebaseFirestore.DefaultInstance;
    }

    public async Task SaveCustomizationAsync(string userId, CharacterCustomization customization)
    {
        var docRef = _firestore.Collection("CharacterCustomizations").Document(userId);
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "HairId", customization.HairId },
            { "FaceId", customization.FaceId },
            { "HatId", customization.HatId },
            { "TopId", customization.TopId },
            { "GloveId", customization.GloveId },
            { "BottomId", customization.BottomId },
            { "ShoesId", customization.ShoesId },
            { "BagId", customization.BagId },
            { "EyeDecoId", customization.EyeDecoId }
        };
        await docRef.SetAsync(data);
    }

    public async Task<CharacterCustomization> LoadCustomizationAsync(string userId)
    {
        var docRef = _firestore.Collection("CharacterCustomizations").Document(userId);
        var snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists)
            return null;

        var data = snapshot.ToDictionary();

        return new CharacterCustomization(
            data["HairId"]?.ToString(),
            data["FaceId"]?.ToString(),
            data["HatId"]?.ToString(),
            data["TopId"]?.ToString(),
            data["GloveId"]?.ToString(),
            data["BottomId"]?.ToString(),
            data["ShoesId"]?.ToString(),
            data["BagId"]?.ToString(),
            data["EyeDecoId"]?.ToString()
        );
    }
}
