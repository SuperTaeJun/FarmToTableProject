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

    // �� �ܰ躰 �� �ֱ� ���� �߰�
    public bool IsWateredForVegetative { get; private set; }
    public bool IsWateredForMature { get; private set; }

    public Crop(ECropType type, string chunkId, Vector3 position)
    {
        Type = type;
        ChunkId = chunkId;
        Position = position;
        GrowthStage = ECropGrowthStage.Seed;
        
        // 현재 게임 시간으로 심은 시간 설정
        if (GameTimeManager.Instance != null)
        {
            PlantedDay = GameTimeManager.Instance.CurrentDay;
            PlantedHour = GameTimeManager.Instance.CurrentHour;
        }
        else
        {
            PlantedDay = 0;
            PlantedHour = 0;
        }
        
        LastWateredDay = -1; // 한 번도 물 안 준 상태
        LastWateredHour = -1;
        IsWatered = false;
        GrowthProgress = 0f;
        IsWateredForVegetative = false;
        IsWateredForMature = false;
    }

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
    public void Water()
    {
        IsWatered = true;
        if (GameTimeManager.Instance != null)
        {
            LastWateredDay = GameTimeManager.Instance.CurrentDay;
            LastWateredHour = GameTimeManager.Instance.CurrentHour;
        }
    }

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
        if (GameTimeManager.Instance != null)
        {
            LastWateredDay = GameTimeManager.Instance.CurrentDay;
            LastWateredHour = GameTimeManager.Instance.CurrentHour;
        }
    }
    public bool IsWateredForCurrentStage()
    {
        switch (GrowthStage)
        {
            case ECropGrowthStage.Seed:
                return true; // ������ �� �ʿ� ����
            case ECropGrowthStage.Vegetative:
                return IsWateredForVegetative;
            case ECropGrowthStage.Mature:
                return IsWateredForMature;
            case ECropGrowthStage.Harvest:
                return true; // ��Ȯ�� �� �ʿ� ����
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
                return false; // ���Ѱ� ��Ȯ �ܰ�� ���� �� �� ����
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

        // �ܰ谡 ����Ǿ��� �� �� ���� �ʱ�ȭ�� ���� ���� (�̹� �� ���� ����)
    }
    
    public void UpdateGrowthWithCropData(float deltaProgress, SO_Crop cropData)
    {
        var previousStage = GrowthStage;
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
