using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
public class BuildingRepository : FirebaseRepositoryBase
{
    private const string COLLECTION_NAME = "buildings";
    public async Task SaveBuildings(string chunkId, List<Building> buildings)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection(COLLECTION_NAME).Document(chunkId);
            var buildingDtoList = new List<BuildingDto>();
            foreach (var building in buildings)
            {
                buildingDtoList.Add(new BuildingDto(building));
            }
            var docData = new Dictionary<string, object>
            {
                { COLLECTION_NAME, buildingDtoList }
            };
            await docRef.SetAsync(docData);
        }, $"������ ���� ûũ : [{chunkId}]");
    }
    public async Task<List<Building>> LoadBuildingByChunk(string chunkId)
    {
        return await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection(COLLECTION_NAME).Document(chunkId);
            var snapshot = await docRef.GetSnapshotAsync();
            var result = new List<Building>();

            if (snapshot.Exists && snapshot.ContainsField(COLLECTION_NAME))
            {
                var buildingDtos = snapshot.ConvertTo<Dictionary<string, List<BuildingDto>>>()[COLLECTION_NAME];

                if (buildingDtos != null)
                {
                    foreach (var buildingDto in buildingDtos)
                    {
                        if (buildingDto != null)
                        {
                            result.Add(buildingDto.ToBuilding(chunkId)); // chunkId ����
                        }
                    }
                }
            }
            return result;
        }, $"������ �ҷ����� ûũ : [{chunkId}]");
    }
    public async Task SaveSingleBuilding(Building building)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection(COLLECTION_NAME).Document(building.ChunkId);
            var snapshot = await docRef.GetSnapshotAsync();
            List<BuildingDto> buildingList = new List<BuildingDto>();
            if (snapshot.Exists && snapshot.ContainsField(COLLECTION_NAME))
            {
                buildingList = snapshot.ConvertTo<Dictionary<string, List<BuildingDto>>>()[COLLECTION_NAME];
            }
            // �� �۹� �߰�
            buildingList.Add(new BuildingDto(building));
            var docData = new Dictionary<string, object>
            {
                { COLLECTION_NAME, buildingList }
            };
            await docRef.SetAsync(docData);
        }, $"�ǹ��ϳ� ���� ��ġ : [{building.Position}] ûũ : [{building.ChunkId}]");
    }
    public async Task RemoveBuilding(string chunkId, Vector3 position)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection(COLLECTION_NAME).Document(chunkId);
            var snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists && snapshot.ContainsField(COLLECTION_NAME))
            {
                var buildingList = snapshot.ConvertTo<Dictionary<string, List<BuildingDto>>>()[COLLECTION_NAME];
                var docData = new Dictionary<string, object>
                {
                    { COLLECTION_NAME, buildingList }
                };
                await docRef.SetAsync(docData);
            }
        }, $"�ǹ� ���� ��ġ : [{position}] ûũ : [{chunkId}]");
    }
}
