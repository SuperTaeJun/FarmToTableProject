using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using DG.Tweening;

public class TitleScene : MonoBehaviour
{
    [SerializeField] private Button NewGameButton;
    [SerializeField] private Button LoadGameButton;


    [Header("확인 팝업")]
    [SerializeField] private Transform ConfirmPopup;
    [SerializeField] private Button ConfirmButton;
    [SerializeField] private Button CancelButton;

    private const string LODINGSCENE_NAME = "LodingScene";
    
    private void Awake()
    {
        NewGameButton.onClick.AddListener(() => OnNewGameButtonClicked());
        LoadGameButton.onClick.AddListener(() => OnLoadGameButtonClicked());
        ConfirmButton.onClick.AddListener(() => OnConfirmButtonClicked());
        CancelButton.onClick.AddListener(() => OnCancelButtonClicked());
    }
    private void Start()
    {
        SoundManager.Instance.PlayBGM(BGMType.Title);
    }

    private void OnNewGameButtonClicked()
    {
        ConfirmPopup.gameObject.SetActive(true);
        ConfirmPopup.localScale = Vector3.zero;
        ConfirmPopup.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

    }
    private async void OnConfirmButtonClicked()
    {
        await DeleteAllGameData();
        FadeManager.Instance.FadeToScene(LODINGSCENE_NAME);
        ConfirmPopup.gameObject.SetActive(false);
    }
    private void OnCancelButtonClicked()
    {
        ConfirmPopup.gameObject.SetActive(false);
    }

    private void OnLoadGameButtonClicked()
    {
        FadeManager.Instance.FadeToScene(LODINGSCENE_NAME);
    }

    private async Task DeleteAllGameData()
    {
        try
        {
            Debug.Log("게임 데이터 삭제 시작...");

            await Task.WhenAll(
                new AchievmentRepository().DeleteAllData(),
                new CharacterCustomizationRepository().DeleteAllData(),
                new CurrencyRepository().DeleteAllData(),
                new InventoryRepository().DeleteAllData(),
                new PlayerDataRepository().DeleteAllData(),
                new GameTimeRepository().DeleteAllData(),
                new ForageRepository().DeleteAllData(),
                new WorldRepository().DeleteAllData(),
                new BuildingRepository().DeleteAllData(),
                new CropRepository().DeleteAllData()
            );
            //PlayerPrefs.DeleteAll(); // PlayerPrefs 데이터도 삭제
            Debug.Log("게임 데이터 삭제 완료!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"게임 데이터 삭제 중 오류 발생: {e.Message}");
        }
    }
}
