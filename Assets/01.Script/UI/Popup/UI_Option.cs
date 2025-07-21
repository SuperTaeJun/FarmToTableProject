using UnityEngine;
using UnityEngine.UI;
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
    }

    private void OnClikedSaveButton()
    {
        WorldManager.Instance.SaveWorld();
    }
    private void OnClikedReturnGameButton()
    {
        Close();
    }
}
