using System;
using UnityEngine;
using System.Threading.Tasks;

public class CustomizationManager : MonoBehaviourSingleton<CustomizationManager>
{
    public CharacterCustomization CurrentCustomization { get; private set; }

    public event Action<CustomizationPart, int> OnPartChanged;

    private CharacterCustomizationRepository _repo;
    string userId = "DefaultUser";
    protected override void Awake()
    {
        base.Awake();

        _repo = new CharacterCustomizationRepository();

        CurrentCustomization = new CharacterCustomization();
    }

    private async void Start()
    {
        await LoadCustomizationAsync(userId);
    }

    public void ChangePart(CustomizationPart part, int newIndex)
    {
        if (CurrentCustomization == null)
        {
            CurrentCustomization = new CharacterCustomization();
        }

        CurrentCustomization.ChangePart(part, newIndex);

        Debug.Log($"[Manager] {part} changed to index {newIndex}");

        OnPartChanged?.Invoke(part, newIndex);
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

        if (IsAllPartsUnset(CurrentCustomization))
        {
            Debug.Log("[Manager] No saved customization found. Applying default values.");
            ApplyDefaultCustomization();
        }

        NotifyAllPartsChanged();
    }

    private bool IsAllPartsUnset(CharacterCustomization customization)
    {
        foreach (var kvp in customization.PartIndexMap)
        {
            if (kvp.Value >= 0)
                return false;
        }
        return true;
    }

    private void ApplyDefaultCustomization()
    {
        CurrentCustomization.ChangePart(CustomizationPart.Hair, 0);
        CurrentCustomization.ChangePart(CustomizationPart.Face, 0);
        CurrentCustomization.ChangePart(CustomizationPart.Top, 0);
        CurrentCustomization.ChangePart(CustomizationPart.Bottom, 0);
        // etc. 필요한 디폴트 값 세팅
    }

    private void NotifyAllPartsChanged()
    {
        foreach (var kvp in CurrentCustomization.PartIndexMap)
        {
            OnPartChanged?.Invoke(kvp.Key, kvp.Value);
        }
    }
}
