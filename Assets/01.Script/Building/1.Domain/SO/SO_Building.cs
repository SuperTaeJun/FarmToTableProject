using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "SO_Building", menuName = "Scriptable Objects/SO_Building")]
public class SO_Building : ScriptableObject
{
    public EBuildingType Type;
    public AssetReference Prefab;
    public AssetReference PreviewPrefab;
    public Vector2Int Size = Vector2Int.one;
    public float Cost;
}
