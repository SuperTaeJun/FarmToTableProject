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
        // �̺�Ʈ ���� ����
        WorldManager.Instance.OnLoadingProgress -= UpdateProgress;
        WorldManager.Instance.OnLoadingComplete -= OnLoadingComplete;
    }
    private async void Start()
    {
        // WorldManager �̺�Ʈ ����
        WorldManager.Instance.OnLoadingProgress += UpdateProgress;
        WorldManager.Instance.OnLoadingComplete += OnLoadingComplete;

        if (WorldManager.Instance == null)
        {
            Debug.LogError("[WorldLoadingScene] WorldManager.Instance�� null!");
            return;
        }

        await WorldManager.Instance.LoadWorldFromFirebase();
    }
    private void UpdateProgress(float progress)
    {
        //progressBar.value = progress;
        targetProgress = Mathf.Clamp01(progress);

        // ���� Ʈ���� �ִٸ� �ߴ�
        progressTween?.Kill();

        // ���� ������ ��ǥ ������ �ε巴�� ����
        progressTween = DOTween.To(
            () => progressBar.value,           // ���� ��
            value => progressBar.value = value, // �� ����
            targetProgress,                    // ��ǥ ��
            progressSpeed                      // ���� �ð�
        ).SetEase(DG.Tweening.Ease.OutQuart);
    }

    private void OnLoadingComplete()
    {
        // �ε� �Ϸ� �� ���� ������ �̵�
        FadeManager.Instance.FadeToScene(mainSceneName);
        //SceneManager.LoadScene(mainSceneName);
    }
}
