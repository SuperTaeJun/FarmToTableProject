using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Inventory : UI_Popup
{
    [Header("UI 컴포넌트")]
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private Transform _slotsParent;
    [SerializeField] private Button _closeButton;

    [Header("정보 패널")]
    [SerializeField] private TextMeshProUGUI _slotCountText;

    private List<UI_InventorySlot> _slots = new List<UI_InventorySlot>();
    private InventoryManager _inventoryManager;

    private void Awake()
    {
        if (_closeButton != null)
            _closeButton.onClick.AddListener(Close);
    }

    private void Start()
    {
        _inventoryManager = InventoryManager.Instance;
        
        if (_inventoryManager != null)
        {
            // 이벤트 구독
            _inventoryManager.OnInventoryChanged.AddListener(UpdateUI);
            
            InitializeSlots();
            UpdateUI();
        }
    }

    private void InitializeSlots()
    {
        // 기존 슬롯들 제거
        foreach (Transform child in _slotsParent)
        {
            Destroy(child.gameObject);
        }
        _slots.Clear();

        // 새 슬롯들 생성
        for (int i = 0; i < _inventoryManager.MaxSlots; i++)
        {
            GameObject slotObj = Instantiate(_slotPrefab, _slotsParent);
            UI_InventorySlot slot = slotObj.GetComponent<UI_InventorySlot>();
            
            if (slot != null)
            {
                slot.Initialize(i);
                _slots.Add(slot);
            }
        }
    }

    private void UpdateUI()
    {
        if (_inventoryManager == null) return;

        // 슬롯 정보 업데이트
        UpdateSlotCountDisplay();

        // 모든 슬롯 초기화
        foreach (var slot in _slots)
        {
            slot.ClearSlot();
        }

        // 아이템들을 슬롯에 배치
        var items = _inventoryManager.Items;
        for (int i = 0; i < items.Count && i < _slots.Count; i++)
        {
            _slots[i].SetItem(items[i]);
        }
    }

    private void UpdateSlotCountDisplay()
    {
        if (_slotCountText != null)
        {
            _slotCountText.text = $"{_inventoryManager.UsedSlots}/{_inventoryManager.MaxSlots}";
        }
    }

    private void OnDestroy()
    {
        if (_inventoryManager != null)
        {
            _inventoryManager.OnInventoryChanged.RemoveListener(UpdateUI);
        }

        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(Close);
    }
}