using UnityEngine;

public enum EPlayerMode
{
    BlockEdit,    // ��������
    Farming,        // �����
    Construction    // �Ǽ����
}
public class PlayerModeController : MonoBehaviour
{
    [SerializeField] private EPlayerMode _currentMode = EPlayerMode.BlockEdit;
    public EPlayerMode CurrentMode => _currentMode;
    public DebugEvent<EPlayerMode> OnModeChanged = new DebugEvent<EPlayerMode>();

    private Player _owner;

    private void Start()
    {
        _owner = GetComponent<Player>();
        _owner.InputController.OnModeChangeInput.AddListener(SwitchMode);

        OnModeChanged.Invoke(_currentMode);
    }

    public void SwitchMode(EPlayerMode newMode)
    {
        if (_currentMode != newMode)
        {
            EPlayerMode oldMode = _currentMode;
            _currentMode = newMode;

            // 건축 모드가 아닐 때 PlayerSelectAbility의 그리드 크기를 1x1로 리셋
            if (_currentMode != EPlayerMode.Construction)
            {
                PlayerSelectAbility selectAbility = _owner.GetAbility<PlayerSelectAbility>();
                if (selectAbility != null)
                {
                    selectAbility.ResetToSingleCell();
                }
            }

            MainHudManager.Instance.RefreshPlayerModeIcon(_currentMode);
            OnModeChanged.Invoke(_currentMode);
        }
    }
}
