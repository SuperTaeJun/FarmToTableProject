using DG.Tweening;
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

        Name.text = "□ "+_achievement.Name;
        Description.text = _achievement.Description;
        Progress.text = $"진행률 : {_achievement.CurrentValue}/{_achievement.TargetValue}";
    }

    public Achievment GetAchievement()
    {
        return _achievement;
    }
    public void PlayCompletionEffect()
    {

        // 텍스트들 원본 색상 저장
        var nameOriginalColor = Name.color;
        var descOriginalColor = Description.color;
        var progressOriginalColor = Progress.color;

        // 시퀀스 생성
        var sequence = DOTween.Sequence();

        // 펀치 스케일 효과
        sequence.Append(transform.DOPunchScale(Vector3.one * 0.4f, 0.4f, 8, 0.5f))
               .Join(Name.DOColor(Color.green, 0.2f))
               .Join(Description.DOColor(Color.green, 0.2f))
               .Join(Progress.DOColor(Color.green, 0.2f))
               .AppendInterval(0.3f)
               .OnComplete(() => Destroy(gameObject));
    }
}
