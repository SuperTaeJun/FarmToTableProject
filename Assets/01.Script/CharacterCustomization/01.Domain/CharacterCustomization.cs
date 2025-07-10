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
            CustomizationPart.Hair, CustomizationPart.Face, CustomizationPart.Top, CustomizationPart.Bottom, CustomizationPart.Shoes 
        };

        foreach (CustomizationPart part in Enum.GetValues(typeof(CustomizationPart)))
        {
            PartIndexMap[part] = 0;
        }
    }

    public CharacterCustomization(Dictionary<CustomizationPart, int> partIndexMap)
    {
        PartIndexMap = new Dictionary<CustomizationPart, int>(partIndexMap);
    }

    public void ChangePart(CustomizationPart part, int newIndex)
    {
        if (PartIndexMap.ContainsKey(part))
        {
            PartIndexMap[part] = newIndex;
        }
    }

    public int GetIndex(CustomizationPart part)
    {
        return PartIndexMap.ContainsKey(part) ? PartIndexMap[part] : -1;
    }
    public Dictionary<string, object> ToDictionary()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();

        foreach (var kvp in PartIndexMap)
        {
            dict[kvp.Key.ToString()] = kvp.Value;
        }

        return dict;
    }
    public static CharacterCustomization FromDictionary(Dictionary<string, object> data)
    {
        Dictionary<CustomizationPart, int> map = new Dictionary<CustomizationPart, int>();

        foreach (var kvp in data)
        {
            if (Enum.TryParse(kvp.Key, out CustomizationPart part))
            {
                map[part] = Convert.ToInt32(kvp.Value);
            }
        }

        return new CharacterCustomization(map);
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

