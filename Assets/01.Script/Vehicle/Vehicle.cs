using UnityEngine;

public abstract class Vehicle : MonoBehaviour
{
    [Header("Vehicle Settings")]
    public EVehicleType vehicleType;
    public float maxSpeed = 3f;
    public float acceleration = 1.5f;
    public float brakeForce = 10f;
    public float turnSpeed = 100f;
    
    [Header("Player Mount")]
    public Transform playerMountPoint;
    
    protected bool isOccupied = false;
    protected Player currentDriver;
    protected Rigidbody vehicleRigidbody;
    
    // 입력 값들
    protected float horizontalInput;
    protected float verticalInput;
    
    protected virtual void Awake()
    {
        vehicleRigidbody = GetComponent<Rigidbody>();
        if (vehicleRigidbody == null)
        {
            vehicleRigidbody = gameObject.AddComponent<Rigidbody>();
        }
        
        // 플레이어 마운트 포인트 설정
        if (playerMountPoint == null)
        {
            GameObject mountPoint = new GameObject("PlayerMountPoint");
            mountPoint.transform.SetParent(transform);
            mountPoint.transform.localPosition = Vector3.up * 2f; // 차량 위쪽으로
            playerMountPoint = mountPoint.transform;
        }
    }
    
    protected virtual void Update()
    {
        if (isOccupied)
        {
            HandleMovement();
            HandleSteering();
        }
    }
    
    public virtual void MountPlayer(Player player)
    {
        if (isOccupied) return;
        
        isOccupied = true;
        currentDriver = player;
        
        // CharacterController 비활성화 (위치 이동을 위해)
        CharacterController playerController = player.GetComponent<CharacterController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // 플레이어를 차량에 부모로 설정
        player.transform.SetParent(playerMountPoint);
        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.identity;
        
        // CharacterController 다시 활성화
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        // 플레이어 모드를 Vehicle로 변경
        player.ModeController.SwitchMode(EPlayerMode.Vehicle);
        player.InputController.SetPlayerMoveInputLock(true);

        OnPlayerMounted(player);
    }
    
    public virtual void DismountPlayer()
    {
        if (!isOccupied || currentDriver == null) return;
        
        // CharacterController 비활성화 (위치 이동을 위해)
        CharacterController playerController = currentDriver.GetComponent<CharacterController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // 플레이어를 차량에서 분리
        currentDriver.transform.SetParent(null);
        
        // 플레이어를 차량 옆으로 이동
        Vector3 dismountPosition = transform.position + transform.right * 3f * ChunkGenerator.Instance.blockOffset.x;
        currentDriver.transform.position = dismountPosition;
        
        // CharacterController 다시 활성화
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        // 플레이어 모드를 기본 모드로 변경
        currentDriver.ModeController.SwitchMode(EPlayerMode.BlockEdit);
        
        OnPlayerDismounted(currentDriver);
        
        currentDriver = null;
        isOccupied = false;
        
        // 차량 오브젝트 제거
        VehicleManager.Instance.DestroyVehicle(this);
    }
    
    public virtual void SetInput(float horizontal, float vertical)
    {
        horizontalInput = horizontal;
        verticalInput = vertical;
    }
    
    protected abstract void HandleMovement();
    protected abstract void HandleSteering();
    protected virtual void OnPlayerMounted(Player player) { }
    protected virtual void OnPlayerDismounted(Player player) { }
    
    public bool IsOccupied => isOccupied;
    public Player CurrentDriver => currentDriver;
    public EVehicleType VehicleType => vehicleType;
}