using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

public class AchievmentManager : MonoBehaviour
{
    public static AchievmentManager Instance { get; private set; }

    private AchievmentRepository _repository;
    private List<Achievment> _activeAchievements = new List<Achievment>();
    private List<Achievment> _predefinedAchievements = new List<Achievment>();

    // 자동 저장 설정
    private float _saveInterval = 30f;
    private float _saveTimer;

    // UI 업데이트용 이벤트
    public DebugEvent<Achievment> OnAchievementCompleted = new DebugEvent<Achievment>();
    public DebugEvent<Achievment> OnAchievementProgressUpdated = new DebugEvent<Achievment>();
    public DebugEvent<List<Achievment>> OnAchievementsLoaded = new DebugEvent<List<Achievment>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _repository = new AchievmentRepository();
            InitializePredefinedAchievements();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private async void Start()
    {
        await LoadAchievements();
        
        // 로드된 업적이 없으면 새로 생성
        if (_activeAchievements.Count == 0)
        {
            GenerateDailyWeeklyAchievements();
        }

        // 게임 매니저 이벤트 구독
        SubscribeToGameEvents();
    }

    private void Update()
    {
        // 자동 저장 타이머
        _saveTimer += Time.deltaTime;
        if (_saveTimer >= _saveInterval)
        {
            _ = SaveAchievements();
            _saveTimer = 0f;
        }
    }

    private void InitializePredefinedAchievements()
    {
        _predefinedAchievements.Clear();

        // 일 퀘
        _predefinedAchievements.Add(new Achievment(
            "식물 심기", "오늘 식물을 3개 심으세요", EAchievmentCategory.Daily,
            EAchievementType.PlantCrop, 3, ECurrencyType.Money, 100, 0));

        _predefinedAchievements.Add(new Achievment(
            "작물 수확", "오늘 작물을 5개 수확하세요", EAchievmentCategory.Daily,
            EAchievementType.HarvestCrop, 5, ECurrencyType.Money, 150, 0));

        _predefinedAchievements.Add(new Achievment(
            "물주기", "오늘 식물에 물을 10번 주세요", EAchievmentCategory.Daily,
            EAchievementType.WaterCrop, 10, ECurrencyType.Money, 50, 0));

        // 주간 퀘
        _predefinedAchievements.Add(new Achievment(
            "건물 건설", "이번 주에 건물을 2개 건설하세요", EAchievmentCategory.Weekly,
            EAchievementType.BuildBuilding, 2, ECurrencyType.Gem, 5, 0));

        _predefinedAchievements.Add(new Achievment(
            "상점 구매", "이번 주에 상점에서 10번 구매하세요", EAchievmentCategory.Weekly,
            EAchievementType.BuyFromShop, 10, ECurrencyType.Money, 500, 0));

        // 튜토리얼
        _predefinedAchievements.Add(new Achievment(
            "첫 작물", "첫 작물을 심어보세요", EAchievmentCategory.Tutorial,
            EAchievementType.PlantCrop, 1, ECurrencyType.Money, 200, 0));
    }

    private void GenerateDailyWeeklyAchievements()
    {
        int currentDay = GameTimeManager.Instance.CurrentDay;

        // Daily achievements 생성/갱신
        var dailyAchievements = _predefinedAchievements.Where(a => a.Category == EAchievmentCategory.Daily).ToList();
        foreach (var template in dailyAchievements)
        {
            var existing = _activeAchievements.FirstOrDefault(a =>
                a.Name == template.Name && a.CreatedDay == currentDay);

            if (existing == null)
            {
                var newDaily = new Achievment(
                    template.Name, template.Description, template.Category,
                    template.AchievementType, template.TargetValue,
                    template.Reward, template.RewardAmount, currentDay);

                _activeAchievements.Add(newDaily);
            }
        }

        // Weekly achievements 생성/갱신 (7일마다)
        int currentWeek = currentDay / 7;
        var weeklyAchievements = _predefinedAchievements.Where(a => a.Category == EAchievmentCategory.Weekly).ToList();

        foreach (var template in weeklyAchievements)
        {
            var existing = _activeAchievements.FirstOrDefault(a =>
                a.Name == template.Name && a.CreatedDay / 7 == currentWeek);

            if (existing == null)
            {
                var newWeekly = new Achievment(
                    template.Name, template.Description, template.Category,
                    template.AchievementType, template.TargetValue,
                    template.Reward, template.RewardAmount, currentDay);

                _activeAchievements.Add(newWeekly);
            }
        }

        // Tutorial achievements는 한 번만 생성
        var tutorialAchievements = _predefinedAchievements.Where(a => a.Category == EAchievmentCategory.Tutorial).ToList();
        foreach (var template in tutorialAchievements)
        {
            var existing = _activeAchievements.FirstOrDefault(a => a.Name == template.Name);

            if (existing == null)
            {
                var newTutorial = new Achievment(
                    template.Name, template.Description, template.Category,
                    template.AchievementType, template.TargetValue,
                    template.Reward, template.RewardAmount, currentDay);

                _activeAchievements.Add(newTutorial);
            }
        }
        
        // UI 업데이트 이벤트 발생
        OnAchievementsLoaded?.Invoke(_activeAchievements);
    }

