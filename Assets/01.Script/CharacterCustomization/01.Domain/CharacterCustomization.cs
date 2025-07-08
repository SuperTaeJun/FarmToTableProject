using UnityEngine;

public enum CustomizationPart
{
    Hair,
    Face,
    Hat,
    Top,
    Glove,
    Bottom,
    Shoes,
    Bag,
    EyeDeco
}


public class CharacterCustomization
{
    public string HairId { get; private set; }
    public string FaceId { get; private set; }
    public string HatId { get; private set; }
    public string TopId { get; private set; }
    public string GloveId { get; private set; }
    public string BottomId { get; private set; }
    public string ShoesId { get; private set; }
    public string BagId { get; private set; }
    public string EyeDecoId { get; private set; }

    public CharacterCustomization(
        string hairId,
        string faceId,
        string hatId,
        string topId,
        string gloveId,
        string bottomId,
        string shoesId,
        string bagId,
        string eyeDecoId)
    {
        HairId = hairId;
        FaceId = faceId;
        HatId = hatId;
        TopId = topId;
        GloveId = gloveId;
        BottomId = bottomId;
        ShoesId = shoesId;
        BagId = bagId;
        EyeDecoId = eyeDecoId;
    }

    public void ChangePart(CustomizationPart part, string newId)
    {
        switch (part)
        {
            case CustomizationPart.Hair:
                HairId = newId;
                break;
            case CustomizationPart.Face:
                FaceId = newId;
                break;
            case CustomizationPart.Hat:
                HatId = newId;
                break;
            case CustomizationPart.Top:
                TopId = newId;
                break;
            case CustomizationPart.Glove:
                GloveId = newId;
                break;
            case CustomizationPart.Bottom:
                BottomId = newId;
                break;
            case CustomizationPart.Shoes:
                ShoesId = newId;
                break;
            case CustomizationPart.Bag:
                BagId = newId;
                break;
            case CustomizationPart.EyeDeco:
                EyeDecoId = newId;
                break;
        }
    }
}
