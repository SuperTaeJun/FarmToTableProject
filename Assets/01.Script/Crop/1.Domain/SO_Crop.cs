using UnityEngine;

[CreateAssetMenu(fileName = "SO_Crop", menuName = "Scriptable Objects/SO_Crop")]
public class SO_Crop : ScriptableObject
{
    [Header("Basic Info")]
    public ECropType Type;
    
    [Header("Growth Settings")]
    [Tooltip("전체 성장에 필요한 게임 시간 (시간 단위)")]
    public float GrowthTimeInHours = 24f;
    
    [Header("Growth Stage Timings")]
    [Tooltip("씨앗 -> 영양 단계로 넘어가는 비율 (0~1)")]
    [Range(0f, 1f)]
    public float VegetativeStageRatio = 0.2f;
    
    [Tooltip("영양 -> 성숙 단계로 넘어가는 비율 (0~1)")]
    [Range(0f, 1f)]
    public float MatureStageRatio = 0.5f;
    
    [Tooltip("성숙 -> 수확 단계로 넘어가는 비율 (0~1)")]
    [Range(0f, 1f)]
    public float HarvestStageRatio = 1.0f;
}
