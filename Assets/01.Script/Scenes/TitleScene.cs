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

    //��ư�� ��Ʈ��
    private void ButtonDoTween(GameObject button)
    {
        button.transform.DOKill();

        button.transform.DOPunchScale
            (
                new Vector3(0.3f, 0.3f, 0), // Ŀ���� �۾��� ũ��
                0.3f,                      // ���ӽð�
                10,                        // ���� Ƚ��
                1                          // ź��
            );
    }

}
