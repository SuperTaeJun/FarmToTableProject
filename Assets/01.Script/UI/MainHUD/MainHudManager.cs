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
    
    [Header("재화 UI")]
    [SerializeField] private UI_CurrencyPanel _currencyPanel;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        _seedSelectButton.onClick.AddListener(OnClikedSeedSelectButton);
        _buildingSelectButton.onClick.AddListener(OnClikedBuildingSelectButton);
        _InventoryButton.onClick.AddListener(OnClikedInventoryButton);
        
        InitializeCurrencyUI();
    }
    
    private void InitializeCurrencyUI()
    {
        if (_currencyPanel != null)
        {
            _currencyPanel.RefreshAllDisplays();
        }
    }
    private void OnClikedSeedSelectButton() => PopupManager.Instance.Open(EPopupType.UI_SeedSelectPopup);
    private void OnClikedBuildingSelectButton() => PopupManager.Instance.Open(EPopupType.UI_BuildingPopup);
    private void OnClikedInventoryButton() => PopupManager.Instance.Open(EPopupType.UI_InventoryPopup);
    public void RefreshPlayerModeIcon(EPlayerMode curmode)
    {
        _playerModeIcon.sprite = _modeIconsSprite[(int)curmode];
    }
    
    #region 재화 UI 관리
    public void RefreshCurrencyUI()
    {
        if (_currencyPanel != null)
        {
            _currencyPanel.RefreshAllDisplays();
        }
    }
    
    public void ShowCurrency(ECurrencyType currencyType, bool show = true)
    {
        if (_currencyPanel != null)
        {
            _currencyPanel.ShowCurrency(currencyType, show);
        }
    }
    
    public void HideCurrency(ECurrencyType currencyType)
    {
        ShowCurrency(currencyType, false);
    }
    
    public UI_CurrencyDisplay GetCurrencyDisplay(ECurrencyType currencyType)
    {
        return _currencyPanel?.GetCurrencyDisplay(currencyType);
    }
    #endregion

}
