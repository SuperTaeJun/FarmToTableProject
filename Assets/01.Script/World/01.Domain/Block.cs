using UnityEngine;

public enum EBlockType
{
    Dirt,
    Grass
}

public class Block
{
    EBlockType type;
    Vector3 chunkLocalPos;
}
