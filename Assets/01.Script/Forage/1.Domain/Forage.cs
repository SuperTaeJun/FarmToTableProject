using System;
using UnityEngine;
public enum EForageType
{
    Tree,
    Stone
}
public class Forage
{
    public EForageType Type { get; set; }
    public string ChunkId { get; set; }

    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }

    public Forage(EForageType type, string chunkId, Vector3 position, Vector3 rotation)
    {
        Type = type;
        ChunkId = chunkId;
        Position = position;
        Rotation = rotation;
    }
}
[Serializable]
public class ForageData
{
    public string Type;
    public Vector3 Position;
    public Vector3 Rotation;

    public ForageData() { }

    public ForageData(Forage forage)
    {
        Type = forage.Type.ToString();
        Position = forage.Position;
        Rotation = forage.Rotation;
    }
}