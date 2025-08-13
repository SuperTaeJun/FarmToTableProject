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
        Debug.Log(forage.Type);

        // 캐싱
        _currentForage = forage;
        //타입에 따라서 애니메이션이 분기되어야해 out으로 값 반환
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
