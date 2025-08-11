using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;

public class ForageRepository : FirebaseRepositoryBase
{
    private const string COLLECTION_NAME = "forages";

    public async Task SaveForages(string chunkId, List<Forage> forages)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection(COLLECTION_NAME).Document(UserId).Collection("chunks").Document(chunkId);

            var forageDataList = new List<Dictionary<string, object>>();

            foreach (var forage in forages)
            {
                var data = new Dictionary<string, object>
            {
                { "type", forage.Type.ToString() },
                { "position", Vector3ToDict(forage.Position) },
                { "rotation", Vector3ToDict(forage.Rotation) }
            };

                forageDataList.Add(data);
            }

            var docData = new Dictionary<string, object>
        {
            { "objects", forageDataList }
        };

            await docRef.SetAsync(docData);
        }, $"청크 채집 오브젝트 저장 [{chunkId}]");
    }

    public async Task<List<Forage>> LoadForagesByChunk(string chunkId)
    {
        return await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Collection(COLLECTION_NAME).Document(UserId).Collection("chunks").Document(chunkId);
            var snapshot = await docRef.GetSnapshotAsync();

            var result = new List<Forage>();

            if (snapshot.Exists && snapshot.ContainsField("objects"))
            {
                var list = snapshot.GetValue<List<object>>("objects");

                foreach (var obj in list)
                {
                    var dict = obj as Dictionary<string, object>;

                    var typeStr = dict["type"].ToString();
                    var type = Enum.Parse<EForageType>(typeStr);

                    var posDict = dict["position"] as Dictionary<string, object>;
                    var rotDict = dict["rotation"] as Dictionary<string, object>;

                    Vector3 pos = DictToVector3(posDict);
                    Vector3 rot = DictToVector3(rotDict);

                    var forage = new Forage(type, chunkId, pos, rot);
                    result.Add(forage);
                }
            }

            return result;
        }, $"청크 채집 오브젝트 로드 [{chunkId}]");
    }
    private Dictionary<string, object> Vector3ToDict(Vector3 v)
    {
        return new Dictionary<string, object>
        {
            { "x", v.x },
            { "y", v.y },
            { "z", v.z }
        };
    }

    private Vector3 DictToVector3(Dictionary<string, object> dict)
    {
        return new Vector3(
            Convert.ToSingle(dict["x"]),
            Convert.ToSingle(dict["y"]),
            Convert.ToSingle(dict["z"])
        );
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
        }, "모든 채집 오브젝트 데이터 삭제");
    }
}
