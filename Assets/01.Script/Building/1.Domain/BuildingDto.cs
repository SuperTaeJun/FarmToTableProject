using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class BuildingDto
{
    [FirestoreProperty]
    public string Type { get; set; }

    [FirestoreProperty]
    public float PositionX { get; set; }
    [FirestoreProperty]
    public float PositionY { get; set; }
    [FirestoreProperty]
    public float PositionZ { get; set; }

    [FirestoreProperty]
    public float RotationX { get; set; }
    [FirestoreProperty]
    public float RotationY { get; set; }
    [FirestoreProperty]
    public float RotationZ { get; set; }

    [FirestoreProperty]
    public int SizeX { get; set; }
    [FirestoreProperty]
    public int SizeY { get; set; }

    public BuildingDto() { }

    public BuildingDto(Building building)
    {
        Type = building.Type.ToString();

        PositionX = building.Position.x;
        PositionY = building.Position.y;
        PositionZ = building.Position.z;

        RotationX = building.Rotation.x;
        RotationY = building.Rotation.y;
        RotationZ = building.Rotation.z;

        SizeX = building.Size.x;
        SizeY = building.Size.y;
    }

    public Building ToBuilding(string chunkId = null)
    {
        var building = new Building();
        building.Type = System.Enum.Parse<EBuildingType>(Type);
        building.Position = new Vector3(PositionX, PositionY, PositionZ);
        building.Rotation = new Vector3(RotationX, RotationY, RotationZ);
        building.Size = new Vector2Int(SizeX, SizeY);

        building.ChunkId = chunkId;

        return building;
    }
}