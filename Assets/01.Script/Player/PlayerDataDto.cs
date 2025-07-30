using System;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class PlayerDataDto
{
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
    public float RotationW { get; set; }

    [FirestoreProperty]
    public DateTime LastSaved { get; set; }

    public Vector3 GetPosition()
    {
        return new Vector3(PositionX, PositionY, PositionZ);
    }

    public Quaternion GetRotation()
    {
        return new Quaternion(RotationX, RotationY, RotationZ, RotationW);
    }

    public void SetPosition(Vector3 position)
    {
        PositionX = position.x;
        PositionY = position.y;
        PositionZ = position.z;
    }

    public void SetRotation(Quaternion rotation)
    {
        RotationX = rotation.x;
        RotationY = rotation.y;
        RotationZ = rotation.z;
        RotationW = rotation.w;
    }
}