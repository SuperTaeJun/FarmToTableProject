using UnityEngine;

[CreateAssetMenu(fileName = "SO_Crop", menuName = "Scriptable Objects/SO_Crop")]
public class SO_Crop : ScriptableObject
{
    public ECropType Type;
    public float Cost;
    public float GrowthTimeTotal;
}
