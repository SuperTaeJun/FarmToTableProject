using UnityEngine;
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
