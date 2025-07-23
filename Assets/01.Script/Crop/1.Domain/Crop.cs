using System;
using UnityEngine;

public class Crop
{
    public ECropType Type { get; private set; }
    public ECropGrowthStage GrowthStage { get; private set; }
    public string ChunkId { get; private set; }
    public Vector3 Position { get; private set; }
    public DateTime PlantedTime { get; private set; }
    public DateTime LastWateredTime { get; private set; }
    public bool IsWatered { get; private set; }
    public float GrowthProgress { get; private set; }

    // 각 단계별 물 주기 상태 추가
    public bool IsWateredForVegetative { get; private set; }
    public bool IsWateredForMature { get; private set; }

    public Crop(ECropType type, string chunkId, Vector3 position)
    {
        Type = type;
        ChunkId = chunkId;
        Position = position;
        GrowthStage = ECropGrowthStage.Seed;
        PlantedTime = DateTime.Now;
        LastWateredTime = DateTime.MinValue;
        IsWatered = false;
        GrowthProgress = 0f;
        IsWateredForVegetative = false;
        IsWateredForMature = false;
    }

    public Crop(ECropType type, string chunkId, Vector3 position, ECropGrowthStage stage, DateTime plantedTime, DateTime lastWateredTime, bool isWatered, float growthProgress, bool isWateredForVegetative = false, bool isWateredForMature = false)
    {
        Type = type;
        ChunkId = chunkId;
        Position = position;
        GrowthStage = stage;
        PlantedTime = plantedTime;
        LastWateredTime = lastWateredTime;
        IsWatered = isWatered;
        GrowthProgress = growthProgress;
        IsWateredForVegetative = isWateredForVegetative;
        IsWateredForMature = isWateredForMature;
    }
    public void Water()
    {
        IsWatered = true;
        LastWateredTime = DateTime.Now;
    }

    // 단계별 물 주기 시스템
    public void WaterCurrentStage()
    {
        switch (GrowthStage)
        {
            case ECropGrowthStage.Vegetative:
                IsWateredForVegetative = true;
                break;
            case ECropGrowthStage.Mature:
                IsWateredForMature = true;
                break;
        }

        IsWatered = true;
        LastWateredTime = DateTime.Now;
    }
    public bool IsWateredForCurrentStage()
    {
        switch (GrowthStage)
        {
            case ECropGrowthStage.Seed:
                return true; // 씨앗은 물 필요 없음
            case ECropGrowthStage.Vegetative:
                return IsWateredForVegetative;
            case ECropGrowthStage.Mature:
                return IsWateredForMature;
            case ECropGrowthStage.Harvest:
                return true; // 수확은 물 필요 없음
            default:
                return false;
        }
    }

    public bool CanWaterCurrentStage()
    {
        switch (GrowthStage)
        {
            case ECropGrowthStage.Seed:
            case ECropGrowthStage.Harvest:
                return false; // 씨앗과 수확 단계는 물을 줄 수 없음
            case ECropGrowthStage.Vegetative:
                return !IsWateredForVegetative;
            case ECropGrowthStage.Mature:
                return !IsWateredForMature;
            default:
                return false;
        }
    }

    public void UpdateGrowth(float deltaProgress)
    {
        var previousStage = GrowthStage;
        GrowthProgress = Mathf.Clamp01(GrowthProgress + deltaProgress);
        UpdateGrowthStage();

        // 단계가 변경되었을 때 물 상태 초기화는 하지 않음 (이미 준 물은 유지)
    }

    private void UpdateGrowthStage()
    {
        if (GrowthProgress >= 1.0f)
            GrowthStage = ECropGrowthStage.Harvest;
        else if (GrowthProgress >= 0.5f)
            GrowthStage = ECropGrowthStage.Mature;
        else if (GrowthProgress >= 0.2f)
            GrowthStage = ECropGrowthStage.Vegetative;
        else
            GrowthStage = ECropGrowthStage.Seed;
    }

    public bool CanHarvest()
    {
        return GrowthStage == ECropGrowthStage.Harvest;
    }

    public bool NeedsWater()
    {
        return !IsWateredForCurrentStage();
    }
}
