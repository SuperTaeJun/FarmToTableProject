using UnityEngine;
using UnityEngine.UI;

public class MainHudManager : MonoBehaviour
{
    public static MainHudManager Instance;

    [SerializeField] private Button _seedSelectButton;
    [SerializeField] private Button _buildingSelectButton;

    [SerializeField] private Sprite[] _modeIconsSprite;
    [SerializeField] private Image _playerModeIcon;
    private void Awake()
    {
        Instance = this;


    }
    private void Start()
    {
        _seedSelectButton.onClick.AddListener(OnClikedSeedSelectButton);
        _buildingSelectButton.onClick.AddListener(OnClikedBuildingSelectButton);
    }
    private void OnClikedSeedSelectButton() => PopupManager.Instance.Open(EPopupType.UI_SeedSelectPopup);
    private void OnClikedBuildingSelectButton() => PopupManager.Instance.Open(EPopupType.UI_BuildingPopup);

    public void RefreshPlayerModeIcon(EPlayerMode curmode)
    {
        _playerModeIcon.sprite = _modeIconsSprite[(int)curmode];
    }

}
