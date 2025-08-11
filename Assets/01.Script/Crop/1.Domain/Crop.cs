using System;
using UnityEngine;

public class Crop
{
    public ECropType Type { get; private set; }
    public ECropGrowthStage GrowthStage { get; private set; }
    public string ChunkId { get; private set; }
    public Vector3 Position { get; private set; }
    public int PlantedDay { get; private set; }
    public int PlantedHour { get; private set; }
    public int LastWateredDay { get; private set; }
    public int LastWateredHour { get; private set; }
    public bool IsWatered { get; private set; }
    public float GrowthProgress { get; private set; }
    public bool IsWateredForVegetative { get; private set; }
    public bool IsWateredForMature { get; private set; }

    public Crop(ECropType type, string chunkId, Vector3 position, int plantedDay, int plantedHour)
    {
        Type = type;
        ChunkId = chunkId;
        Position = position;
        GrowthStage = ECropGrowthStage.Seed;

        PlantedDay = plantedDay;
        PlantedHour = plantedHour;

        LastWateredDay = -1;
        LastWateredHour = -1;
        IsWatered = false;
        GrowthProgress = 0f;
        IsWateredForVegetative = false;
        IsWateredForMature = false;
    }
    // 로딩용 생성자
    public Crop(ECropType type, string chunkId, Vector3 position, ECropGrowthStage stage, int plantedDay, int plantedHour, int lastWateredDay, int lastWateredHour, bool isWatered, float growthProgress, bool isWateredForVegetative = false, bool isWateredForMature = false)
    {
        Type = type;
        ChunkId = chunkId;
        Position = position;
        GrowthStage = stage;
        PlantedDay = plantedDay;
        PlantedHour = plantedHour;
        LastWateredDay = lastWateredDay;
        LastWateredHour = lastWateredHour;
        IsWatered = isWatered;
        GrowthProgress = growthProgress;
        IsWateredForVegetative = isWateredForVegetative;
        IsWateredForMature = isWateredForMature;
    }

    public void Water(int plantedDay, int plantedHour)
    {
        IsWatered = true;
        LastWateredDay = plantedDay;
        LastWateredHour = plantedHour;
    }

    public void WaterCurrentStage(int plantedDay, int plantedHour)
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
        LastWateredDay = plantedDay;
        LastWateredHour = plantedHour;
    }

    public bool IsWateredForCurrentStage()
    {
        switch (GrowthStage)
        {
            case ECropGrowthStage.Seed:
            case ECropGrowthStage.Harvest:
                return true;
            case ECropGrowthStage.Vegetative:
                return IsWateredForVegetative;
            case ECropGrowthStage.Mature:
                return IsWateredForMature;
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
                return false;
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
        GrowthProgress = Mathf.Clamp01(GrowthProgress + deltaProgress);
        UpdateGrowthStage();
    }

    public void UpdateGrowthWithCropData(float deltaProgress, SO_Crop cropData)
    {
        GrowthProgress = Mathf.Clamp01(GrowthProgress + deltaProgress);
        UpdateGrowthStageWithCropData(cropData);
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

    private void UpdateGrowthStageWithCropData(SO_Crop cropData)
    {
        if (GrowthProgress >= cropData.HarvestStageRatio)
            GrowthStage = ECropGrowthStage.Harvest;
        else if (GrowthProgress >= cropData.MatureStageRatio)
            GrowthStage = ECropGrowthStage.Mature;
        else if (GrowthProgress >= cropData.VegetativeStageRatio)
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
