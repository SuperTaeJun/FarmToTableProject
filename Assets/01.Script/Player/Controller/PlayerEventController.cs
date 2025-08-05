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


    private void SubscribeToInputEvents()
    {
        if (_isInputEventsSubscribed || _owner.GetAbility<PlayerInputAbility>() == null) return;

        _owner.GetAbility<PlayerInputAbility>()?.OnChunkPurchaseInput.AddListener(_owner.TryGenerateChunk);
        _owner.GetAbility<PlayerInputAbility>()?.OnBuildingInteractionInput.AddListener(_owner.TryInteractWithBuilding);
        
        _isInputEventsSubscribed = true;
    }

    private void UnsubscribeFromInputEvents()
    {
        if (!_isInputEventsSubscribed || _owner.GetAbility<PlayerInputAbility>() == null) return;

        _owner.GetAbility<PlayerInputAbility>()?.OnChunkPurchaseInput.RemoveListener(_owner.TryGenerateChunk);
        _owner.GetAbility<PlayerInputAbility>()?.OnBuildingInteractionInput.RemoveListener(_owner.TryInteractWithBuilding);
        
        _isInputEventsSubscribed = false;
    }

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

}