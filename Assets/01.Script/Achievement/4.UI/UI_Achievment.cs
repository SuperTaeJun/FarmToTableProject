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
        // 완료된 업적의 슬롯을 찾아서 삭제
        var slotToRemove = SlotList.FirstOrDefault(slot => 
            slot.GetAchievement()?.Name == completedAchievement.Name);

        if (slotToRemove != null)
        {
            SlotList.Remove(slotToRemove);
            Destroy(slotToRemove.gameObject);
        }
    }

    private void OnProgressUpdated(Achievment updatedAchievement)
    {
        // 해당 슬롯의 UI 업데이트
        var slot = SlotList.FirstOrDefault(s => 
            s.GetAchievement()?.Name == updatedAchievement.Name);
        
        slot?.UpdateUI();
    }

    private void OnAchievementsLoaded(List<Achievment> achievements)
    {
        RefreshAchievementUI();
    }

    public void RefreshAchievementUI()
    {
        // 기존 슬롯들 정리
        ClearSlots();

        if (AchievmentManager.Instance == null) return;

        // 미완료된 업적들만 가져와서 카테고리별로 정렬
        var incompleteAchievements = AchievmentManager.Instance.GetIncompleteAchievements();
        
        // 튜토리얼 -> 일간 -> 주간 순서로 정렬
        var sortedAchievements = incompleteAchievements
            .OrderBy(a => GetCategoryOrder(a.Category))
            .ToList();

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
            EAchievmentCategory.Tutorial => 0,  // 튜토리얼이 제일 위
            EAchievmentCategory.Daily => 1,     // 일간이 두번째
            EAchievmentCategory.Weekly => 2,    // 주간이 세번째
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
