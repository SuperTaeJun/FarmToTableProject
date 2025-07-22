using UnityEngine;

public class BuildingData
{
    public string Id;                    // 청크위치
    public BuildingType Type;            // 
    public Vector3Int Position;          // 청크에서의 로컬 위치
    public Vector3Int Size;              // 차지하는 사이즈 2x2 3x3...
    public int Rotation;                 // 
}
