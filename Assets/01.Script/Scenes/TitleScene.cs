using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TitleScene : MonoBehaviour
{
    [SerializeField]private Button NewGameButton;
    [SerializeField] private Button LoadGameButton;

    private const string LODINGSCENE_NAME = "LodingScene";
    private void Awake()
    {
        NewGameButton.onClick.AddListener(() => OnNewGameButtonClicked());
    }

    private void OnNewGameButtonClicked()
    {
        FadeManager.Instance.FadeToScene(LODINGSCENE_NAME);
    }
}
