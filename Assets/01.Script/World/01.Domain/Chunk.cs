using UnityEngine;
using System.Collections.Generic;
using System;
using Firebase.Firestore;
public struct ChunkPosition
{
    public int X;
    public int Y;
    public int Z;

    public ChunkPosition(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}
public class Chunk
{
    public const int ChunkSize = 16;

    public ChunkPosition Position { get; private set; }
    public Block[,,] Blocks { get; private set; }

    public Chunk(ChunkPosition position)
    {
        Position = position;
        Blocks = new Block[ChunkSize, ChunkSize, ChunkSize];
    }

    public void SetBlock(Block block)
    {
        var pos = block.Position;
        if (IsValidPosition(pos))
        {
            Blocks[pos.X, pos.Y, pos.Z] = block;
        }
    }

    public Block GetBlock(int x, int y, int z)
    {
        if (IsValidPosition(new BlockPosition(x, y, z)))
        {
            return Blocks[x, y, z];
        }
        return null;
    }

    private bool IsValidPosition(BlockPosition pos)
    {
        return pos.X >= 0 && pos.X < ChunkSize &&
               pos.Y >= 0 && pos.Y < ChunkSize &&
               pos.Z >= 0 && pos.Z < ChunkSize;
    }

}

[FirestoreData]
public class WorldDocumentDto
{
    [FirestoreProperty]
    public int ChunkX { get; set; }

    [FirestoreProperty]
    public int ChunkY { get; set; }

    [FirestoreProperty]
    public int ChunkZ { get; set; }

    [FirestoreProperty]
    public List<BlockDto> Blocks { get; set; }
}