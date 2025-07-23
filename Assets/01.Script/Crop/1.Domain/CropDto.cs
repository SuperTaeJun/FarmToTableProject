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
    public Timestamp PlantedTime { get; set; }

    [FirestoreProperty]
    public Timestamp LastWateredTime { get; set; }

    [FirestoreProperty]
    public bool IsWatered { get; set; }

    [FirestoreProperty]
    public float GrowthProgress { get; set; }

    // 단계별 물 주기 상태 추가
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
        PlantedTime = Timestamp.FromDateTime(crop.PlantedTime.ToUniversalTime());
        LastWateredTime = crop.LastWateredTime == DateTime.MinValue ?
            Timestamp.FromDateTime(DateTime.MinValue.ToUniversalTime()) :
            Timestamp.FromDateTime(crop.LastWateredTime.ToUniversalTime());
        IsWatered = crop.IsWatered;
        GrowthProgress = crop.GrowthProgress;
        IsWateredForVegetative = crop.IsWateredForVegetative;
        IsWateredForMature = crop.IsWateredForMature;
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
            GrowthProgress,
            IsWateredForVegetative,
            IsWateredForMature
        );
    }
}
