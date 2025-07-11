using UnityEngine;

[CreateAssetMenu(fileName = "SO_PlayerData", menuName = "Scriptable Objects/SO_PlayerData")]
public class SO_PlayerData : ScriptableObject
{
    [Header("스테이트 설정")]
    public float MaxHealth;
    public float WalkSpeed;
    public float RunSpeed;
    public float JumpPower;


}
