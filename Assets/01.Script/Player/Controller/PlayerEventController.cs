using System;
using UnityEngine;

public class PlayerEventController
{
    private Player _owner;

    // 이벤트 리스너들을 관리하기 위한 플래그들
    private bool _isInputEventsSubscribed = false;
    private bool _isBuildingEventsSubscribed = false;
    private bool _isVehicleEventsSubscribed = false;

    public PlayerEventController(Player owner)
    {
        _owner = owner;
    }

    public void Initialize()
    {
        SubscribeToInputEvents();
        SubscribeToBuildingEvents();
        SubscribeToVehicleEvents();
    }

    public void Cleanup()
    {
        UnsubscribeFromInputEvents();
        UnsubscribeFromBuildingEvents();
        UnsubscribeFromVehicleEvents();
    }

    #region Input Events
    private void SubscribeToInputEvents()
    {
        if (_isInputEventsSubscribed || _owner.InputController == null) return;

        _owner.InputController.OnChunkPurchaseInput.AddListener(_owner.TryGenerateChunk);
        _owner.InputController.OnBuildingInteractionInput.AddListener(_owner.TryInteractWithBuilding);
        
        _isInputEventsSubscribed = true;
    }

    private void UnsubscribeFromInputEvents()
    {
        if (!_isInputEventsSubscribed || _owner.InputController == null) return;

        _owner.InputController.OnChunkPurchaseInput.RemoveListener(_owner.TryGenerateChunk);
        _owner.InputController.OnBuildingInteractionInput.RemoveListener(_owner.TryInteractWithBuilding);
        
        _isInputEventsSubscribed = false;
    }
    #endregion

    #region Building Events
    private void SubscribeToBuildingEvents()
    {
        if (_isBuildingEventsSubscribed || BuildingManager.Instance == null) return;

        BuildingManager.Instance.OnBuildingEnterRange.AddListener(_owner.OnBuildingEnterRange);
        BuildingManager.Instance.OnBuildingExitRange.AddListener(_owner.OnBuildingExitRange);
        
        _isBuildingEventsSubscribed = true;
    }

    private void UnsubscribeFromBuildingEvents()
    {
        if (!_isBuildingEventsSubscribed || BuildingManager.Instance == null) return;

        BuildingManager.Instance.OnBuildingEnterRange.RemoveListener(_owner.OnBuildingEnterRange);
        BuildingManager.Instance.OnBuildingExitRange.RemoveListener(_owner.OnBuildingExitRange);
        
        _isBuildingEventsSubscribed = false;
    }
    #endregion

    #region Vehicle Events
    private void SubscribeToVehicleEvents()
    {
        if (_isVehicleEventsSubscribed || VehicleManager.Instance == null || _owner.DataController == null) return;

        VehicleManager.Instance.OnVehicleDataChanged.AddListener(_owner.DataController.OnVehicleDataChanged);
        
        _isVehicleEventsSubscribed = true;
    }

    private void UnsubscribeFromVehicleEvents()
    {
        if (!_isVehicleEventsSubscribed || VehicleManager.Instance == null || _owner.DataController == null) return;

        VehicleManager.Instance.OnVehicleDataChanged.RemoveListener(_owner.DataController.OnVehicleDataChanged);
        
        _isVehicleEventsSubscribed = false;
    }
    #endregion

    // 런타임에서 특정 이벤트 그룹을 다시 구독하는 메서드들
    public void RefreshInputEvents()
    {
        UnsubscribeFromInputEvents();
        SubscribeToInputEvents();
    }

    public void RefreshBuildingEvents()
    {
        UnsubscribeFromBuildingEvents();
        SubscribeToBuildingEvents();
    }

    public void RefreshVehicleEvents()
    {
        UnsubscribeFromVehicleEvents();
        SubscribeToVehicleEvents();
    }

    // 전체 이벤트 새로고침
    public void RefreshAllEvents()
    {
        Cleanup();
        Initialize();
    }
}