using UnityEngine;

public class Building
{
    public EBuildingType Type { get; set; }
    public string ChunkId { get; set; }
    public Vector3 Position { get; set; }    // ûũ �� ���� ������
    public Vector3 Rotation { get; set; }
    public Vector2Int Size { get; set; }     // �����ϴ� ũ�� (1x1 �⺻)
    public Building() { }
    public Building(EBuildingType type, string chunkId, Vector3 position, Vector3 rotation, Vector2Int size)
    {
        Type = type;
        ChunkId = chunkId;
        Position = position;
        Rotation = rotation;
        Size = size;
    }
    // ���� ID ����
    public string GetBuildingId() => $"{ChunkId}_{Position.x}_{Position.y}_{Position.z}";
}