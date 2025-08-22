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
        SaveButton.onClick.AddListener(OnClickedSaveButton);
        ReturnGameButton.onClick.AddListener(OnClickedReturnGameButton);
        ExitButton.onClick.AddListener(OnClickedReturnToTitle);
    }

    private void OnClickedSaveButton()
    {
        WorldManager.Instance.SaveWorld();
    }
    private void OnClickedReturnGameButton()
    {
        PopupManager.Instance.PopUpClose();
    }
    private void OnClickedReturnToTitle()
    {
        SceneManager.LoadScene("TitleScene"); // 타이틀 씬 로드
    }

}
