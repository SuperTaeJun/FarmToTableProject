using System;
using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }

    [Header("Time Settings")]
    [Tooltip("게임 내 하루가 몇 초인지")]
    public float secondsPerDay = 60f;
    [Tooltip("시작 시간 (시)")]
    public int startHour = 12; // 12시부터 시작

    private float totalGameTime = 0f;
    private int previousDay = 0;
    private int previousHour = 0;
    private int previousMinute = 0;

    // 이벤트 정의
    public DebugEvent<int> OnDayChanged= new DebugEvent<int>();
    public DebugEvent<int, int> OnTimeChanged = new DebugEvent<int, int> { };

    public int CurrentDay => Mathf.FloorToInt(totalGameTime / secondsPerDay);

    public int CurrentHour
    {
        get
        {
            float timeInDay = totalGameTime % secondsPerDay;
            float normalizedTime = timeInDay / secondsPerDay;
            float hourFloat = (normalizedTime * 24f + startHour) % 24f;
            return Mathf.FloorToInt(hourFloat);
        }
    }

    public int CurrentMinute
    {
        get
        {
            float timeInDay = totalGameTime % secondsPerDay;
            float normalizedTime = timeInDay / secondsPerDay;
            float hourFloat = (normalizedTime * 24f + startHour) % 24f;
            float minuteFloat = (hourFloat - Mathf.FloorToInt(hourFloat)) * 60f;
            return Mathf.FloorToInt(minuteFloat);
        }
    }

    public float CurrentHourFloat
    {
        get
        {
            float timeInDay = totalGameTime % secondsPerDay;
            float normalizedTime = timeInDay / secondsPerDay;
            return (normalizedTime * 24f + startHour) % 24f;
        }
    }

    public float NormalizedTimeOfDay => (totalGameTime % secondsPerDay) / secondsPerDay;
    public string CurrentTimeString => $"{CurrentHour:D2}:{CurrentMinute:D2}";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 초기값 설정
        previousDay = CurrentDay;
        previousHour = CurrentHour;
        previousMinute = CurrentMinute;

        // 시작할 때 한 번 이벤트 발생 (초기 설정을 위해)
        OnTimeChanged?.Invoke(CurrentHour, CurrentMinute);
    }

    private void Update()
    {
        totalGameTime += Time.deltaTime;
        CheckTimeChanges();
    }

    private void CheckTimeChanges()
    {
        int currentDay = CurrentDay;
        int currentHour = CurrentHour;
        int currentMinute = CurrentMinute;

        // 분 변화 체크
        if (currentMinute != previousMinute)
        {
            OnTimeChanged?.Invoke(currentHour, currentMinute);
            previousMinute = currentMinute;
        }

        // 시간 변화 체크
        if (currentHour != previousHour)
        {
            previousHour = currentHour;
        }

        // 날짜 변화 체크
        if (currentDay != previousDay)
        {
            OnDayChanged?.Invoke(currentDay);
            previousDay = currentDay;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 정리 (메모리 누수 방지)
        OnDayChanged = null;
        OnTimeChanged = null;
    }
}