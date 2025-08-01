using TMPro;
using UnityEngine;

public class UI_Time : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private TextMeshProUGUI _dayText;
    private void Start()
    {
        GameTimeManager.Instance.OnDayChanged.AddListener(RefreshDayText);
        GameTimeManager.Instance.OnTimeChanged.AddListener(RefreshTimeText);
    }

    private void RefreshTimeText(int hour, int minute)
    {
        _timeText.text = $"{hour} : {minute}";
    }
    private void RefreshDayText(int currentDay)
    {
        _dayText.text = $"{currentDay+1} 일차";
    }
}
