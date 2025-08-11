using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class TitleScene : MonoBehaviour
{
    [SerializeField]private Button NewGameButton;
    [SerializeField] private Button LoadGameButton;

    private const string LODINGSCENE_NAME = "LodingScene";
    
    private void Awake()
    {
        NewGameButton.onClick.AddListener(() => OnNewGameButtonClicked());
        LoadGameButton.onClick.AddListener(() => OnLoadGameButtonClicked());
    }
    private void Start()
    {
        SoundManager.Instance.PlayBGM(BGMType.Title);
    }

    private async void OnNewGameButtonClicked()
    {
        await DeleteAllGameData();
        FadeManager.Instance.FadeToScene(LODINGSCENE_NAME);
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
                new BuildingRepository().DeleteAllData()
            );

            Debug.Log("게임 데이터 삭제 완료!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"게임 데이터 삭제 중 오류 발생: {e.Message}");
        }
    }
}
