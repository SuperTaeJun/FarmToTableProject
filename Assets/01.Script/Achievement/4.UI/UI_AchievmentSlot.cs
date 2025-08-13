using TMPro;
using UnityEngine;

public class UI_AchievmentSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Name;
    [SerializeField] private TextMeshProUGUI Description;
    [SerializeField] private TextMeshProUGUI Progress;

    private Achievment _achievement;

    public void Setup(Achievment achievement)
    {
        _achievement = achievement;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (_achievement == null) return;

        Name.text = "¡à "+_achievement.Name;
        Description.text = _achievement.Description;
        Progress.text = $"ÁøÇà·ü : {_achievement.CurrentValue}/{_achievement.TargetValue}";
    }

    public Achievment GetAchievement()
    {
        return _achievement;
    }
}
