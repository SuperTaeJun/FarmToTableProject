using Firebase.Firestore;
using System;
using UnityEngine;

public enum ECropType
{
    Carrot,
    Beet,
    Bean,
    Broccoli,
    Chilli,
    Cucumber,
    Pumkin,
}
public enum ECropGrowthStage
{
    Seed,          
    Vegetative,    
    Mature,   
    Harvest        
}
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
    }

    public Crop(ECropType type, string chunkId, Vector3 position, ECropGrowthStage stage, DateTime plantedTime, DateTime lastWateredTime, bool isWatered, float growthProgress)
    {
        Type = type;
        ChunkId = chunkId;
        Position = position;
        GrowthStage = stage;
        PlantedTime = plantedTime;
        LastWateredTime = lastWateredTime;
        IsWatered = isWatered;
        GrowthProgress = growthProgress;
    }

    public void Water()
    {
        IsWatered = true;
        LastWateredTime = DateTime.Now;
    }

    public void UpdateGrowth(float deltaProgress)
    {
        GrowthProgress = Mathf.Clamp01(GrowthProgress + deltaProgress);
        UpdateGrowthStage();
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
        return !IsWatered || (DateTime.Now - LastWateredTime).TotalHours > 24;
    }
}
[FirestoreData]
public class CropDto
{
    [FirestoreProperty]
    public int Type { get; set; }

    [FirestoreProperty]
    public int GrowthStage { get; set; }

    [FirestoreProperty]
    public string ChunkId { get; set; }

    [FirestoreProperty]
    public float PositionX { get; set; }

    [FirestoreProperty]
    public float PositionY { get; set; }

    [FirestoreProperty]
    public float PositionZ { get; set; }

    [FirestoreProperty]
    public Timestamp PlantedTime { get; set; }

    [FirestoreProperty]
    public Timestamp LastWateredTime { get; set; }

    [FirestoreProperty]
    public bool IsWatered { get; set; }

    [FirestoreProperty]
    public float GrowthProgress { get; set; }

    public CropDto() { }

    public CropDto(Crop crop)
    {
        Type = (int)crop.Type;
        GrowthStage = (int)crop.GrowthStage;
        ChunkId = crop.ChunkId;
        PositionX = crop.Position.x;
        PositionY = crop.Position.y;
        PositionZ = crop.Position.z;
        PlantedTime = Timestamp.FromDateTime(crop.PlantedTime.ToUniversalTime());
        LastWateredTime = crop.LastWateredTime == DateTime.MinValue ?
            Timestamp.FromDateTime(DateTime.MinValue.ToUniversalTime()) :
            Timestamp.FromDateTime(crop.LastWateredTime.ToUniversalTime());
        IsWatered = crop.IsWatered;
        GrowthProgress = crop.GrowthProgress;
    }

    public Crop ToCrop()
    {
        Vector3 position = new Vector3(PositionX, PositionY, PositionZ);
        DateTime plantedTime = PlantedTime.ToDateTime().ToLocalTime();
        DateTime lastWateredTime = LastWateredTime.ToDateTime() == DateTime.MinValue.ToUniversalTime() ?
            DateTime.MinValue :
            LastWateredTime.ToDateTime().ToLocalTime();

        return new Crop(
            (ECropType)Type,
            ChunkId,
            position,
            (ECropGrowthStage)GrowthStage,
            plantedTime,
            lastWateredTime,
            IsWatered,
            GrowthProgress
        );
    }
}