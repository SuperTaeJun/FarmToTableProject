using UnityEngine;

public class Tractor : Vehicle
{
    [Header("Tractor Specific")]
    public float plowingSpeed = 2f;
    public float plowingWidth = 3f;
    public LayerMask farmlandLayer;
    
    protected override void Awake()
    {
        base.Awake();
        vehicleType = EVehicleType.Tractor;

    }
    
    
    public void HandlePlowing()
    {
        WorldManager.Instance.SetBlock(transform.position, EBlockType.Farmland,4);
        ObjectPoolManager.Instance.Get(PoolType.Smoke);

    }
    
    protected override void OnPlayerMounted(Player player)
    {

    }

    protected override void OnPlayerDismounted(Player player)
    {


    }
    
}