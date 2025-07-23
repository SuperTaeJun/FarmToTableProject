using UnityEngine;

[CreateAssetMenu(fileName = "SO_Building", menuName = "Scriptable Objects/SO_Building")]
public class SO_Building : ScriptableObject
{
    public BuildingType Type;
    public GameObject Prefab;
    public Vector3Int Size = Vector3Int.one;
    public float Cost;
    public string DisplayName;
    public Sprite Icon;
}
