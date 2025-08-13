using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class UI_Achievment : MonoBehaviour
{
    [SerializeField] private Transform Content;

    [SerializeField] private GameObject AchievementSlotPrefab;

    private List<UI_AchievmentSlot> SlotList = new List<UI_AchievmentSlot>();

    private void Start()
    {
        SubscribeToEvents();
        RefreshAchievementUI();
    }

    private void SubscribeToEvents()
    {
        if (AchievmentManager.Instance != null)
        {
            AchievmentManager.Instance.OnAchievementCompleted.AddListener(OnAchievementCompleted);
            AchievmentManager.Instance.OnAchievementProgressUpdated.AddListener(OnProgressUpdated);
            AchievmentManager.Instance.OnAchievementsLoaded.AddListener(OnAchievementsLoaded);
        }
    }

    private void OnAchievementCompleted(Achievment completedAchievement)
    {
        var slotToRemove = SlotList.FirstOrDefault(slot => slot.GetAchievement()?.Name == completedAchievement.Name);

        if (slotToRemove != null)
        {
            SlotList.Remove(slotToRemove);
            slotToRemove.GetComponent<UI_AchievmentSlot>()?.PlayCompletionEffect();
        }
    }

    private void OnProgressUpdated(Achievment updatedAchievement)
    {
        var slot = SlotList.FirstOrDefault(slot => slot.GetAchievement()?.Name == updatedAchievement.Name);

        slot?.UpdateUI();
    }

    private void OnAchievementsLoaded(List<Achievment> achievements)
    {
        RefreshAchievementUI();
    }

    public void RefreshAchievementUI()
    {
        ClearSlots();

        if (AchievmentManager.Instance == null) return;

        var incompleteAchievements = AchievmentManager.Instance.GetIncompleteAchievements();

        var sortedAchievements = incompleteAchievements.OrderBy(a => GetCategoryOrder(a.Category)).ToList();

        foreach (var achievement in sortedAchievements)
        {
            CreateAchievementSlot(achievement);
        }
    }

    private void CreateAchievementSlot(Achievment achievement)
    {
        if (AchievementSlotPrefab == null) return;

        GameObject slotObj = Instantiate(AchievementSlotPrefab, Content);
        UI_AchievmentSlot slot = slotObj.GetComponent<UI_AchievmentSlot>();

        if (slot != null)
        {
            slot.Setup(achievement);
            SlotList.Add(slot);
        }
    }

    private void ClearSlots()
    {
        foreach (var slot in SlotList)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        SlotList.Clear();
    }

    private int GetCategoryOrder(EAchievmentCategory category)
    {
        return category switch
        {
            EAchievmentCategory.Tutorial => 0,
            EAchievmentCategory.Daily => 1,
            EAchievmentCategory.Weekly => 2,
            _ => 3
        };
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (AchievmentManager.Instance != null)
        {
            AchievmentManager.Instance.OnAchievementCompleted.RemoveListener(OnAchievementCompleted);
            AchievmentManager.Instance.OnAchievementProgressUpdated.RemoveListener(OnProgressUpdated);
            AchievmentManager.Instance.OnAchievementsLoaded.RemoveListener(OnAchievementsLoaded);
        }
    }






}
