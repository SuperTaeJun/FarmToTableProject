using System;
using System.Collections.Generic;
public class CharacterCustomization
{
    public Dictionary<ECustomizationPartType, int> PartIndexMap { get; private set; }
    public List<ECustomizationPartType> EssentialParts { get; private set; }
    public CharacterCustomization()
    {
        PartIndexMap = new Dictionary<ECustomizationPartType, int>();
        EssentialParts = new List<ECustomizationPartType>
        {
            ECustomizationPartType.Hair,
            ECustomizationPartType.Face,
            ECustomizationPartType.Top,
            ECustomizationPartType.Bottom,
            ECustomizationPartType.Shoes
        };

        InitializeAllParts();
    }

    private void InitializeAllParts()
    {
        foreach (ECustomizationPartType part in Enum.GetValues(typeof(ECustomizationPartType)))
        {
            PartIndexMap[part] = 0;
        }
    }

    public void ChangePart(ECustomizationPartType part, int newIndex)
    {
        if (!Enum.IsDefined(typeof(ECustomizationPartType), part))
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

    public int GetIndex(ECustomizationPartType part)
    {
        if (!Enum.IsDefined(typeof(ECustomizationPartType), part))
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
