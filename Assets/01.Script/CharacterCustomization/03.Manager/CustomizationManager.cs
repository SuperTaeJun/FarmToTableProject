using System;
using UnityEngine;

public class CustomizationManager : MonoBehaviour
{
    private static CustomizationManager _instance;
    public static CustomizationManager Instance => _instance;

    public CharacterCustomization CurrentCustomization { get; private set; }

    // 파츠가 변경되면 알리는 이벤트
    public event Action<CustomizationPart, string> OnPartChanged;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ChangePart(CustomizationPart part, string newId)
    {
        if (CurrentCustomization == null)
        {
            CurrentCustomization = new CharacterCustomization(
                null, null, null, null, null, null, null, null, null);
        }

        CurrentCustomization.ChangePart(part, newId);
        OnPartChanged?.Invoke(part, newId);
    }

    public void SetCustomization(CharacterCustomization customization)
    {
        CurrentCustomization = customization;
    }
}
