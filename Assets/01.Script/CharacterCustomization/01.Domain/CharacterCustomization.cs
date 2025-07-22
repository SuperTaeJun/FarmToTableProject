using System;
using System.Collections.Generic;
using UnityEngine;



public class CharacterCustomization
{

    public Dictionary<CustomizationPart, int> PartIndexMap { get; private set; }
    public List<CustomizationPart> EssentialParts { get; private set; }
    public CharacterCustomization()
    {
        PartIndexMap = new Dictionary<CustomizationPart, int>();
        EssentialParts = new List<CustomizationPart>
        {
            CustomizationPart.Hair,
            CustomizationPart.Face,
            CustomizationPart.Top,
            CustomizationPart.Bottom,
            CustomizationPart.Shoes
        };

        InitializeAllParts();
    }

    private void InitializeAllParts()
    {
        foreach (CustomizationPart part in Enum.GetValues(typeof(CustomizationPart)))
        {
            PartIndexMap[part] = 0;
        }
    }

    public void ChangePart(CustomizationPart part, int newIndex)
    {
        if (!Enum.IsDefined(typeof(CustomizationPart), part))
        {
            throw new ArgumentException($"정의되지 않은 커스터마이징 파츠입니다: {part}", nameof(part));
        }

        if (newIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(newIndex), newIndex, "파츠 인덱스는 0 이상이어야 합니다.");
        }

        if (!PartIndexMap.ContainsKey(part))
        {
            throw new InvalidOperationException($"파츠 인덱스 맵에 '{part}' 파츠가 존재하지 않습니다.");
        }

        PartIndexMap[part] = newIndex;
    }

    public int GetIndex(CustomizationPart part)
    {
        if (!Enum.IsDefined(typeof(CustomizationPart), part))
        {
            throw new ArgumentException($"정의되지 않은 커스터마이징 파츠입니다: {part}", nameof(part));
        }

        return PartIndexMap.TryGetValue(part, out int value) ? value : -1;
    }

    public Dictionary<string, object> ToDictionary()
    {
        if (PartIndexMap == null || PartIndexMap.Count == 0)
        {
            throw new InvalidOperationException("파츠 인덱스 맵이 비어있거나 초기화되지 않았습니다.");
        }

        var result = new Dictionary<string, object>();

        foreach (var kvp in PartIndexMap)
        {
            result[kvp.Key.ToString()] = kvp.Value;
        }

        return result;
    }
}
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

