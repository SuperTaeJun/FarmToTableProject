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
        // WorldManager �̺�Ʈ ����
        WorldManager.Instance.OnLoadingProgress += UpdateProgress;
        WorldManager.Instance.OnLoadingComplete += OnLoadingComplete;
    }

    private void OnDisable()
    {
        // �̺�Ʈ ���� ����
        WorldManager.Instance.OnLoadingProgress -= UpdateProgress;
        WorldManager.Instance.OnLoadingComplete -= OnLoadingComplete;
    }
    private async void Start()
    {
        if (WorldManager.Instance == null)
        {
            Debug.LogError("[WorldLoadingScene] WorldManager.Instance�� null!");
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
        // �ε� �Ϸ� �� ���� ������ �̵�
        FadeManager.Instance.FadeToScene(mainSceneName);
        //SceneManager.LoadScene(mainSceneName);
    }
}
