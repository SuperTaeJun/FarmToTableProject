using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ImageSelector : UI_Popup
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Transform _content;
    [SerializeField] private GameObject imageSlotPrefab;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private List<UI_ImageSlot> imageSlots = new List<UI_ImageSlot>();
    private int selectedIndex = -1;

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    public override void Open(System.Action callback = null)
    {
        base.Open(callback);
        
        // 기존 슬롯들 제거
        ClearImageSlots();
        
        // 저장된 이미지들을 슬롯으로 생성
        CreateImageSlots();
        
        // 선택 상태 초기화
        selectedIndex = -1;
        UpdateConfirmButton();
    }

    private void ClearImageSlots()
    {
        foreach (var slot in imageSlots)
        {
            if (slot != null && slot.gameObject != null)
                DestroyImmediate(slot.gameObject);
        }
        imageSlots.Clear();
    }

    private void CreateImageSlots()
    {
        var generatedImages = ImageGenerationManager.Instance.GeneratedImages;
        
        for (int i = 0; i < generatedImages.Count; i++)
        {
            if (generatedImages[i] != null)
            {
                GameObject slotObj = Instantiate(imageSlotPrefab, _content);
                UI_ImageSlot slot = slotObj.GetComponent<UI_ImageSlot>();
                
                if (slot != null)
                {
                    int index = i; 
                    slot.Setup(generatedImages[i], index, OnImageSlotClicked);
                    imageSlots.Add(slot);
                }
            }
        }
    }

    private void OnImageSlotClicked(int index)
    {
        // 이전 선택 해제
        if (selectedIndex >= 0 && selectedIndex < imageSlots.Count)
        {
            imageSlots[selectedIndex].SetSelected(false);
        }
        
        // 새로운 선택 설정
        selectedIndex = index;
        if (selectedIndex >= 0 && selectedIndex < imageSlots.Count)
        {
            imageSlots[selectedIndex].SetSelected(true);
        }
        
        UpdateConfirmButton();
    }

    private void UpdateConfirmButton()
    {
        confirmButton.interactable = selectedIndex >= 0;
    }

    private void OnConfirmButtonClicked()
    {
        if (selectedIndex >= 0)
        {
            ImageGenerationManager.Instance.SelectImage(selectedIndex);

            PopupManager.Instance.PopUpClose();
        }
    }

    private void OnCancelButtonClicked()
    {
        PopupManager.Instance.PopUpClose();
    }
}

