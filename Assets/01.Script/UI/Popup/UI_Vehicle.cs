using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Vehicle : UI_Popup
{
    [SerializeField] private List<VehicleButtonInfo> _vehicleButtons = new List<VehicleButtonInfo>();
    [SerializeField] private Button _closeButton;
    
    private void Start()
    {
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        
        _closeButton.onClick.AddListener(Close);
        
        // 각 버튼에 이벤트 연결 및 해금 상태 확인
        foreach (var buttonInfo in _vehicleButtons)
        {
            EVehicleType vehicleType = buttonInfo.VehicleType;
            
            // 아이콘 설정
            SetupVehicleIcon(buttonInfo);
            
            // 해금 상태에 따른 버튼 활성화/비활성화
            bool isUnlocked = VehicleManager.Instance.IsVehicleUnlocked(vehicleType);
            buttonInfo.Button.interactable = isUnlocked;
            
            if (isUnlocked)
            {
                buttonInfo.Button.onClick.AddListener(() => { SpawnVehicle(vehicleType, player); });
            }
        }
        
        // 차량 해금 이벤트 구독
        VehicleManager.Instance.OnVehicleUnlocked.AddListener(OnVehicleUnlocked);
    }
    

    private void SpawnVehicle(EVehicleType vehicleType, Player player)
    {
        if (VehicleManager.Instance == null)
        {
            Debug.LogError("VehicleManager가 없습니다!");
            return;
        }

        Vector3 spawnPosition = player.transform.position;
        Vehicle vehicle = VehicleManager.Instance.SpawnVehicle(vehicleType, spawnPosition, player, player.transform.rotation);
        
        if (vehicle != null)
        {
            Close();
        }
    }
    
    private void OnVehicleUnlocked(EVehicleType vehicleType)
    {
        // 해금된 차량의 버튼 활성화
        var buttonInfo = _vehicleButtons.Find(info => info.VehicleType == vehicleType);
        if (buttonInfo != null)
        {
            buttonInfo.Button.interactable = true;
            
            // 이벤트 리스너 추가 (중복 방지를 위해 한번 제거 후 추가)
            buttonInfo.Button.onClick.RemoveAllListeners();
            Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            buttonInfo.Button.onClick.AddListener(() => { SpawnVehicle(vehicleType, player); });
        }
    }
    
    private async void SetupVehicleIcon(VehicleButtonInfo buttonInfo)
    {
        if (buttonInfo.IconImage == null) return;
        
        // 차량 타입에 해당하는 아이템 타입 찾기
        EItemType itemType = GetItemTypeFromVehicle(buttonInfo.VehicleType);
        if (itemType == EItemType.Bike1) // 기본값이므로 실제로는 None을 체크해야 하지만 일단 이렇게
        {
            buttonInfo.IconImage.sprite = null;
            buttonInfo.IconImage.color = Color.gray;
            return;
        }
        
        // ItemDatabase에서 아이콘 로드
        var iconSprite = await InventoryManager.Instance.ItemDatabase.LoadItemIconAsync(itemType);
        if (iconSprite != null)
        {
            buttonInfo.IconImage.sprite = iconSprite;
            buttonInfo.IconImage.color = Color.white;
        }
        else
        {
            buttonInfo.IconImage.sprite = null;
            buttonInfo.IconImage.color = Color.gray;
        }
    }
    
    private EItemType GetItemTypeFromVehicle(EVehicleType vehicleType)
    {
        // VehicleManager의 매핑을 역순으로 찾기
        foreach (var kvp in VehicleManager.Instance.ItemToVehicleMap)
        {
            if (kvp.Value == vehicleType)
                return kvp.Key;
        }
        return EItemType.Bike1; // None이 없으므로 기본값
    }
    
    private void OnDestroy()
    {
        if (VehicleManager.Instance != null)
        {
            VehicleManager.Instance.OnVehicleUnlocked.RemoveListener(OnVehicleUnlocked);
        }
    }
}

[Serializable]
public class VehicleButtonInfo
{
    public EVehicleType VehicleType;
    public Button Button;
    public Image IconImage;  // 차량 아이콘 이미지
}