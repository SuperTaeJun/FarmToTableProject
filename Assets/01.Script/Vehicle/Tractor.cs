using UnityEngine;

public class Tractor : Vehicle
{
    [Header("Tractor Specific")]
    public float plowingSpeed = 2f;
    public float plowingWidth = 3f;
    public LayerMask farmlandLayer;
    
    private bool isPlowing = false;
    
    protected override void Awake()
    {
        base.Awake();
        vehicleType = EVehicleType.Tractor;
        
        // 트랙터 기본 설정 (트럭보다 느리고 강력함)
        maxSpeed = 8f;
        acceleration = 12f;
        brakeForce = 15f;
        turnSpeed = 60f;
    }
    
    protected override void HandleMovement()
    {
        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            float motorForce = verticalInput * acceleration * 100f;
            
            // 경작 중일 때는 속도 감소
            if (isPlowing)
            {
                motorForce *= 0.5f;
                HandlePlowing();
            }
            
            vehicleRigidbody.AddForce(transform.forward * motorForce);
            
            // 최대 속도 제한
            float currentMaxSpeed = isPlowing ? plowingSpeed : maxSpeed;
            if (vehicleRigidbody.linearVelocity.magnitude > currentMaxSpeed)
            {
                vehicleRigidbody.linearVelocity = vehicleRigidbody.linearVelocity.normalized * currentMaxSpeed;
            }
        }
        else
        {
            // 브레이크/관성 감소
            vehicleRigidbody.linearVelocity *= 0.9f;
        }
    }
    
    protected override void HandleSteering()
    {
        if (Mathf.Abs(horizontalInput) > 0.1f && Mathf.Abs(verticalInput) > 0.1f)
        {
            float steerAngle = horizontalInput * turnSpeed * Time.deltaTime;
            transform.Rotate(0, steerAngle, 0);
        }
    }
    
    private void HandlePlowing()
    {
        // 경작 범위 내의 땅을 farmland로 변경
        Vector3 plowCenter = transform.position;
        Vector3 plowDirection = transform.forward;
        
        // 경작 범위 계산 (blockOffset 고려)
        float blockSizeX = ChunkGenerator.Instance.blockOffset.x;
        float blockSizeZ = ChunkGenerator.Instance.blockOffset.z;
        
        for (float x = -plowingWidth * 0.5f; x <= plowingWidth * 0.5f; x += blockSizeX)
        {
            Vector3 checkPos = plowCenter + transform.right * x;
            
            // 해당 위치의 블록을 farmland로 변경
            if (WorldManager.Instance != null)
            {
                WorldManager.Instance.SetBlock(checkPos, EBlockType.Farmland);
            }
        }
    }
    
    protected override void OnPlayerMounted(Player player)
    {
        Debug.Log("플레이어가 트랙터에 탑승했습니다. [F키]로 경작 모드를 토글할 수 있습니다.");
    }
    
    protected override void OnPlayerDismounted(Player player)
    {
        Debug.Log("플레이어가 트랙터에서 하차했습니다.");
        isPlowing = false;
    }
    
    public void TogglePlowing()
    {
        isPlowing = !isPlowing;
        Debug.Log(isPlowing ? "경작 모드 활성화" : "경작 모드 비활성화");
    }
    
    public bool IsPlowing => isPlowing;
}