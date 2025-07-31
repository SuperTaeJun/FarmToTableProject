using System.Collections.Generic;
using UnityEngine;

public abstract class Vehicle : MonoBehaviour
{
    [Header("Vehicle Settings")]
    public EVehicleType vehicleType;
    public float acceleration = 5f;
    public float maxSpeed = 10f;
    public float deceleration = 5f;
    public float turnSpeed = 100f;
    public Vector3 moveVelocity = Vector3.zero;

    public float gravity = -9.81f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    public bool isGrounded;
    public float verticalVelocity = 0f;

    [Header("바퀴 세팅")]
    [SerializeField] private List<Transform> wheelMeshes; // 바퀴 오브젝트들 (Transform)
    [SerializeField] private float wheelRadius = 0.3f;
    [SerializeField] private List<Transform> steeringWheels; // 앞바퀴
    [SerializeField] private float maxSteerAngle = 30f;

    [Header("Player Mount")]
    public Transform playerMountPoint;
    
    protected bool isOccupied = false;
    protected Player currentDriver;
    protected CharacterController vehicleController;
    
    // 입력 값들
    protected float horizontalInput;
    protected float verticalInput;
    
    protected virtual void Awake()
    {
        vehicleController = GetComponent<CharacterController>();

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
            HandleGravity();

            UpdateWheels();
            UpdateSteering();
        }
    }
    
    public virtual void MountPlayer(Player player)
    {
        if (isOccupied) return;
        
        isOccupied = true;
        currentDriver = player;
        

        
        // 플레이어를 차량에 부모로 설정
        player.transform.SetParent(playerMountPoint);
        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.identity;
        
        
        OnPlayerMounted(player);
    }
    
    public virtual void DismountPlayer()
    {
        if (!isOccupied || currentDriver == null) return;
        

        // 플레이어를 차량에서 분리
        currentDriver.transform.SetParent(null);
        
        //// 플레이어를 차량 옆으로 이동
        //Vector3 dismountPosition = transform.position + transform.right * 3f * ChunkGenerator.Instance.blockOffset.x;
        //currentDriver.transform.position = dismountPosition;
        
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

    protected void HandleMovement()
    {
        // 기존 가속/감속 로직
        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            float targetSpeed = verticalInput * maxSpeed;
            moveVelocity = Vector3.MoveTowards(moveVelocity,
                transform.forward * targetSpeed,
                acceleration * Time.deltaTime);
        }
        else
        {
            moveVelocity = Vector3.MoveTowards(moveVelocity, Vector3.zero,
                deceleration * Time.deltaTime);
        }

        // 청크 경계 검사 및 이동 적용
        ApplyMovementWithChunkCheck();
    }
    protected void HandleSteering()
    {
        if (Mathf.Abs(horizontalInput) > 0.1f && Mathf.Abs(verticalInput) > 0.1f)
        {
            float steerAngle = horizontalInput * turnSpeed * Time.deltaTime;
            transform.Rotate(0, steerAngle, 0);
        }
    }

    protected void HandleGravity()
    {
        // 바닥 체크 (간단한 Raycast)
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // 바닥에 붙이는 정도로 소폭 유지
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // 위에서 계산한 중력 벡터를 포함한 전체 이동 적용
        Vector3 gravityMove = new Vector3(0f, verticalVelocity, 0f);
        vehicleController.Move(gravityMove * Time.deltaTime);
    }
    protected void UpdateWheels()
    {
        float speed = moveVelocity.magnitude; // 현재 속도
        float direction = Mathf.Sign(Vector3.Dot(moveVelocity, transform.forward)); // 전진 or 후진

        float rotationAngle = (speed / wheelRadius) * Mathf.Rad2Deg * Time.deltaTime * direction;

        foreach (var wheel in wheelMeshes)
        {
            // X축 기준 회전 (굴러가는 표현)
            wheel.Rotate(Vector3.right, rotationAngle, Space.Self);
        }
    }
    protected void UpdateSteering()
    {
        float steerAngle = horizontalInput * maxSteerAngle;
        foreach (var wheel in steeringWheels)
        {
            // 조향 방향 적용 (Y축 회전)
            Vector3 euler = wheel.localEulerAngles;
            euler.y = steerAngle;
            wheel.localEulerAngles = euler;
        }
    }


    protected virtual void OnPlayerMounted(Player player) 
    {
        ObjectPoolManager.Instance.Get(PoolType.Smoke, transform.position);

    }
    protected virtual void OnPlayerDismounted(Player player) 
    {
        ObjectPoolManager.Instance.Get(PoolType.Smoke, transform.position);
    }

    private void ApplyMovementWithChunkCheck()
    {
        Vector3 desiredMove = moveVelocity * Time.deltaTime;
        Vector3 targetPos = transform.position + desiredMove;

        // 마진 설정 (플레이어와 동일)
        float chunkMargin = 2f;
        float chunkSizeX = Chunk.ChunkSize * WorldManager.Instance.dynamicGenerator.blockOffset.x;
        float chunkSizeZ = Chunk.ChunkSize * WorldManager.Instance.dynamicGenerator.blockOffset.z;

        // 이동 방향이 있을 때만 청크 검사
        if (moveVelocity.magnitude > 0.1f)
        {
            Vector3 moveDirection = moveVelocity.normalized;
            Vector3 marginPos = targetPos + moveDirection * chunkMargin;

            int targetChunkX = Mathf.FloorToInt(marginPos.x / chunkSizeX);
            int targetChunkZ = Mathf.FloorToInt(marginPos.z / chunkSizeZ);
            var targetChunkPos = new ChunkPosition(targetChunkX, 0, targetChunkZ);

            if (WorldManager.Instance.HasChunk(targetChunkPos))
            {
                // 이동 허용 - 수평 이동 적용
                Vector3 horizontalMove = new Vector3(desiredMove.x, 0, desiredMove.z);
                vehicleController.Move(horizontalMove);
            }
            else
            {
                // 이동 차단 - 수평 이동 없이 속도 초기화
                moveVelocity = Vector3.zero;
            }
        }
    }

    public bool IsOccupied => isOccupied;
    public Player CurrentDriver => currentDriver;
    public EVehicleType VehicleType => vehicleType;
}