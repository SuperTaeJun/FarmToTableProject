using UnityEngine;
public enum EAchievmentCategory
{
    Daily,
    Weekly,
    Tutorial
}

public enum EAchievementType
{
    PlantCrop,
    HarvestCrop,   
    WaterCrop,
    BuildBuilding,
    BuyFromShop,
    SellToShop,
    EarnCurrency,
    SpendCurrency  
}

public class Achievment
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public EAchievmentCategory Category { get; private set; }
    public EAchievementType AchievementType { get; private set; }

    public int TargetValue { get; private set; }
    public int CurrentValue { get; private set; }

    public int ItemIndex { get; private set; }

    public ECurrencyType Reward { get; private set; }
    public int RewardAmount { get; private set; }

    public int CreatedDay { get; private set; }
    

    public bool IsCompleted => CurrentValue >= TargetValue;
    public float Progress => (float)CurrentValue / TargetValue;

    public Achievment(string name, string description, EAchievmentCategory category,
                     EAchievementType achievementType, int targetValue,
                     ECurrencyType reward, int rewardAmount, int createdDay, object specificTarget = null)
    {
        Name = name;
        Description = description;
        Category = category;
        AchievementType = achievementType;
        TargetValue = targetValue;
        Reward = reward;
        RewardAmount = rewardAmount;
        CreatedDay = createdDay;

        CurrentValue = 0;
    }

    public bool TryProgress(int amount = 1)
    {
        if (IsCompleted) return false;

        CurrentValue += amount;
        return IsCompleted;
    }

    public void SetProgress(int value)
    {
        CurrentValue = Mathf.Min(value, TargetValue);
    }
}
