using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class UI_Option : UI_Popup
{
    [SerializeField] private Button ReturnGameButton;
    [SerializeField] private Button SaveButton;
    [SerializeField] private Button ExitButton;
    [SerializeField] private Button SetupButton;

    void Start()
    {
        SaveButton.onClick.AddListener(OnClikedSaveButton);
        ReturnGameButton.onClick.AddListener(OnClikedReturnGameButton);
        ExitButton.onClick.AddListener(OnClikedReturnToTitle);
    }

    private void OnClikedSaveButton()
    {
        WorldManager.Instance.SaveWorld();
    }
    private void OnClikedReturnGameButton()
    {
        Close();
    }
    private void OnClikedReturnToTitle()
    {
        // 모든 DontDestroyOnLoad 객체들 찾아서 삭제
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.scene.name == "DontDestroyOnLoad")
            {
                Destroy(obj);
            }
        }

        SceneManager.LoadScene("TitleScene"); // 타이틀 씬 로드
    }

}
