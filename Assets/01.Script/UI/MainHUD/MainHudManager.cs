using UnityEngine;
using UnityEngine.UI;

public class MainHudManager : MonoBehaviour
{
    public static MainHudManager Instance;

    [SerializeField] private Button _seedSelectButton;
    [SerializeField] private Button _buildingSelectButton;
    [SerializeField] private Button _InventoryButton;
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
        _InventoryButton.onClick.AddListener(OnClikedInventoryButton);
    }
    private void OnClikedSeedSelectButton() => PopupManager.Instance.Open(EPopupType.UI_SeedSelectPopup);
    private void OnClikedBuildingSelectButton() => PopupManager.Instance.Open(EPopupType.UI_BuildingPopup);
    private void OnClikedInventoryButton() => PopupManager.Instance.Open(EPopupType.UI_InventoryPopup);
    public void RefreshPlayerModeIcon(EPlayerMode curmode)
    {
        _playerModeIcon.sprite = _modeIconsSprite[(int)curmode];
    }

}
