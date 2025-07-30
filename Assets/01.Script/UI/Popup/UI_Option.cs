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
        SceneManager.LoadScene("TitleScene"); // 타이틀 씬 로드
    }

}
