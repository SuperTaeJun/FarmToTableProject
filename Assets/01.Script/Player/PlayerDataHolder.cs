using UnityEngine;

public class PlayerDataHolder : MonoBehaviourSingleton<PlayerDataHolder>
{
    [Header("저장 데이터")]
    [SerializeField] private Vector3 _savedPos;
    [SerializeField] private Quaternion _savedRot;

    public Vector3 SavedPos => _savedPos;
    public Quaternion SavedRot => _savedRot;
    private bool _hasSavedData = false;

    public void SavedData(Vector3 newPos, Quaternion newRot)
    {
        _savedPos = newPos;
        _savedRot = newRot;
        _hasSavedData = true;
    }

    public bool IsSavedData()
    {
        return _hasSavedData;
    }
}
