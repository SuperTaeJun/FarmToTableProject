using UnityEngine;

public class Truck : Vehicle
{

    protected override void Awake()
    {
        base.Awake();
        vehicleType = EVehicleType.Truck;
        
        // 트럭 기본 설정
        maxSpeed = 15f;
        acceleration = 8f;
        deceleration = 12f;
        turnSpeed = 80f;
    }
    
    
}