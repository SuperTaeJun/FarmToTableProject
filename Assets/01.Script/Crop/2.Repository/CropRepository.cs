using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CropRepository : FirebaseRepositoryBase
{
    public async Task SaveCrops(string chunkId, List<Crop> crops)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection("crops").Document(chunkId);

            var cropDataList = new List<CropDto>();

            foreach (var crop in crops)
            {
                cropDataList.Add(new CropDto(crop));
            }

            var docData = new Dictionary<string, object>
            {
                { "crops", cropDataList }
            };

            await docRef.SetAsync(docData);
        }, $"Save Crops for Chunk [{chunkId}]");
    }

    public async Task<List<Crop>> LoadCropsByChunk(string chunkId)
    {
        return await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection("crops").Document(chunkId);
            var snapshot = await docRef.GetSnapshotAsync();

            var result = new List<Crop>();

            if (snapshot.Exists && snapshot.ContainsField("crops"))
            {
                var cropDtos = snapshot.ConvertTo<Dictionary<string, List<CropDto>>>()["crops"];

                foreach (var cropDto in cropDtos)
                {
                    result.Add(cropDto.ToCrop());
                }
            }

            return result;
        }, $"Load Crops for Chunk [{chunkId}]");
    }

    public async Task SaveSingleCrop(Crop crop)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection("crops").Document(crop.ChunkId);
            var snapshot = await docRef.GetSnapshotAsync();

            List<CropDto> cropList = new List<CropDto>();

            if (snapshot.Exists && snapshot.ContainsField("crops"))
            {
                cropList = snapshot.ConvertTo<Dictionary<string, List<CropDto>>>()["crops"];
            }

            // ���� ��ġ�� ���� �۹� ����
            cropList.RemoveAll(c => Vector3.Distance(
                new Vector3(c.PositionX, c.PositionY, c.PositionZ),
                crop.Position) < 0.1f);

            // �� �۹� �߰�
            cropList.Add(new CropDto(crop));

            var docData = new Dictionary<string, object>
            {
                { "crops", cropList }
            };

            await docRef.SetAsync(docData);
        }, $"Save Single Crop at [{crop.Position}] in Chunk [{crop.ChunkId}]");
    }
    public async Task RemoveCrop(string chunkId, Vector3 position)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection("crops").Document(chunkId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists && snapshot.ContainsField("crops"))
            {
                var cropList = snapshot.ConvertTo<Dictionary<string, List<CropDto>>>()["crops"];

                // �ش� ��ġ�� �۹� ����
                cropList.RemoveAll(c => Vector3.Distance(
                    new Vector3(c.PositionX, c.PositionY, c.PositionZ),
                    position) < 0.1f);

                var docData = new Dictionary<string, object>
                {
                    { "crops", cropList }
                };

                await docRef.SetAsync(docData);
            }
        }, $"Remove Crop at [{position}] in Chunk [{chunkId}]");
    }
    public async Task UpdateCropGrowth(string chunkId, Vector3 position, float newGrowthProgress)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection("crops").Document(chunkId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists && snapshot.ContainsField("crops"))
            {
                var cropList = snapshot.ConvertTo<Dictionary<string, List<CropDto>>>()["crops"];

                // �ش� ��ġ�� �۹� ã�Ƽ� ���嵵 ������Ʈ
                var targetCrop = cropList.Find(c => Vector3.Distance(
                    new Vector3(c.PositionX, c.PositionY, c.PositionZ),
                    position) < 0.1f);

                if (targetCrop != null)
                {
                    targetCrop.GrowthProgress = newGrowthProgress;

                    // ���� �ܰ� ����
                    if (newGrowthProgress >= 1.0f)
                        targetCrop.GrowthStage = (int)ECropGrowthStage.Harvest;
                    else if (newGrowthProgress >= 0.5f)
                        targetCrop.GrowthStage = (int)ECropGrowthStage.Mature;
                    else if (newGrowthProgress >= 0.2f)
                        targetCrop.GrowthStage = (int)ECropGrowthStage.Vegetative;
                    else
                        targetCrop.GrowthStage = (int)ECropGrowthStage.Seed;

                    var docData = new Dictionary<string, object>
                    {
                        { "crops", cropList }
                    };

                    await docRef.SetAsync(docData);
                }
            }
        }, $"Update Crop Growth at [{position}] in Chunk [{chunkId}]");
    }
    public async Task WaterCrop(string chunkId, Vector3 position)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection("crops").Document(chunkId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists && snapshot.ContainsField("crops"))
            {
                var cropList = snapshot.ConvertTo<Dictionary<string, List<CropDto>>>()["crops"];

                // �ش� ��ġ�� �۹� ã�Ƽ� ���ֱ�
                var targetCrop = cropList.Find(c => Vector3.Distance(
                    new Vector3(c.PositionX, c.PositionY, c.PositionZ),
                    position) < 0.1f);

                if (targetCrop != null)
                {
                    targetCrop.IsWatered = true;
                    targetCrop.LastWateredTime = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime());

                    var docData = new Dictionary<string, object>
                    {
                        { "crops", cropList }
                    };

                    await docRef.SetAsync(docData);
                }
            }
        }, $"Water Crop at [{position}] in Chunk [{chunkId}]");
    }
}
