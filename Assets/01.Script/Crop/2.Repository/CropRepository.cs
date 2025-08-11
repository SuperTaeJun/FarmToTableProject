using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CropRepository : FirebaseRepositoryBase
{
    private const string CollectionName = "cropChunks";
    // 청크 단위로 모든 작물 저장 (익명 인증 사용)
    public async Task SaveCrops(string chunkId, List<Crop> crops)
    {
        await ExecuteAsync(async () =>
        {
            var dto = ConvertToDto(crops, chunkId);
            var docRef = Firestore.Collection(CollectionName).Document(UserId).Collection("crops").Document(chunkId);
            
            await docRef.SetAsync(dto);
        }, "작물 청크 저장");
    }

    // 청크별 작물 로드 (익명 인증 사용)
    public async Task<List<Crop>> LoadCropsByChunk(string chunkId)
    {
        return await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection(CollectionName).Document(UserId).Collection("crops").Document(chunkId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                Debug.Log($"[CropRepo] 작물 청크 없음: {chunkId}");
                return new List<Crop>();
            }

            var dto = snapshot.ConvertTo<CropChunkDocumentDto>();
            return ConvertToDomain(dto);
        }, "작물 청크 로드");
    }

    public async Task SaveSingleCrop(Crop crop)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection(CollectionName).Document(UserId).Collection("crops").Document(crop.ChunkId);
            var snapshot = await docRef.GetSnapshotAsync();

            CropChunkDocumentDto dto;
            
            if (snapshot.Exists)
            {
                dto = snapshot.ConvertTo<CropChunkDocumentDto>();
                
                // 기존 작물 제거 (같은 위치)
                dto.Crops.RemoveAll(c => Vector3.Distance(
                    new Vector3(c.PositionX, c.PositionY, c.PositionZ),
                    crop.Position) < 0.1f);
            }
            else
            {
                dto = new CropChunkDocumentDto
                {
                    ChunkId = crop.ChunkId,
                    Crops = new List<CropDto>()
                };
            }

            dto.Crops.Add(new CropDto(crop));
            await docRef.SetAsync(dto);
        }, "단일 작물 저장");
    }
    public async Task RemoveCrop(string chunkId, Vector3 position)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection(CollectionName).Document(UserId).Collection("crops").Document(chunkId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                var dto = snapshot.ConvertTo<CropChunkDocumentDto>();
                
                dto.Crops.RemoveAll(c => Vector3.Distance(
                    new Vector3(c.PositionX, c.PositionY, c.PositionZ),
                    position) < 0.1f);

                await docRef.SetAsync(dto);
            }
        }, "작물 제거");
    }
    // 작물 성장 업데이트 (익명 인증 사용)
    public async Task UpdateCropGrowth(string chunkId, Vector3 position, float newGrowthProgress)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection(CollectionName).Document(UserId).Collection("crops").Document(chunkId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                var dto = snapshot.ConvertTo<CropChunkDocumentDto>();

                var targetCrop = dto.Crops.Find(c => Vector3.Distance(
                    new Vector3(c.PositionX, c.PositionY, c.PositionZ),
                    position) < 0.1f);

                if (targetCrop != null)
                {
                    targetCrop.GrowthProgress = newGrowthProgress;

                    // 성장 단계 업데이트
                    if (newGrowthProgress >= 1.0f)
                        targetCrop.GrowthStage = (int)ECropGrowthStage.Harvest;
                    else if (newGrowthProgress >= 0.5f)
                        targetCrop.GrowthStage = (int)ECropGrowthStage.Mature;
                    else if (newGrowthProgress >= 0.2f)
                        targetCrop.GrowthStage = (int)ECropGrowthStage.Vegetative;
                    else
                        targetCrop.GrowthStage = (int)ECropGrowthStage.Seed;

                    await docRef.SetAsync(dto);
                }
            }
        }, "작물 성장 업데이트");
    }
    public async Task WaterCrop(string chunkId, Vector3 position)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection(CollectionName).Document(UserId).Collection("crops").Document(chunkId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                var dto = snapshot.ConvertTo<CropChunkDocumentDto>();

                var targetCrop = dto.Crops.Find(c => Vector3.Distance(
                    new Vector3(c.PositionX, c.PositionY, c.PositionZ),
                    position) < 0.1f);

                if (targetCrop != null)
                {
                    targetCrop.IsWatered = true;
                    if (GameTimeManager.Instance != null)
                    {
                        targetCrop.LastWateredDay = GameTimeManager.Instance.CurrentDay;
                        targetCrop.LastWateredHour = GameTimeManager.Instance.CurrentHour;
                    }

                    await docRef.SetAsync(dto);
                }
            }
        }, "작물 급수");
    }

    private CropChunkDocumentDto ConvertToDto(List<Crop> crops, string chunkId)
    {
        var dto = new CropChunkDocumentDto
        {
            ChunkId = chunkId,
            Crops = new List<CropDto>()
        };

        foreach (var crop in crops)
        {
            dto.Crops.Add(new CropDto(crop));
        }

        return dto;
    }

    private List<Crop> ConvertToDomain(CropChunkDocumentDto dto)
    {
        var crops = new List<Crop>();

        foreach (var cropDto in dto.Crops)
        {
            crops.Add(cropDto.ToCrop());
        }

        return crops;
    }

    public async Task<List<string>> GetAllCropChunkIds()
    {
        var chunkIds = new List<string>();

        var collection = Firestore.Collection(CollectionName).Document(UserId).Collection("crops");
        var snapshot = await collection.GetSnapshotAsync();

        foreach (var doc in snapshot.Documents)
        {
            chunkIds.Add(doc.Id);
        }

        return chunkIds;
    }

    public async Task DeleteAllData()
    {
        await ExecuteAsync(async () =>
        {
            // 1. crops 컬렉션의 모든 문서 조회
            var cropsCollection = Firestore.Collection(CollectionName)
                                           .Document(UserId)
                                           .Collection("crops");
            var snapshot = await cropsCollection.GetSnapshotAsync();
            
            // 2. 각 청크 문서 삭제
            foreach (var doc in snapshot.Documents)
            {
                await doc.Reference.DeleteAsync();
            }
            
            // 3. 상위 사용자 문서 삭제
            await Firestore.Collection(CollectionName)
                          .Document(UserId)
                          .DeleteAsync();
        }, "모든 작물 데이터 삭제");
    }
}
