using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Rendering.LookDev;

public class CustomizationManager : MonoBehaviourSingleton<CustomizationManager>
{
    public CharacterCustomization CurrentCustomization { get; private set; }
    [SerializeField] private List<PartsMaxIndex> _partsMaxIndexMap;

    public event Action<CustomizationPart, int> OnPartChanged;
    public event Action<ECustomizeCharacterAnimType> OnPlayAnim;
    private CharacterCustomizationRepository _repo;

    //일단 디폴트유저로해둠
    string userId = "DefaultUser";
    protected override void Awake()
    {
        base.Awake();

        _repo = new CharacterCustomizationRepository();

        CurrentCustomization = new CharacterCustomization();
    }

    //로딩씬에서 처리
    //private async void Start()
    //{
    //    await LoadCustomizationAsync(userId);
    //}

    public List<CustomizationPart> GetEssentialParts()
    {
        return CurrentCustomization.EssentialParts;
    }
    public void ChangePart(CustomizationPart part, int newIndex)
    {
        if (CurrentCustomization == null)
        {
            CurrentCustomization = new CharacterCustomization();
        }

        CurrentCustomization.ChangePart(part, newIndex);

        OnPartChanged?.Invoke(part, newIndex);
    }
    public void GenerateRandomPart()
    {
        var keys = new List<CustomizationPart>(CurrentCustomization.PartIndexMap.Keys);

        foreach (var key in keys)
        {
            int maxIndex = 0;

            foreach (var maxIndexPart in _partsMaxIndexMap)
            {
                if (key == maxIndexPart.Part)
                {
                    maxIndex = maxIndexPart.MaxIndex;
                    break;
                }
            }

            if (maxIndex == 0)
            {
                Debug.LogWarning($"커스마이징 key값을 못찾아서 MaxIndex가 0인 상태 입니다.");
                continue;
            }

            int randIndex;
            if (CurrentCustomization.EssentialParts.Contains(key))
            {
                randIndex = UnityEngine.Random.Range(1, maxIndex + 1);
            }
            else
            {
                randIndex = UnityEngine.Random.Range(0, maxIndex + 1);
            }

            CurrentCustomization.ChangePart(key, randIndex);
            OnPartChanged?.Invoke(key, randIndex);
        }
    }

    public async Task SaveCustomizationAsync()
    {
        if (CurrentCustomization != null)
        {
            await _repo.SaveCustomizationAsync(userId, CurrentCustomization);
        }
    }

    public async Task LoadCustomizationAsync(string userID)
    {
        CurrentCustomization = await _repo.LoadCustomizationAsync(userID);

        if (IsAllPartsDefault(CurrentCustomization))
        {
            ApplyDefaultCustomization();
        }

        foreach (var part in CurrentCustomization.PartIndexMap)
        {
            OnPartChanged?.Invoke(part.Key, part.Value);
        }
    }

    //디폴트 커마상태인지 확인
    private bool IsAllPartsDefault(CharacterCustomization customization)
    {
        foreach (var kvp in customization.PartIndexMap)
        {
            if (kvp.Value > 0)
                return false;
        }
        return true;
    }

    private void ApplyDefaultCustomization()
    {
        List<CustomizationPart> essentialParts = GetEssentialParts();

        foreach (var parts in essentialParts)
        {
            CurrentCustomization.ChangePart(parts, 1);
        }
    }

    public int GetPartCurrentIndex(CustomizationPart part)
    {
        return CurrentCustomization.GetIndex(part);
    }
    public int GetPartMaxIndex(CustomizationPart part)
    {
        foreach (var partMaxIndex in _partsMaxIndexMap)
        {
            if (partMaxIndex.Part == part)
                return partMaxIndex.MaxIndex;
        }
        return 0;
    }

    public void PlayAnim(ECustomizeCharacterAnimType type)
    {
        OnPlayAnim?.Invoke(type);
    }
    public void CyclePart(CustomizationPart part, bool isNext)
    {
        int currentIndex = GetPartCurrentIndex(part);
        int maxIndex = GetPartMaxIndex(part);
        bool isEssential = CurrentCustomization.EssentialParts.Contains(part);
        int minIndex = isEssential ? 1 : 0;

        int newIndex;
        if (isNext)
        {
            newIndex = currentIndex + 1;
            if (newIndex > maxIndex) newIndex = minIndex;
        }
        else
        {
            newIndex = currentIndex - 1;
            if (newIndex < minIndex) newIndex = maxIndex;
        }

        ChangePart(part, newIndex);
    }

}
[System.Serializable]
public class PartsMaxIndex
{
    public CustomizationPart Part;
    public int MaxIndex;
}