using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.UI.GridLayoutGroup;

public class PlayerLocomotionAbility : PlayerAbility
{
    [Header("이동 설정")]
    private float _currentMoveSpeed;
    private float _rotationSpeed = 10f;
    private const float GRAVITY = -9.81f;

    private Vector3 velocity;
    private bool _isInMarginZone = false; // 클래스 멤버 변수로 추가

    protected override void Awake()
    {
        base.Awake();

    }
    private void Start()
    {
        _currentMoveSpeed = _owner.Data.WalkSpeed;
        _owner.InputController.OnMoveInput.AddListener(HandleMovement);

    }
    private void HandleMovement(Vector2 input)
    {
        Vector3 inputDirection = new Vector3(input.x, 0f, input.y);
        Vector3 moveDirection = Vector3.zero;

        if (inputDirection.magnitude > 0.1f)
        {
            Vector3 cameraForward = _owner.cameraTarget.forward;
            Vector3 cameraRight = _owner.cameraTarget.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();
            moveDirection = (cameraForward * inputDirection.z + cameraRight * inputDirection.x).normalized;

            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                _owner.transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
        }

        if (_owner.Animator != null)
        {
            _owner.Animator.SetFloat("MoveSpeed", inputDirection.magnitude);
        }

        // 중력 처리
        if (_owner.CharacterController.isGrounded)
        {
            if (velocity.y < 0f)
                velocity.y = -2f;
        }
        else
        {
            velocity.y += GRAVITY * Time.deltaTime;
        }

        // 수평 이동 적용
        velocity.x = moveDirection.x * _currentMoveSpeed;
        velocity.z = moveDirection.z * _currentMoveSpeed;
        Vector3 desiredMove = velocity * Time.deltaTime;
        Vector3 targetPos = transform.position + desiredMove;

        // 마진 설정
        float chunkMargin = 2f;
        float chunkSizeX = Chunk.ChunkSize * WorldManager.Instance.dynamicGenerator.blockOffset.x;
        float chunkSizeZ = Chunk.ChunkSize * WorldManager.Instance.dynamicGenerator.blockOffset.z;

        // 현재 위치
        Vector3 currentPos = transform.position;
        int currentChunkX = Mathf.FloorToInt(currentPos.x / chunkSizeX);
        int currentChunkZ = Mathf.FloorToInt(currentPos.z / chunkSizeZ);

        // 현재 청크 내에서의 상대 위치 계산
        float localX = ((currentPos.x % chunkSizeX) + chunkSizeX) % chunkSizeX;
        float localZ = ((currentPos.z % chunkSizeZ) + chunkSizeZ) % chunkSizeZ;

        // 각 방향의 인접 청크가 있는지 확인
        bool hasLeftChunk = WorldManager.Instance.HasChunk(new ChunkPosition(currentChunkX - 1, 0, currentChunkZ));
        bool hasRightChunk = WorldManager.Instance.HasChunk(new ChunkPosition(currentChunkX + 1, 0, currentChunkZ));
        bool hasForwardChunk = WorldManager.Instance.HasChunk(new ChunkPosition(currentChunkX, 0, currentChunkZ + 1));
        bool hasBackChunk = WorldManager.Instance.HasChunk(new ChunkPosition(currentChunkX, 0, currentChunkZ - 1));

        // 월드 끝 경계에서만 마진 체크
        bool isAtWorldEdge = false;

        // 왼쪽 경계 체크 (인접 청크가 없고 마진 내에 있음)
        if (!hasLeftChunk && localX <= chunkMargin)
        {
            isAtWorldEdge = true;
        }
        // 오른쪽 경계 체크
        else if (!hasRightChunk && (chunkSizeX - localX) <= chunkMargin)
        {
            isAtWorldEdge = true;
        }
        // 앞쪽 경계 체크
        else if (!hasForwardChunk && (chunkSizeZ - localZ) <= chunkMargin)
        {
            isAtWorldEdge = true;
        }
        // 뒤쪽 경계 체크
        else if (!hasBackChunk && localZ <= chunkMargin)
        {
            isAtWorldEdge = true;
        }

        // UI 상태 관리 (월드 끝 경계에서만)
        if (isAtWorldEdge && !_isInMarginZone)
        {
            _isInMarginZone = true;
            _owner.UiController.ActiveDialogBox(EPlayerUiType.Chunk);
        }
        else if (!isAtWorldEdge && _isInMarginZone)
        {
            _isInMarginZone = false;
            _owner.UiController.DisActiveDialogBox();
        }

        // 이동 가능 여부 체크 (기존 로직)
        if (inputDirection.magnitude > 0.1f)
        {
            Vector3 marginPos = targetPos + moveDirection.normalized * chunkMargin;
            int targetChunkX = Mathf.FloorToInt(marginPos.x / chunkSizeX);
            int targetChunkZ = Mathf.FloorToInt(marginPos.z / chunkSizeZ);
            var targetChunkPos = new ChunkPosition(targetChunkX, 0, targetChunkZ);

            if (WorldManager.Instance.HasChunk(targetChunkPos))
            {
                // 이동 허용
                _owner.CharacterController.Move(desiredMove);
            }
            else
            {
                // 이동 막기
                Debug.Log("로드되지 않은 청크라 이동을 막았습니다.");
                PopupManager.Instance.Open(EPopupType.UI_ChunkPopup);
                Vector3 verticalMove = new Vector3(0, desiredMove.y, 0);
                _owner.CharacterController.Move(verticalMove);
            }
        }
        else
        {
            // 입력이 없어도 중력은 적용
            Vector3 verticalMove = new Vector3(0, velocity.y * Time.deltaTime, 0);
            _owner.CharacterController.Move(verticalMove);
        }
    }
}
