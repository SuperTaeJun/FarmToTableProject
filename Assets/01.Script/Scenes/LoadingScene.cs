using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
public class LoadingScene : MonoBehaviour
{
    private const string NEXT_SCENE_NAME = "CharacterSelectScene";
    private const int MIN_LOADING_MS = 2000;
    private const string DEFAULT_USER_ID = "DefaultUser";

    private async void Start()
    {
        try
        {
            // �ּ� �ε� �ð�
            var minLoadingTask = Task.Delay(MIN_LOADING_MS);

            // Firebase �ʱ�ȭ Task
            var firebaseInitTask = FirebaseManager.Instance.InitTask;

            // Customization �ʱ�ȭ Task (Firebase ���Ŀ� ȣ���ؾ� Firestore ��� ����)
            await firebaseInitTask;
            Debug.Log("Firebase �ʱ�ȭ �Ϸ�");

            var customizationLoadTask = CustomizationManager.Instance.LoadCustomizationAsync(DEFAULT_USER_ID);
            Debug.Log("Customization �ε� ����");

            await Task.WhenAll(minLoadingTask, customizationLoadTask);

            Debug.Log("Customization �ʱ�ȭ �Ϸ�");

            MoveNextScene();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"�ε� �� ���� �߻�: {ex.Message}\n{ex.StackTrace}");
            // ���� UI ǥ�� �� ó�� �ʿ�
        }
    }

    private void MoveNextScene()
    {
        SceneManager.LoadScene(NEXT_SCENE_NAME);
    }
}
