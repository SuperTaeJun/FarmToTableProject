using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
public class GachaScene : MonoBehaviour
{
    public static GachaScene Instance { get; private set; }

    [SerializeField] private GameObject _rewardUI;

    private List<VehicleGachaData> vehiclePool = new List<VehicleGachaData>();

    public event Action OnGachaPerformed;
    public event Action <Sprite,string> OnVehicleRewarded;
    private void Awake()
    {
        Instance = this;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void Start()
    {
        InitializeVehiclePool();
    }

    public void SetActiveRewardUI(bool isActive)
    {
        _rewardUI.SetActive(isActive);
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
    public void OnGachaButtonClicked()
    {
        OnGachaPerformed?.Invoke();
    }
    public async void PerformGacha()
    {
        bool spent = await CurrencyManager.Instance.TrySpendCurrency(ECurrencyType.Gem, 1);

        if (!spent)
        {
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

        }
        else
        {
            VehicleManager.Instance.UnlockVehicle(vehicleType);

        }

        _rewardUI.SetActive(true);
        var CurrentVehicle = VehicleManager.Instance.Vehicles.Find(v => v.Type == vehicleType);
        OnVehicleRewarded.Invoke(CurrentVehicle.Sprite, GetVehicleName(vehicleType));
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


    public class VehicleGachaData
    {
        public EVehicleType vehicleType;
        public float dropRate;
    }



}
