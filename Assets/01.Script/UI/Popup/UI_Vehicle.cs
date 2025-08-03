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

            // 해금 상태 업데이트
            UpdateVehicleLockState(buttonInfo);

            if (VehicleManager.Instance.IsVehicleUnlocked(vehicleType))
            {
                buttonInfo.Button.onClick.AddListener(() => { SpawnVehicle(vehicleType, player); });
            }
        }

        // 차량 해금 이벤트 구독
        VehicleManager.Instance.OnVehicleUnlocked.AddListener(OnVehicleUnlocked);
    }

    private void UpdateVehicleLockState(VehicleButtonInfo buttonInfo)
    {
        bool isUnlocked = VehicleManager.Instance.IsVehicleUnlocked(buttonInfo.VehicleType);

        // 버튼 상호작용 설정
        buttonInfo.Button.interactable = isUnlocked;

        // 잠금 오브젝트만 관리 (해금시 꺼짐, 잠금시 켜짐)
        if (buttonInfo.LockObject != null)
        {
            buttonInfo.LockObject.SetActive(!isUnlocked);
        }

        // 아이콘 색상 조정 (선택사항)
        if (buttonInfo.IconImage != null)
        {
            buttonInfo.IconImage.color = isUnlocked ? Color.white : Color.gray;
        }
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
        // 해금된 차량의 UI 상태 업데이트
        var buttonInfo = _vehicleButtons.Find(info => info.VehicleType == vehicleType);
        if (buttonInfo != null)
        {
            // 잠금 상태 업데이트
            UpdateVehicleLockState(buttonInfo);

            // 이벤트 리스너 추가
            buttonInfo.Button.onClick.RemoveAllListeners();
            Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            buttonInfo.Button.onClick.AddListener(() => { SpawnVehicle(vehicleType, player); });

            Debug.Log($"{vehicleType} 차량 UI가 해금 상태로 업데이트되었습니다.");
        }
    }

    private async void SetupVehicleIcon(VehicleButtonInfo buttonInfo)
    {
        if (buttonInfo.IconImage == null) return;

        // 차량 타입에 해당하는 아이템 타입 찾기
        EItemType itemType = GetItemTypeFromVehicle(buttonInfo.VehicleType);

        // ItemDatabase에서 아이콘 로드
        var iconSprite = await InventoryManager.Instance.ItemDatabase.LoadItemIconAsync(itemType);
        if (iconSprite != null)
        {
            buttonInfo.IconImage.sprite = iconSprite;
        }
        else
        {
            buttonInfo.IconImage.sprite = null;
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
    [Header("기본 설정")]
    public EVehicleType VehicleType;
    public Button Button;
    public Image IconImage;

    [Header("잠금 오브젝트")]
    [Tooltip("잠금 상태일 때 활성화될 오브젝트 (자물쇠 아이콘 등)")]
    public GameObject LockObject;
}