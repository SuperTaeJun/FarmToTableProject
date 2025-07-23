using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class BuildingDto
{
    [FirestoreProperty]
    public string Type { get; set; }

    [FirestoreProperty]
    public Vector3 Position { get; set; }

    [FirestoreProperty]
    public Vector3 Rotation { get; set; }

    [FirestoreProperty]
    public Vector3Int Size { get; set; }

    public BuildingDto() { }

    public BuildingDto(Building building)
    {
        Type = building.Type.ToString();
        Position = building.Position;
        Rotation = building.Rotation;
        Size = building.Size;
    }
}