    private void SubscribeToGameEvents()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnDayChanged.AddListener(OnDayChanged);
        }
        // 심기,물주기,수확하기 관련 이벤트
        CropsManager.Instance.OnReportAchievement.AddListener(ReportProgress);

        // 돈쓰는거 관련 이벤트

    }

    private void OnDayChanged(int newDay)
    {
        // 완료되지 않은 이전 일일 도전과제 제거
        RemoveExpiredDailyAchievements(newDay);
        // 새로운 일일 도전과제 생성
        GenerateDailyWeeklyAchievements();
    }

    private void RemoveExpiredDailyAchievements(int currentDay)
    {
        var expiredDailies = _activeAchievements
            .Where(a => a.Category == EAchievmentCategory.Daily &&
                       a.CreatedDay < currentDay &&
                       !a.IsCompleted)
            .ToList();

        foreach (var expired in expiredDailies)
        {
            _activeAchievements.Remove(expired);
        }
    }

    public void ReportProgress(EAchievementType achievementType, int amount = 1)
    {
        var relevantAchievements = _activeAchievements
            .Where(a => a.AchievementType == achievementType && !a.IsCompleted)
            .ToList();

        foreach (var achievement in relevantAchievements)
        {
            bool wasCompleted = achievement.TryProgress(amount);

            OnAchievementProgressUpdated?.Invoke(achievement);
            if (wasCompleted)
            {
                CompleteAchievement(achievement);
            }
        }
    }

    public async void CompleteAchievement(Achievment achievement)
    {
        if (achievement.IsCompleted)
        {
            OnAchievementCompleted?.Invoke(achievement);

            // 보상 지급
            if (CurrencyManager.Instance != null)
            {
                bool success = await CurrencyManager.Instance.TryEarnCurrency(achievement.Reward, achievement.RewardAmount);

                if (success) Debug.Log($"업적으로 재화 획득 {achievement.Reward.ToString()}");
            }

        }
    }

    public List<Achievment> GetActiveAchievements(EAchievmentCategory? category = null)
    {
        if (category == null)
            return new List<Achievment>(_activeAchievements);

        return _activeAchievements.Where(a => a.Category == category.Value).ToList();
    }

    public List<Achievment> GetCompletedAchievements()
    {
        return _activeAchievements.Where(a => a.IsCompleted).ToList();
    }

    public List<Achievment> GetIncompleteAchievements()
    {
        return _activeAchievements.Where(a => !a.IsCompleted).ToList();
    }

    public async Task SaveAchievements()
    {
        await _repository.SaveAchievements(_activeAchievements);
    }

    public async Task LoadAchievements()
    {
        var loadedAchievements = await _repository.LoadAchievements();

        if (loadedAchievements != null && loadedAchievements.Count > 0)
        {
            _activeAchievements = loadedAchievements;
            OnAchievementsLoaded?.Invoke(_activeAchievements);
        }
    }

    private void OnDestroy()
    {
        _ = SaveAchievements();

        // 이벤트 구독 해제
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnDayChanged.RemoveListener(OnDayChanged);
        }

        OnAchievementCompleted = null;
        OnAchievementProgressUpdated = null;
        OnAchievementsLoaded = null;

        if (Instance == this)
        {
            Instance = null;
        }
    }


}
