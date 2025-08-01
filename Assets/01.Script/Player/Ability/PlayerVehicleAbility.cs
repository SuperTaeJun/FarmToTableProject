using UnityEngine;

public class PlayerVehicleAbility : PlayerAbility
{
    private Vehicle currentVehicle;
    
    private void Start()
    {
        // Vehicle 모드에서만 활성화되도록 모드 변경 이벤트 구독
        _owner.ModeController.OnModeChanged.AddListener(OnModeChanged);
    }
    
    private void Update()
    {
        if (_owner.ModeController.CurrentMode != EPlayerMode.Vehicle)
            return;
            
        if (currentVehicle == null)
        {
            // 현재 차량 찾기
            currentVehicle = VehicleManager.Instance.GetPlayerVehicle(_owner);
            if (currentVehicle == null)
            {
                // 차량이 없으면 모드를 기본으로 변경
                _owner.ModeController.SwitchMode(EPlayerMode.BlockEdit);
                return;
            }
        }
        
        HandleVehicleInput();
    }
    
    private void HandleVehicleInput()
    {
        if (currentVehicle == null) return;
        
        // 이동 입력
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        currentVehicle.SetInput(horizontal, vertical);
        
        // 하차 입력 (E키)
        if (Input.GetKeyDown(KeyCode.E))
        {
            DismountVehicle();
        }
        
        // 트랙터 전용 - V키로 농사 작업
        if (currentVehicle is Tractor tractor && Input.GetKeyDown(KeyCode.V))
        {
            tractor.HandleFarming();
        }
    }
    
    private void OnModeChanged(EPlayerMode newMode)
    {
        if (newMode == EPlayerMode.Vehicle)
        {
            // Vehicle 모드로 전환 시
            currentVehicle = VehicleManager.Instance.GetPlayerVehicle(_owner);
            if (currentVehicle != null)
            {
                Debug.Log($"{currentVehicle.VehicleType} 조작 모드 활성화");
                ShowVehicleControls();
            }
        }
        else
        {
            // Vehicle 모드에서 벗어날 때
            if (currentVehicle != null)
            {
                Debug.Log("차량 조작 모드 비활성화");
                HideVehicleControls();
                currentVehicle = null;
            }
        }
    }
    
    public void MountVehicle(Vehicle vehicle)
    {
        if (vehicle == null) return;
        
        currentVehicle = vehicle;

        _owner.CharacterController.enabled = false;
        _owner.InputController.SetPlayerMoveInputLock(true);
        _owner.ModeController.SwitchMode(EPlayerMode.Vehicle);
        _owner.Animator.SetTrigger("Mount");
        vehicle.MountPlayer(_owner);
    }
    
    public void DismountVehicle()
    {
        if (currentVehicle != null)
        {
            currentVehicle.DismountPlayer();
            currentVehicle = null;
            _owner.CharacterController.enabled = true;
            _owner.InputController.SetPlayerMoveInputLock(false);
            _owner.Animator.SetTrigger("Dismount");
            _owner.ModeController.SwitchMode(EPlayerMode.BlockEdit);

        }
    }
    
    private void ShowVehicleControls()
    {
        if (currentVehicle == null) return;
        
        string controls = "차량 조작법:\\n";
        controls += "WASD: 이동\\n";
        controls += "E: 하차\\n";
        
        if (currentVehicle is Tractor)
        {
            controls += "F: 경작 모드 토글";
        }
        
        // 향후 UI로 표시할 수 있음
        Debug.Log(controls);
    }
    
    private void HideVehicleControls()
    {
        // 향후 UI 숨기기 로직
    }
    
    public Vehicle CurrentVehicle => currentVehicle;
    public bool IsInVehicle => currentVehicle != null && currentVehicle.IsOccupied;
    
    private void OnDestroy()
    {
        if (_owner != null && _owner.ModeController != null)
        {
            _owner.ModeController.OnModeChanged.RemoveListener(OnModeChanged);
        }
    }
}