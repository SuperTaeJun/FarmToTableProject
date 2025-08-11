using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;

public class AchievmentRepository : FirebaseRepositoryBase
{
    private const string COLLECTION_NAME = "Achievements";

    private string GetUserAchievementPath()
    {
        return $"{COLLECTION_NAME}/{UserId}";
    }

    public async Task SaveAchievements(List<Achievment> achievements)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Document(GetUserAchievementPath());

            var achievementData = new Dictionary<string, object>();

            foreach (var achievement in achievements)
            {
                var data = new Dictionary<string, object>
                {
                    { "name", achievement.Name },
                    { "description", achievement.Description },
                    { "category", (int)achievement.Category },
                    { "achievementType", (int)achievement.AchievementType },
                    { "targetValue", achievement.TargetValue },
                    { "currentValue", achievement.CurrentValue },
                    { "reward", (int)achievement.Reward },
                    { "rewardAmount", achievement.RewardAmount },
                    { "createdDay", achievement.CreatedDay },
                    { "isCompleted", achievement.IsCompleted }
                };

                achievementData[achievement.Name] = data;
            }

            await docRef.SetAsync(achievementData);
        }, "업적 저장");
    }

    public async Task<List<Achievment>> LoadAchievements()
    {
        return await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Document(GetUserAchievementPath());
            var snapshot = await docRef.GetSnapshotAsync();

            var result = new List<Achievment>();

            if (snapshot.Exists)
            {
                var achievementData = snapshot.ToDictionary();

                foreach (var kvp in achievementData)
                {
                    if (kvp.Value is Dictionary<string, object> data)
                    {
                        var achievement = new Achievment(
                            data["name"].ToString(),
                            data["description"].ToString(),
                            (EAchievmentCategory)System.Convert.ToInt32(data["category"]),
                            (EAchievementType)System.Convert.ToInt32(data["achievementType"]),
                            System.Convert.ToInt32(data["targetValue"]),
                            (ECurrencyType)System.Convert.ToInt32(data["reward"]),
                            System.Convert.ToInt32(data["rewardAmount"]),
                            System.Convert.ToInt32(data["createdDay"])
                        );

                        achievement.SetProgress(System.Convert.ToInt32(data["currentValue"]));
                        result.Add(achievement);
                    }
                }
            }

            return result;
        }, "업적 불러오기");
    }

    public async Task UpdateAchievementProgress(string achievementName, int newProgress)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Document(GetUserAchievementPath());
            var updates = new Dictionary<string, object>
            {
                { $"{achievementName}.currentValue", newProgress }
            };

            await docRef.UpdateAsync(updates);
        }, $"업적 진행도 업데이트 [{achievementName}] to {newProgress}");
    }

}
