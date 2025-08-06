using System;
using UnityEngine;
using System.Threading.Tasks;

public class GameTimeManager : MonoBehaviour,ICurrentGameTimeProvider
{
    public static GameTimeManager Instance { get; private set; }

    [Header("Time Settings")]
    public float secondsPerDay = 60f;
    public int startHour = 12; 

    private float totalGameTime = 0f;
    private int previousDay = 0;
    private int previousHour = 0;
    private int previousMinute = 0;

    // Firebase Repository
    private GameTimeRepository _repository;
    
    // 자동 저장 설정
    private float _saveInterval = 60f; // 60초마다 저장
    private float _saveTimer;

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
    public float TotalGameTime => totalGameTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            _repository = new GameTimeRepository();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private async void Start()
    {
        await LoadGameTime();
        
        previousDay = CurrentDay;
        previousHour = CurrentHour;
        previousMinute = CurrentMinute;

        PlayTimeBasedBGM(CurrentHour);

        OnDayChanged?.Invoke(previousDay);
        OnTimeChanged?.Invoke(CurrentHour, CurrentMinute);
    }

    private void Update()
    {
        totalGameTime += Time.deltaTime;
        CheckTimeChanges();
        
        // 자동 저장 타이머
        _saveTimer += Time.deltaTime;
        if (_saveTimer >= _saveInterval)
        {
            _ = SaveGameTime(); // 비동기 저장
            _saveTimer = 0f;
        }
    }

    private void CheckTimeChanges()
    {
        int currentDay = CurrentDay;
        int currentHour = CurrentHour;
        int currentMinute = CurrentMinute;

        if (currentMinute != previousMinute)
        {
            OnTimeChanged?.Invoke(currentHour, currentMinute);
            previousMinute = currentMinute;
        }

        if (currentHour != previousHour)
        {
            previousHour = currentHour;
            if ((previousHour < 12 && CurrentHour >= 12) || (previousHour >= 12 && CurrentHour < 12))
            {
                PlayTimeBasedBGM(CurrentHour);
            }
        }

        // 날짜 변화 체크
        if (currentDay != previousDay)
        {
            OnDayChanged?.Invoke(currentDay);
            previousDay = currentDay;
            
            // 날짜가 바뀔 때마다 저장
            _ = SaveGameTime();
        }
    }

    public async Task SaveGameTime()
    {
        var gameTimeDto = new GameTimeDto
        {
            CurrentDay = CurrentDay,
            CurrentHour = CurrentHour,
            CurrentMinute = CurrentMinute
        };

        await _repository.SaveGameTimeAsync(gameTimeDto);
    }

    public async Task LoadGameTime()
    {
        var gameTimeDto = await _repository.LoadGameTimeAsync();
        
        if (gameTimeDto != null)
        {

            SetGameTime(gameTimeDto.CurrentDay, gameTimeDto.CurrentHour, gameTimeDto.CurrentMinute);
        }

    }

    private void SetGameTime(int day, int hour, int minute)
    {
        float targetTotalTime = day * secondsPerDay;
        
        float targetHour = (hour - startHour + 24) % 24;
        float targetMinute = minute;
        float timeInDay = (targetHour + targetMinute / 60f) / 24f * secondsPerDay;
        
        totalGameTime = targetTotalTime + timeInDay;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            _ = SaveGameTime();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            _ = SaveGameTime();
        }
    }
    
    private void OnDestroy()
    {
        _ = SaveGameTime();
        
        OnDayChanged = null;
        OnTimeChanged = null;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void PlayTimeBasedBGM(int hour)
    {
        if (hour >= 12)
        {
            SoundManager.Instance.PlayBGM(BGMType.Pm);
        }
        else
        {
            SoundManager.Instance.PlayBGM(BGMType.Am);
        }
    }

    public void GoToNextDayMorning()
    {
        int nextDay = CurrentDay + 1;
        int targetHour = 8;
        int targetMinute = 0;
        
        SetGameTime(nextDay, targetHour, targetMinute);
        
        previousDay = CurrentDay;
        previousHour = CurrentHour;
        previousMinute = CurrentMinute;
        
        PlayTimeBasedBGM(CurrentHour);
        
        OnDayChanged?.Invoke(CurrentDay);
        OnTimeChanged?.Invoke(CurrentHour, CurrentMinute);
        
        _ = SaveGameTime();
        
    }
}