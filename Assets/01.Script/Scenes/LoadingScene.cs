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
            // 최소 로딩 시간
            var minLoadingTask = Task.Delay(MIN_LOADING_MS);

            // Firebase 초기화 Task
            var firebaseInitTask = FirebaseManager.Instance.InitTask;

            // Customization 초기화 Task (Firebase 이후에 호출해야 Firestore 사용 가능)
            await firebaseInitTask;
            Debug.Log("Firebase 초기화 완료");

            var customizationLoadTask = CustomizationManager.Instance.LoadCustomizationAsync(DEFAULT_USER_ID);
            Debug.Log("Customization 로드 시작");

            await Task.WhenAll(minLoadingTask, customizationLoadTask);

            Debug.Log("Customization 초기화 완료");

            MoveNextScene();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"로딩 중 오류 발생: {ex.Message}\n{ex.StackTrace}");
            // 에러 UI 표시 등 처리 필요
        }
    }

    private void MoveNextScene()
    {
        SceneManager.LoadScene(NEXT_SCENE_NAME);
    }
}
