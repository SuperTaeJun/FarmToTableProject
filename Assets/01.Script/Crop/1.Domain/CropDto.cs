using Firebase.Firestore;
using System;
using UnityEngine;

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
    public int PlantedDay { get; set; }

    [FirestoreProperty]
    public int PlantedHour { get; set; }

    [FirestoreProperty]
    public int LastWateredDay { get; set; }

    [FirestoreProperty]
    public int LastWateredHour { get; set; }

    [FirestoreProperty]
    public bool IsWatered { get; set; }

    [FirestoreProperty]
    public float GrowthProgress { get; set; }

    [FirestoreProperty]
    public bool IsWateredForVegetative { get; set; }

    [FirestoreProperty]
    public bool IsWateredForMature { get; set; }

    public CropDto() { }

    public CropDto(Crop crop)
    {
        Type = (int)crop.Type;
        GrowthStage = (int)crop.GrowthStage;
        ChunkId = crop.ChunkId;
        PositionX = crop.Position.x;
        PositionY = crop.Position.y;
        PositionZ = crop.Position.z;
        PlantedDay = crop.PlantedDay;
        PlantedHour = crop.PlantedHour;
        LastWateredDay = crop.LastWateredDay;
        LastWateredHour = crop.LastWateredHour;
        IsWatered = crop.IsWatered;
        GrowthProgress = crop.GrowthProgress;
        IsWateredForVegetative = crop.IsWateredForVegetative;
        IsWateredForMature = crop.IsWateredForMature;
    }

    public Crop ToCrop()
    {
        Vector3 position = new Vector3(PositionX, PositionY, PositionZ);

        return new Crop(
            (ECropType)Type,
            ChunkId,
            position,
            (ECropGrowthStage)GrowthStage,
            PlantedDay,
            PlantedHour,
            LastWateredDay,
            LastWateredHour,
            IsWatered,
            GrowthProgress,
            IsWateredForVegetative,
            IsWateredForMature
        );
    }
}
