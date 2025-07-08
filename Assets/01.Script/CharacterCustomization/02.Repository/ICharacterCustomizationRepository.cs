using System.Threading.Tasks;
using UnityEngine;

public interface ICharacterCustomizationRepository 
{
    Task SaveCustomizationAsync(string userId, CharacterCustomization customization);
    Task<CharacterCustomization> LoadCustomizationAsync(string userId);
}
