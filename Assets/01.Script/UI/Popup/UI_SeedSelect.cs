using UnityEngine;
using UnityEngine.UI;

public class UI_SeedSelect : UI_Popup
{
    [SerializeField] private Button[] SeedButtons;

    private void Start()
    {
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        PlayerFarmingAbility ability = player.GetAbility<PlayerFarmingAbility>();

        for (int i = 0; i < SeedButtons.Length; i++)
        {
            int seedIndex = i; // 클로저 문제 해결
            SeedButtons[i].onClick.AddListener(() =>
            {
                ability.SetCurrentSeed(seedIndex); Close();
            });
        }
    }
}
