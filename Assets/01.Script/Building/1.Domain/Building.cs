using UnityEngine;

public class Building
{
    public BuildingType Type { get; set; }
    public string ChunkId { get; set; }
    public Vector3 Position { get; set; }    // 청크 내 로컬 포지션
    public Vector3 Rotation { get; set; }
    public Vector3Int Size { get; set; }     // 차지하는 크기 (1x1x1 기본)

    public Building(BuildingType type, string chunkId, Vector3 position, Vector3 rotation, Vector3Int size)
    {
        Type = type;
        ChunkId = chunkId;
        Position = position;
        Rotation = rotation;
        Size = size;
    }

    // 고유 ID 생성
    public string GetBuildingId() => $"{ChunkId}_{Position.x}_{Position.y}_{Position.z}";
}
