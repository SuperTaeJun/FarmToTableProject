using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class WorldRepository : FirebaseRepositoryBase
{
    private const string CollectionName = "worldChunks";

    public async Task SaveChunkAsync(Chunk chunk)
    {
        await ExecuteAsync(async () =>
        {
            var dto = ConvertToDto(chunk);

            var docId = GetDocumentId(chunk.Position);
            var docRef = Firestore.Collection(CollectionName).Document(docId);

            await docRef.SetAsync(dto);
        }, "SaveChunk");
    }

    public async Task<Chunk> LoadChunkAsync(ChunkPosition pos)
    {
        return await ExecuteAsync(async () =>
        {
            var docId = GetDocumentId(pos);
            var docRef = Firestore.Collection(CollectionName).Document(docId);

            var snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists)
            {
                Debug.Log($"[WorldRepo] 청크 없음: {docId}");
                return null;
            }

            var dto = snapshot.ConvertTo<WorldDocumentDto>();
            return ConvertToDomain(dto);
        }, "LoadChunk");
    }
    private string GetDocumentId(ChunkPosition pos)
    {
        return $"{pos.X}_{pos.Y}_{pos.Z}";
    }

    public WorldDocumentDto ConvertToDto(Chunk chunk)
    {
        var dto = new WorldDocumentDto
        {
            ChunkX = chunk.Position.X,
            ChunkY = chunk.Position.Y,
            ChunkZ = chunk.Position.Z,
            Blocks = new List<BlockDto>()
        };

        for (int x = 0; x < Chunk.ChunkSize; x++)
        {
            for (int y = 0; y < Chunk.ChunkSize; y++)
            {
                for (int z = 0; z < Chunk.ChunkSize; z++)
                {
                    var block = chunk.GetBlock(x, y, z);
                    if (block != null)
                    {
                        dto.Blocks.Add(new BlockDto
                        {
                            Type = (int)block.Type,
                            X = block.Position.X,
                            Y = block.Position.Y,
                            Z = block.Position.Z
                        });
                    }
                }
            }
        }
        Debug.Log($"[ConvertToDto] Saving Chunk {chunk.Position.X},{chunk.Position.Z} with {dto.Blocks.Count} blocks.");

        return dto;
    }

    public Chunk ConvertToDomain(WorldDocumentDto dto)
    {
        var chunkPos = new ChunkPosition(dto.ChunkX, dto.ChunkY, dto.ChunkZ);
        var chunk = new Chunk(chunkPos);

        int blockCount = 0;
        if (dto.Blocks != null)
        {
            foreach (var b in dto.Blocks)
            {
                var block = new Block(
                    (EBlockType)b.Type,
                    new BlockPosition(b.X, b.Y, b.Z)
                );

                chunk.SetBlock(block);
                blockCount++;
            }
        }
        Debug.Log($"[ConvertToDomain] Block count after conversion: {blockCount}");

        return chunk;
    }
    public async Task<List<ChunkPosition>> GetAllChunkPositionsFromFirebase()
    {
        var chunkPositions = new List<ChunkPosition>();

        var collection = Firestore.Collection("worldChunks");
        var snapshot = await collection.GetSnapshotAsync();

        foreach (var doc in snapshot.Documents)
        {
            string id = doc.Id; // e.g. "2_0_1"
            var split = id.Split('_');
            if (split.Length == 3)
            {
                int x = int.Parse(split[0]);
                int y = int.Parse(split[1]);
                int z = int.Parse(split[2]);

                chunkPositions.Add(new ChunkPosition(x, y, z));
            }
            else
            {
                Debug.LogWarning($"[WorldManager] 잘못된 청크 ID 형식: {id}");
            }
        }

        Debug.Log($"[WorldManager] Firebase에서 {chunkPositions.Count}개의 청크를 발견했습니다.");

        return chunkPositions;
    }
}
