using Firebase.Firestore;
using System.Collections.Generic;

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