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
        ButtonDoTween(NewGameButton.gameObject);

        FadeManager.Instance.FadeToScene(LODINGSCENE_NAME);
    }

    //버튼용 두트윈
    private void ButtonDoTween(GameObject button)
    {
        button.transform.DOKill();

        button.transform.DOPunchScale
            (
                new Vector3(0.3f, 0.3f, 0), // 커졌다 작아질 크기
                0.3f,                      // 지속시간
                10,                        // 진동 횟수
                1                          // 탄성
            );
    }

}
