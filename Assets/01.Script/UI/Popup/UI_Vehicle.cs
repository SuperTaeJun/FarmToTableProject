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
        
        // 각 버튼에 이벤트 연결
        foreach (var buttonInfo in _vehicleButtons)
        {
            EVehicleType vehicleType = buttonInfo.VehicleType;
            buttonInfo.Button.onClick.AddListener(() => { SpawnVehicle(vehicleType, player); });
        }
    }
    
    private void SpawnVehicle(EVehicleType vehicleType, Player player)
    {
        if (VehicleManager.Instance == null)
        {
            Debug.LogError("VehicleManager가 없습니다!");
            return;
        }
        
        // 플레이어 위치에서 차량 생성
        Vector3 spawnPosition = player.transform.position + player.transform.forward * 5f;
        
        Vehicle vehicle = VehicleManager.Instance.SpawnVehicle(vehicleType, spawnPosition, player);
        
        if (vehicle != null)
        {
            Debug.Log($"{vehicleType} 차량이 생성되었습니다!");
            Close(); // 팝업 닫기
        }
        else
        {
            Debug.LogError($"{vehicleType} 차량 생성에 실패했습니다!");
        }
    }
}

[Serializable]
public class VehicleButtonInfo
{
    public EVehicleType VehicleType;
    public Button Button;
}