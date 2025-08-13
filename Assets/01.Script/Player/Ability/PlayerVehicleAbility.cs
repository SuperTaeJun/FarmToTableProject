using UnityEngine;

public class PlayerVehicleAbility : PlayerAbility
{
    private Vehicle currentVehicle;

    public bool IsMounted;
    private void Start()
    {
        // Vehicle 모드에서만 활성화되도록 모드 변경 이벤트 구독
        _owner.GetAbility<PlayerModeAbility>()?.OnModeChanged.AddListener(OnModeChanged);
    }
    
    private void Update()
    {
        if (_owner.GetAbility<PlayerModeAbility>()?.CurrentMode != EPlayerMode.Vehicle)
            return;
            
        if (currentVehicle == null)
        {
            // 현재 차량 찾기
            currentVehicle = VehicleManager.Instance.GetPlayerVehicle(_owner);
            if (currentVehicle == null)
            {
                // 차량이 없으면 모드를 기본으로 변경
                _owner.GetAbility<PlayerModeAbility>()?.SwitchMode(EPlayerMode.BlockEdit);
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
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            DismountVehicle();
        }

        if (currentVehicle is Tractor tractor && Input.GetKeyDown(KeyCode.V))
        {
            tractor.HandleFarming();
        }
    }
    
    private void OnModeChanged(EPlayerMode newMode)
    {
        if (newMode == EPlayerMode.Vehicle)
        {
            currentVehicle = VehicleManager.Instance.GetPlayerVehicle(_owner);
            if (currentVehicle != null)
            {
                IsMounted = true;
            }
        }
        else
        {
            if (currentVehicle != null)
            {
                currentVehicle = null;
                IsMounted = false;
            }
        }
    }
    
    public void MountVehicle(Vehicle vehicle)
    {
        if (vehicle == null) return;
        
        currentVehicle = vehicle;

        IsMounted = true;

        _owner.CharacterController.enabled = false;
        _owner.GetAbility<PlayerInputAbility>().SetPlayerMoveInputLock(true);
        _owner.GetAbility<PlayerModeAbility>()?.SwitchMode(EPlayerMode.Vehicle);
        _owner.Animator.SetTrigger("Mount");
        vehicle.MountPlayer(_owner);

        SoundManager.Instance.PlaySFX(SFXType.Mount);
        
    }
    
    public void DismountVehicle()
    {
        if (currentVehicle != null)
        {
            IsMounted = false;

            currentVehicle.DismountPlayer();
            currentVehicle = null;
            _owner.CharacterController.enabled = true;
            _owner.GetAbility<PlayerInputAbility>().SetPlayerMoveInputLock(false);
            _owner.Animator.SetTrigger("Dismount");
            _owner.GetAbility<PlayerModeAbility>()?.SwitchMode(EPlayerMode.BlockEdit);

            SoundManager.Instance.PlaySFX(SFXType.Dismount);
        }
    }
    
    private void OnDestroy()
    {
        if (_owner != null && _owner.GetAbility<PlayerModeAbility>() != null)
        {
            _owner.GetAbility<PlayerModeAbility>()?.OnModeChanged.RemoveListener(OnModeChanged);
        }
    }
}