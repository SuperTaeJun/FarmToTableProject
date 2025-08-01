using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class WorldLoadingScene : MonoBehaviour
{
    [Header("Loading UI")]
    public Slider progressBar;

    [Header("Scene")]
    public string mainSceneName = "MainScene";

    [Header("Animation Settings")]
    [SerializeField] private float progressSpeed = 2f;
    private float targetProgress = 0f;
    private Tween progressTween;

    private void OnEnable()
    {
        // 이벤트 등록은 필요시 여기에 추가
    }

    private void OnDisable()
    {
        // 이벤트 등록 해제
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.OnLoadingProgress -= UpdateWorldProgress;
            WorldManager.Instance.OnLoadingComplete -= OnWorldLoadingComplete;
        }
    }

    private async void Start()
    {
        if (WorldManager.Instance == null)
        {
            Debug.LogError("[WorldLoadingScene] WorldManager.Instance가 null입니다!");
            return;
        }

        // 월드 로딩 시작
        await LoadWorldSequentially();
    }

    private async Task LoadWorldSequentially()
    {
        try
        {
            // 1단계: 월드 로딩
            Debug.Log("[WorldLoadingScene] 1단계: 월드 로딩 시작");
            WorldManager.Instance.OnLoadingProgress += UpdateWorldProgress;
            WorldManager.Instance.OnLoadingComplete += OnWorldLoadingComplete;

            await WorldManager.Instance.LoadWorldFromFirebase();

            // 2단계: Forage 로딩
            Debug.Log("[WorldLoadingScene] 2단계: Forage 로딩 시작");
            await LoadForages();

            // 3단계: Building 로딩
            Debug.Log("[WorldLoadingScene] 3단계: Building 로딩 시작");
            await LoadBuildings();

            // 4단계: Crop 로딩
            Debug.Log("[WorldLoadingScene] 4단계: Crop 로딩 시작");
            await LoadCrops();

            // 전체 로딩 완료
            Debug.Log("[WorldLoadingScene] 전체 로딩 완료!");
            OnAllLoadingComplete();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[WorldLoadingScene] 로딩 중 예외 발생: {e.Message}");
        }
    }

    private async Task LoadForages()
    {
        if (ForageManager.Instance != null)
        {
            UpdateProgress(0.3f); // 30% 진행됨
            await ForageManager.Instance.LoadAllForages();
            UpdateProgress(0.5f); // 50% 진행됨
        }
    }

    private async Task LoadBuildings()
    {
        if (BuildingManager.Instance != null)
        {
            UpdateProgress(0.5f); // 50% 진행됨
            await BuildingManager.Instance.LoadAllBuilding();
            UpdateProgress(0.75f); // 75% 진행됨
        }
    }

    private async Task LoadCrops()
    {
        if (CropsManager.Instance != null)
        {
            UpdateProgress(0.75f); // 75% 진행됨
            await CropsManager.Instance.LoadAllCrops();
            UpdateProgress(1.0f); // 100% 완료
        }
    }

    private void UpdateWorldProgress(float worldProgress)
    {
        // 월드 로딩은 전체의 30%를 차지
        float totalProgress = worldProgress * 0.3f;
        UpdateProgress(totalProgress);
    }

    private void OnWorldLoadingComplete()
    {
        // 월드 로딩 완료 시 이벤트 해제
        WorldManager.Instance.OnLoadingProgress -= UpdateWorldProgress;
        WorldManager.Instance.OnLoadingComplete -= OnWorldLoadingComplete;
    }

    private void OnAllLoadingComplete()
    {
        // 전체 로딩이 완료되면 메인 씬으로 이동
        FadeManager.Instance.FadeToScene(mainSceneName);
    }

    private void UpdateProgress(float progress)
    {
        targetProgress = Mathf.Clamp01(progress);

        // 기존 트윈이 있으면 중지
        progressTween?.Kill();

        // 진행바 트윈으로 부드럽게 업데이트
        progressTween = DOTween.To(
            () => progressBar.value,              // 시작 값
            value => progressBar.value = value,   // 업데이트 콜백
            targetProgress,                       // 목표 값
            progressSpeed                         // 지속 시간
        ).SetEase(Ease.OutQuart);
    }
}
