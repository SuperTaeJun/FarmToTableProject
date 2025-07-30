using UnityEngine;

public class Truck : Vehicle
{
    [Header("Truck Specific")]
    public float cargoCapacity = 1000f;
    public Transform cargoArea;
    
    private float currentCargoWeight = 0f;
    
    protected override void Awake()
    {
        base.Awake();
        vehicleType = EVehicleType.Truck;
        
        // 트럭 기본 설정
        maxSpeed = 15f;
        acceleration = 8f;
        brakeForce = 12f;
        turnSpeed = 80f;
    }
    
    protected override void HandleMovement()
    {
        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            float motorForce = verticalInput * acceleration * 100f;
            vehicleRigidbody.AddForce(transform.forward * motorForce);
            
            // 최대 속도 제한
            if (vehicleRigidbody.linearVelocity.magnitude > maxSpeed)
            {
                vehicleRigidbody.linearVelocity = vehicleRigidbody.linearVelocity.normalized * maxSpeed;
            }
        }
        else
        {
            // 브레이크/관성 감소
            vehicleRigidbody.linearVelocity *= 0.95f;
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
    
    protected override void OnPlayerMounted(Player player)
    {
        Debug.Log($"플레이어가 트럭에 탑승했습니다. 적재량: {currentCargoWeight}/{cargoCapacity}");
    }
    
    protected override void OnPlayerDismounted(Player player)
    {
        Debug.Log("플레이어가 트럭에서 하차했습니다.");
    }
    
    public bool CanLoadCargo(float weight)
    {
        return currentCargoWeight + weight <= cargoCapacity;
    }
    
    public void LoadCargo(float weight)
    {
        if (CanLoadCargo(weight))
        {
            currentCargoWeight += weight;
            Debug.Log($"화물 적재: {weight}kg, 총 적재량: {currentCargoWeight}/{cargoCapacity}");
        }
    }
    
    public void UnloadCargo(float weight)
    {
        currentCargoWeight = Mathf.Max(0, currentCargoWeight - weight);
        Debug.Log($"화물 하역: {weight}kg, 총 적재량: {currentCargoWeight}/{cargoCapacity}");
    }
    
    public float CurrentCargoWeight => currentCargoWeight;
    public float CargoCapacity => cargoCapacity;
}