using UnityEngine;

public class PlayerForageAbility : PlayerAbility
{
    private ForageObject _currentForage;
    private void Start()
    {
    }

    public bool CanForaging(out EForageType type)
    {
        ForageObject forage = ForageManager.Instance.GetForageAtWorldPosition(_owner.CurrentSelectedPos);

        if (forage == null)
        {
            type = EForageType.None;
            return false;
        }

        _currentForage = forage;
        type = forage.Type;
        return true;
    }
    public void OnForage()
    {
        if (_currentForage == null) return;

        ObjectPoolManager.Instance.Get(PoolType.SomkeL, _owner.CurrentSelectedPos);

        ForageManager.Instance.RemoveForage(_currentForage);
    }
}
