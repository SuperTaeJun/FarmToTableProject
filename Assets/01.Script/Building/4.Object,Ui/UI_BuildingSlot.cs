using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuildingSlot : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private EBuildingType _buildingType;
    
    private System.Action<EBuildingType> _onSelected;
    
    private void Start()
    {
        SetupSlot();
    }
    
    private void SetupSlot()
    {
        // BuildingManager에서 해당 타입의 캐시된 스프라이트 가져오기
        Sprite uiSprite = BuildingManager.Instance?.GetBuildingUISprite(_buildingType);
        if (uiSprite != null)
        {
            _icon.sprite = uiSprite;
        }
        
        // BuildingManager에서 해당 타입의 빌딩 이름 가져오기
        SO_Building buildingData = BuildingManager.Instance?.GetBuildingInfo(_buildingType);
        if (buildingData != null && _nameText != null)
        {
            _nameText.text = buildingData.BuildingName;
        }
        
        // 버튼 이벤트 설정
        _button.onClick.AddListener(() => {
            _onSelected?.Invoke(_buildingType);
        });
    }
    
    public void SetOnSelectedCallback(System.Action<EBuildingType> onSelected)
    {
        _onSelected = onSelected;
    }
    
    public EBuildingType GetBuildingType()
    {
        return _buildingType;
    }

    public void SetBuildingType(EBuildingType buildingType)
    {
        _buildingType = buildingType;
        SetupSlot();
    }
}