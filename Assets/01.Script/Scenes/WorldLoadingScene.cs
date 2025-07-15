using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class WorldLoadingScene : MonoBehaviour
{
    [Header("Loading UI")]
    public Slider progressBar;

    [Header("Scene")]
    public string mainSceneName = "MainScene";

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
        progressBar.value = progress;
    }

    private void OnLoadingComplete()
    {
        // 로딩 완료 시 메인 씬으로 이동
        FadeManager.Instance.FadeToScene(mainSceneName);
        //SceneManager.LoadScene(mainSceneName);
    }
}
