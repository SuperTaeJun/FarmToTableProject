using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

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

    }
    private void OnDisable()
    {
        // 이벤트 구독 해제
        WorldManager.Instance.OnLoadingProgress -= UpdateProgress;
        WorldManager.Instance.OnLoadingComplete -= OnLoadingComplete;
    }
    private async void Start()
    {
        // WorldManager 이벤트 구독
        WorldManager.Instance.OnLoadingProgress += UpdateProgress;
        WorldManager.Instance.OnLoadingComplete += OnLoadingComplete;

        if (WorldManager.Instance == null)
        {
            Debug.LogError("[WorldLoadingScene] WorldManager.Instance가 null!");
            return;
        }

        await WorldManager.Instance.LoadWorldFromFirebase();
    }
    private void UpdateProgress(float progress)
    {
        //progressBar.value = progress;
        targetProgress = Mathf.Clamp01(progress);

        // 기존 트윈이 있다면 중단
        progressTween?.Kill();

        // 현재 값에서 목표 값까지 부드럽게 보간
        progressTween = DOTween.To(
            () => progressBar.value,           // 현재 값
            value => progressBar.value = value, // 값 설정
            targetProgress,                    // 목표 값
            progressSpeed                      // 지속 시간
        ).SetEase(DG.Tweening.Ease.OutQuart);
    }

    private void OnLoadingComplete()
    {
        // 로딩 완료 시 메인 씬으로 이동
        FadeManager.Instance.FadeToScene(mainSceneName);
        //SceneManager.LoadScene(mainSceneName);
    }
}
