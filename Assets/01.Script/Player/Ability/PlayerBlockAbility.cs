using UnityEngine;

public class PlayerBlockAbility : PlayerAbility
{
    // 현재 모드 (파괴 또는 설치)
    private bool _isDestroyMode = true;

    // 마지막으로 파괴한 블럭 타입
    private EBlockType _lastDestroyedBlockType = EBlockType.Air;

    private void Start()
    {
        // 모드 변경 이벤트 구독
        _owner.ModeController.OnModeChanged.AddListener(OnModeChanged);
        // 초기 모드 확인
        OnModeChanged(_owner.ModeController.CurrentMode);
    }

    private void OnModeChanged(EPlayerMode newMode)
    {
        bool isBlockEditMode = (newMode == EPlayerMode.BlockEdit);
        enabled = isBlockEditMode;

        if (isBlockEditMode)
        {
            Debug.Log("[PlayerBlockAbility] 블럭 편집 모드 활성화");
            // 블럭 편집 모드 진입 시 파괴 모드로 초기화
            _isDestroyMode = true;
            _lastDestroyedBlockType = EBlockType.Air;
        }
        else
        {
            Debug.Log("[PlayerBlockAbility] 블럭 편집 모드 비활성화");
        }
    }

    public void OnBlockEditInput()
    {
        if (!enabled) return;

        Vector3 groundPosition = _owner.CurrentSelectedPos;

        if (_isDestroyMode)
        {
            // 파괴 모드: 바닥 블럭 파괴
            Vector3 belowPosition = groundPosition + Vector3.down * 0.25f;
            EBlockType belowBlockType = WorldManager.Instance.GetBlockType(belowPosition);

            if (belowBlockType != EBlockType.Air)
            {
                DestroyBlockAtPosition(belowPosition);
            }
            else
            {
                Debug.Log("[PlayerBlockAbility] 파괴할 블럭이 없습니다.");
            }
        }
        else
        {
            // 설치 모드: 위쪽에 블럭 설치
            Vector3 abovePosition = groundPosition + Vector3.up * 0.25f;
            EBlockType aboveBlockType = WorldManager.Instance.GetBlockType(abovePosition);

            if (aboveBlockType == EBlockType.Air)
            {
                PlaceBlockAtPosition(abovePosition);
            }
            else
            {
                Debug.Log("[PlayerBlockAbility] 블럭을 설치할 수 없습니다. 이미 블럭이 존재합니다.");
            }
        }
    }

    private void DestroyBlockAtPosition(Vector3 worldPosition)
    {
        // 파괴한 블럭 타입 저장
        _lastDestroyedBlockType = WorldManager.Instance.GetBlockType(worldPosition);

        // 블럭 파괴
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

            Debug.Log("[PlayerBlockAbility] 설치할 블럭 타입이 없습니다.");
            return;
        }

        // 저장된 블럭 타입으로 설치
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
