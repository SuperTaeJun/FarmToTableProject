using UnityEngine;

public class PlayerBlockAbility : PlayerAbility
{
    // ���� ��� (�ı� �Ǵ� ��ġ)
    private bool _isDestroyMode = true;

    // ���������� �ı��� �� Ÿ��
    private EBlockType _lastDestroyedBlockType = EBlockType.Air;

    private void Start()
    {
        // ��� ���� �̺�Ʈ ����
        _owner.ModeController.OnModeChanged.AddListener(OnModeChanged);
        // �ʱ� ��� Ȯ��
        OnModeChanged(_owner.ModeController.CurrentMode);
    }

    private void OnModeChanged(EPlayerMode newMode)
    {
        bool isBlockEditMode = (newMode == EPlayerMode.BlockEdit);
        enabled = isBlockEditMode;

        if (isBlockEditMode)
        {
            Debug.Log("[PlayerBlockAbility] �� ���� ��� Ȱ��ȭ");
            // �� ���� ��� ���� �� �ı� ���� �ʱ�ȭ
            _isDestroyMode = true;
            _lastDestroyedBlockType = EBlockType.Air;
        }
        else
        {
            Debug.Log("[PlayerBlockAbility] �� ���� ��� ��Ȱ��ȭ");
        }
    }

    public void OnBlockEditInput()
    {
        if (!enabled) return;

        Vector3 groundPosition = _owner.CurrentSelectedPos;

        if (_isDestroyMode)
        {
            // �ı� ���: �ٴ� �� �ı�
            Vector3 belowPosition = groundPosition + Vector3.down * 0.25f;
            EBlockType belowBlockType = WorldManager.Instance.GetBlockType(belowPosition);

            if (belowBlockType != EBlockType.Air)
            {
                DestroyBlockAtPosition(belowPosition);
            }
            else
            {
                Debug.Log("[PlayerBlockAbility] �ı��� ���� �����ϴ�.");
            }
        }
        else
        {
            // ��ġ ���: ���ʿ� �� ��ġ
            Vector3 abovePosition = groundPosition + Vector3.up * 0.25f;
            EBlockType aboveBlockType = WorldManager.Instance.GetBlockType(abovePosition);

            if (aboveBlockType == EBlockType.Air)
            {
                PlaceBlockAtPosition(abovePosition);
            }
            else
            {
                Debug.Log("[PlayerBlockAbility] ���� ��ġ�� �� �����ϴ�. �̹� ���� �����մϴ�.");
            }
        }
    }

    private void DestroyBlockAtPosition(Vector3 worldPosition)
    {
        // �ı��� �� Ÿ�� ����
        _lastDestroyedBlockType = WorldManager.Instance.GetBlockType(worldPosition);

        // �� �ı�
        bool success = WorldManager.Instance.SetBlock(worldPosition, EBlockType.Air);

        if (success)
        {

            if (ObjectPoolManager.Instance)
                ObjectPoolManager.Instance.Get(PoolType.Smoke, worldPosition);

            _isDestroyMode = false;
        }
    }

    private void PlaceBlockAtPosition(Vector3 worldPosition)
    {
        if (_lastDestroyedBlockType == EBlockType.Air)
        {

            Debug.Log("[PlayerBlockAbility] ��ġ�� �� Ÿ���� �����ϴ�.");
            return;
        }

        // ����� �� Ÿ������ ��ġ
        bool success = WorldManager.Instance.SetBlock(worldPosition, _lastDestroyedBlockType);

        if (success)
        {
            if (ObjectPoolManager.Instance)
                ObjectPoolManager.Instance.Get(PoolType.Smoke, worldPosition);
            _isDestroyMode = true;
        }
    }


    public bool IsDestroyMode => _isDestroyMode;
    public EBlockType GetStoredBlockType => _lastDestroyedBlockType;
}
