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
            throw new ArgumentException($"���ǵ��� ���� Ŀ���͸���¡ �����Դϴ�: {part}", nameof(part));
        }

        if (newIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(newIndex), newIndex, "���� �ε����� 0 �̻��̾�� �մϴ�.");
        }

        if (!PartIndexMap.ContainsKey(part))
        {
            throw new InvalidOperationException($"���� �ε��� �ʿ� '{part}' ������ �������� �ʽ��ϴ�.");
        }

        PartIndexMap[part] = newIndex;
    }

    public int GetIndex(CustomizationPart part)
    {
        if (!Enum.IsDefined(typeof(CustomizationPart), part))
        {
            throw new ArgumentException($"���ǵ��� ���� Ŀ���͸���¡ �����Դϴ�: {part}", nameof(part));
        }

        return PartIndexMap.TryGetValue(part, out int value) ? value : -1;
    }

    public Dictionary<string, object> ToDictionary()
    {
        if (PartIndexMap == null || PartIndexMap.Count == 0)
        {
            throw new InvalidOperationException("���� �ε��� ���� ����ְų� �ʱ�ȭ���� �ʾҽ��ϴ�.");
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

