using UnityEngine;

[CreateAssetMenu(fileName = "SO_Building", menuName = "Scriptable Objects/SO_Building")]
public class SO_Building : ScriptableObject
{
    public EBuildingType Type;
    public GameObject Prefab;
    public Vector2Int Size = Vector2Int.one;
    public float Cost;
}
