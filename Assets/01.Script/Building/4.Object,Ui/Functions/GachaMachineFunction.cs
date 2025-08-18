using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
[System.Serializable]
public class VehicleGachaData
{
    public EVehicleType vehicleType;
    public float dropRate;
}

public class GachaMachineFunction : IBuildingFunction
{
    private BuildingObject _buildingObject;
    private List<VehicleGachaData> vehiclePool = new List<VehicleGachaData>();
    private int _gemAmount = 1;
    
    public GachaMachineFunction(BuildingObject buildingObject)
    {
        _buildingObject = buildingObject;
        InitializeVehiclePool();
    }

    private void InitializeVehiclePool()
    {
        vehiclePool.Add(new VehicleGachaData { vehicleType = EVehicleType.Bicycle, dropRate = 30f });
        vehiclePool.Add(new VehicleGachaData { vehicleType = EVehicleType.Bike1, dropRate = 25f });
        vehiclePool.Add(new VehicleGachaData { vehicleType = EVehicleType.Bike2, dropRate = 20f });
        vehiclePool.Add(new VehicleGachaData { vehicleType = EVehicleType.sedan, dropRate = 10f });
        vehiclePool.Add(new VehicleGachaData { vehicleType = EVehicleType.Cart, dropRate = 8f });
        vehiclePool.Add(new VehicleGachaData { vehicleType = EVehicleType.Bike3, dropRate = 5f });
        vehiclePool.Add(new VehicleGachaData { vehicleType = EVehicleType.RacingCar, dropRate = 1.5f });
        vehiclePool.Add(new VehicleGachaData { vehicleType = EVehicleType.Tractor, dropRate = 0.5f });
    }

    public void Execute()
    {
        if (CanPerformGacha())
        {
            PerformGacha();
        }
        else
        {
            Debug.Log("가챠를 수행할 수 없습니다. 충분한 재화가 필요합니다.");
        }
    }
    

    private bool CanPerformGacha()
    {
        return CurrencyManager.Instance.CanAfford(ECurrencyType.Gem, _gemAmount);
    }

    private async void PerformGacha()
    {
        bool spent = await CurrencyManager.Instance.TrySpendCurrency(ECurrencyType.Gem, _gemAmount);
        
        if (!spent)
        {
            Debug.Log("재화 소모에 실패했습니다.");
            return;
        }
        
        EVehicleType selectedVehicle = SelectRandomVehicle();
        
        if (selectedVehicle != EVehicleType.None)
        {
            UnlockVehicleForPlayer(selectedVehicle);
        }
    }

    private EVehicleType SelectRandomVehicle()
    {
        float totalRate = 0f;
        foreach (var vehicle in vehiclePool)
        {
            totalRate += vehicle.dropRate;
        }

        float randomValue = Random.Range(0f, totalRate);
        float currentRate = 0f;

        foreach (var vehicle in vehiclePool)
        {
            currentRate += vehicle.dropRate;
            if (randomValue <= currentRate)
            {
                return vehicle.vehicleType;
            }
        }

        return vehiclePool[0].vehicleType;
    }

    private void UnlockVehicleForPlayer(EVehicleType vehicleType)
    {
        if (VehicleManager.Instance.IsVehicleUnlocked(vehicleType))
        {
            Debug.Log($"{GetVehicleName(vehicleType)}은(는) 이미 해금된 차량입니다! 추가 보상을 지급합니다.");
        }
        else
        {
            VehicleManager.Instance.UnlockVehicle(vehicleType);
            Debug.Log($"축하합니다! {GetVehicleName(vehicleType)}을(를) 획득했습니다!");
        }
    }

    private string GetVehicleName(EVehicleType vehicleType)
    {
        switch (vehicleType)
        {
            case EVehicleType.sedan: return "세단";
            case EVehicleType.Tractor: return "트랙터";
            case EVehicleType.Bike1: return "바이크1";
            case EVehicleType.Bike2: return "바이크2"; 
            case EVehicleType.Bike3: return "바이크3";
            case EVehicleType.Bicycle: return "자전거";
            case EVehicleType.RacingCar: return "레이싱카";
            case EVehicleType.Cart: return "카트";
            default: return vehicleType.ToString();
        }
    }

    public void Update()
    {
        
    }
}
