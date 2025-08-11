using UnityEngine;
using UnityEngine.UI;

public class UI_Building : UI_Popup
{
    [Header("탭")]
    [SerializeField] private Button _functionalTabButton;
    [SerializeField] private Button _decorativeTabButton;

    [Header("패널")]
    [SerializeField] private Transform _functionalPenal;
    [SerializeField] private Transform _decorativePenal;

    [Header("콘텐츠 트렌스폼")]
    [SerializeField] private Transform _functionalContent;
    [SerializeField] private Transform _decorativeContent;
    
    [Header("슬롯")]
    [SerializeField] private GameObject _buildingSlotPrefab;
    
    [SerializeField] private Button _closeButton;
    
    private PlayerBuildAbility _buildAbility;
    private EBuildingCategory _currentCategory = EBuildingCategory.Functional;

    private void Start()
    {
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        _buildAbility = player.GetAbility<PlayerBuildAbility>();

        SetupUI();
        SetupSlots();
        ShowCategory(_currentCategory);
    }

    private void SetupUI()
    {
        _closeButton.onClick.AddListener(Close);
        _functionalTabButton.onClick.AddListener(() => ShowCategory(EBuildingCategory.Functional));
        _decorativeTabButton.onClick.AddListener(() => ShowCategory(EBuildingCategory.Decorative));
    }

    private void SetupSlots()
    {
        // BuildingManager에서 빌딩 데이터 가져와서 슬롯 생성
        CreateSlotsForCategory(EBuildingCategory.Functional, _functionalContent);
        CreateSlotsForCategory(EBuildingCategory.Decorative, _decorativeContent);
    }

    private void CreateSlotsForCategory(EBuildingCategory category, Transform parent)
    {
        // 기존 슬롯들 제거
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }

        // BuildingManager에서 해당 카테고리의 건물들 찾기
        SO_Building[] allBuildingData = BuildingManager.Instance.GetAllBuildingData();
        
        foreach (var buildingData in allBuildingData)
        {
            if (buildingData.Category == category)
            {
                // 슬롯 생성
                GameObject slotObj = Instantiate(_buildingSlotPrefab, parent);
                UI_BuildingSlot slot = slotObj.GetComponent<UI_BuildingSlot>();
                
                // 슬롯 설정
                slot.SetBuildingType(buildingData.Type);
                slot.SetOnSelectedCallback(OnBuildingSelected);
            }
        }
    }

    private void OnBuildingSelected(EBuildingType buildingType)
    {
        _buildAbility.SetSelectedType(buildingType);
        Close();
    }

    private void ShowCategory(EBuildingCategory category)
    {
        _currentCategory = category;
        
        // 탭 버튼 상태 업데이트
        UpdateTabButtons();
        
        // 컨텐츠 표시/숨김
        _functionalPenal.gameObject.SetActive(category == EBuildingCategory.Functional);
        _decorativePenal.gameObject.SetActive(category == EBuildingCategory.Decorative);
    }

    private void UpdateTabButtons()
    {
        // 탭 버튼 색상이나 상태 변경 로직
        // 예: 선택된 탭은 다른 색상으로 표시
        ColorBlock functionalColors = _functionalTabButton.colors;
        ColorBlock decorativeColors = _decorativeTabButton.colors;
        
        if (_currentCategory == EBuildingCategory.Functional)
        {
            functionalColors.normalColor = Color.yellow;
            decorativeColors.normalColor = Color.white;
        }
        else
        {
            functionalColors.normalColor = Color.white;
            decorativeColors.normalColor = Color.yellow;
        }
        
        _functionalTabButton.colors = functionalColors;
        _decorativeTabButton.colors = decorativeColors;
    }
}
