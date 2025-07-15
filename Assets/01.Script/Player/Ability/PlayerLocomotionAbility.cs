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

    protected override void Awake()
    {
        base.Awake();

    }
    private void Start()
    {
        _currentMoveSpeed = _owner.Data.WalkSpeed;
        _owner.InputController.OnMoveInput.AddListener(HandleMovement);

    }
    private void LateUpdate()
    {
        //ClampPositionToWorld();
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

        // targetPos가 어느 청크인지 계산
        float chunkSizeX = Chunk.ChunkSize * WorldManager.Instance.dynamicGenerator.blockOffset.x;
        float chunkSizeZ = Chunk.ChunkSize * WorldManager.Instance.dynamicGenerator.blockOffset.z;

        int targetChunkX = Mathf.FloorToInt(targetPos.x / chunkSizeX);
        int targetChunkZ = Mathf.FloorToInt(targetPos.z / chunkSizeZ);

        var targetChunkPos = new ChunkPosition(targetChunkX, 0, targetChunkZ);

        if (WorldManager.Instance.HasChunk(targetChunkPos))
        {
            // 이동 허용
            _owner.CharacterController.Move(desiredMove);
            _owner.UiController.DisActiveDialogBox();
        }
        else
        {
            // 이동 막기
            Debug.Log("로드되지 않은 청크라 이동을 막았습니다.");
            _owner.UiController.ActiveDialogBox(EPlayerUiType.Chunk);

            PopupManager.Instance.Open(EPopupType.UI_ChunkPopup);

            Vector3 verticalMove = new Vector3(0, desiredMove.y, 0);
            _owner.CharacterController.Move(verticalMove);
        }
    }
    public void ClampPositionToWorld()
    {
        float margin = 2.0f;

        int minChunkX = int.MaxValue;
        int maxChunkX = int.MinValue;
        int minChunkZ = int.MaxValue;
        int maxChunkZ = int.MinValue;

        foreach (var loadedpos in WorldManager.Instance.LoadedChunkPositions)
        {
            minChunkX = Mathf.Min(minChunkX, loadedpos.X);
            maxChunkX = Mathf.Max(maxChunkX, loadedpos.X);
            minChunkZ = Mathf.Min(minChunkZ, loadedpos.Z);
            maxChunkZ = Mathf.Max(maxChunkZ, loadedpos.Z);
        }

        float chunkSizeX = Chunk.ChunkSize * WorldManager.Instance.dynamicGenerator.blockOffset.x;
        float chunkSizeZ = Chunk.ChunkSize * WorldManager.Instance.dynamicGenerator.blockOffset.z;

        float minX = minChunkX * chunkSizeX + margin;
        float maxX = (maxChunkX + 1) * chunkSizeX - margin;

        float minZ = minChunkZ * chunkSizeZ + margin;
        float maxZ = (maxChunkZ + 1) * chunkSizeZ - margin;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

        Vector3 delta = pos - transform.position;

        if (delta.sqrMagnitude > 0.0001f)
        {
            _owner.CharacterController.Move(delta);
        }
    }
}
