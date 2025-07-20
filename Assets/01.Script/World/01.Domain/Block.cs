using Firebase.Firestore;
using System;
using UnityEngine;

public enum EBlockType
{
    Air,
    Dirt,
    Grass
}
public struct BlockPosition
{
    public int X;
    public int Y;
    public int Z;

    public BlockPosition(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

public class Block
{
    public EBlockType Type { get; private set; }
    public BlockPosition Position { get; private set; }

    public Block(EBlockType type, BlockPosition position)
    {
        Type = type;
        Position = position;
    }
}

[FirestoreData]
public class BlockDto
{
    [FirestoreProperty]
    public int Type { get; set; }

    [FirestoreProperty]
    public int X { get; set; }

    [FirestoreProperty]
    public int Y { get; set; }

    [FirestoreProperty]
    public int Z { get; set; }
}