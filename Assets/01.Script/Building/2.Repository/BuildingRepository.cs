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
            var docRef = Firestore.Collection(COLLECTION_NAME).Document(UserId).Collection("chunks").Document(chunkId);
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
        }, $"건물 저장 청크 : [{chunkId}]");

    }
    public async Task<List<Building>> LoadBuildingByChunk(string chunkId)
    {
        return await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection(COLLECTION_NAME).Document(UserId).Collection("chunks").Document(chunkId);
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
                            result.Add(buildingDto.ToBuilding(chunkId)); // chunkId 전달
                        }
                    }
                }
            }
            return result;
        }, $"건물 불러오기 청크 : [{chunkId}]");
    }
    public async Task SaveSingleBuilding(Building building)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection(COLLECTION_NAME).Document(UserId).Collection("chunks").Document(building.ChunkId);
            var snapshot = await docRef.GetSnapshotAsync();
            List<BuildingDto> buildingList = new List<BuildingDto>();
            if (snapshot.Exists && snapshot.ContainsField(COLLECTION_NAME))
            {
                buildingList = snapshot.ConvertTo<Dictionary<string, List<BuildingDto>>>()[COLLECTION_NAME];
            }
            // 새 건물 추가
            buildingList.Add(new BuildingDto(building));
            var docData = new Dictionary<string, object>
            {
                { COLLECTION_NAME, buildingList }
            };
            await docRef.SetAsync(docData);
        }, $"건물하나 저장 위치 : [{building.Position}] 청크 : [{building.ChunkId}]");
    }
    public async Task RemoveBuilding(string chunkId, Vector3 position)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection(COLLECTION_NAME).Document(UserId).Collection("chunks").Document(chunkId);
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
        }, $"건물 제거 위치 : [{position}] 청크 : [{chunkId}]");
    }

    public async Task DeleteAllData()
    {
        await ExecuteAsync(async () =>
        {
            // 1. chunks 컬렉션의 모든 문서 조회
            var chunksCollection = Firestore.Collection(COLLECTION_NAME)
                                           .Document(UserId)
                                           .Collection("chunks");
            var snapshot = await chunksCollection.GetSnapshotAsync();
            
            // 2. 각 청크 문서 삭제
            foreach (var doc in snapshot.Documents)
            {
                await doc.Reference.DeleteAsync();
            }
            
            // 3. 상위 사용자 문서 삭제
            await Firestore.Collection(COLLECTION_NAME)
                          .Document(UserId)
                          .DeleteAsync();
        }, "모든 건물 데이터 삭제");
    }
}
