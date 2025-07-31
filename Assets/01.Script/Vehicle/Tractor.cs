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

        // 트럭 기본 설정
        maxSpeed = 15f;
        acceleration = 8f;
        deceleration = 12f;
        turnSpeed = 80f;
    }
    
    
    public void HandlePlowing()
    {
        WorldManager.Instance.SetBlock(transform.position, EBlockType.Farmland,4);
        ObjectPoolManager.Instance.Get(PoolType.Smoke);

    }
    
    protected override void OnPlayerMounted(Player player)
    {
        Debug.Log("플레이어가 트랙터에 탑승했습니다. [F키]로 경작 모드를 토글할 수 있습니다.");
        ObjectPoolManager.Instance.Get(PoolType.Smoke,transform.position);

    }

    protected override void OnPlayerDismounted(Player player)
    {
        Debug.Log("플레이어가 트랙터에서 하차했습니다.");
        ObjectPoolManager.Instance.Get(PoolType.Smoke,transform.position);

        isPlowing = false;
    }
    
    public bool IsPlowing => isPlowing;
}