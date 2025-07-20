using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.UI.GridLayoutGroup;

public class PlayerLocomotionAbility : PlayerAbility
{
    [Header("�̵� ����")]
    private float _currentMoveSpeed;
    private float _rotationSpeed = 10f;
    private const float GRAVITY = -9.81f;

    private Vector3 velocity;
    private bool _isInMarginZone = false; // Ŭ���� ��� ������ �߰�

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

        // �߷� ó��
        if (_owner.CharacterController.isGrounded)
        {
            if (velocity.y < 0f)
                velocity.y = -2f;
        }
        else
        {
            velocity.y += GRAVITY * Time.deltaTime;
        }

        // ���� �̵� ����
        velocity.x = moveDirection.x * _currentMoveSpeed;
        velocity.z = moveDirection.z * _currentMoveSpeed;
        Vector3 desiredMove = velocity * Time.deltaTime;
        Vector3 targetPos = transform.position + desiredMove;

        // ���� ����
        float chunkMargin = 2f;
        float chunkSizeX = Chunk.ChunkSize * WorldManager.Instance.dynamicGenerator.blockOffset.x;
        float chunkSizeZ = Chunk.ChunkSize * WorldManager.Instance.dynamicGenerator.blockOffset.z;

        // ���� ��ġ
        Vector3 currentPos = transform.position;
        int currentChunkX = Mathf.FloorToInt(currentPos.x / chunkSizeX);
        int currentChunkZ = Mathf.FloorToInt(currentPos.z / chunkSizeZ);

        // ���� ûũ �������� ��� ��ġ ���
        float localX = ((currentPos.x % chunkSizeX) + chunkSizeX) % chunkSizeX;
        float localZ = ((currentPos.z % chunkSizeZ) + chunkSizeZ) % chunkSizeZ;

        // �� ������ ���� ûũ�� �ִ��� Ȯ��
        bool hasLeftChunk = WorldManager.Instance.HasChunk(new ChunkPosition(currentChunkX - 1, 0, currentChunkZ));
        bool hasRightChunk = WorldManager.Instance.HasChunk(new ChunkPosition(currentChunkX + 1, 0, currentChunkZ));
        bool hasForwardChunk = WorldManager.Instance.HasChunk(new ChunkPosition(currentChunkX, 0, currentChunkZ + 1));
        bool hasBackChunk = WorldManager.Instance.HasChunk(new ChunkPosition(currentChunkX, 0, currentChunkZ - 1));

        // ���� �� ��迡���� ���� üũ
        bool isAtWorldEdge = false;

        // ���� ��� üũ (���� ûũ�� ���� ���� ���� ����)
        if (!hasLeftChunk && localX <= chunkMargin)
        {
            isAtWorldEdge = true;
        }
        // ������ ��� üũ
        else if (!hasRightChunk && (chunkSizeX - localX) <= chunkMargin)
        {
            isAtWorldEdge = true;
        }
        // ���� ��� üũ
        else if (!hasForwardChunk && (chunkSizeZ - localZ) <= chunkMargin)
        {
            isAtWorldEdge = true;
        }
        // ���� ��� üũ
        else if (!hasBackChunk && localZ <= chunkMargin)
        {
            isAtWorldEdge = true;
        }

        // UI ���� ���� (���� �� ��迡����)
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

        // �̵� ���� ���� üũ (���� ����)
        if (inputDirection.magnitude > 0.1f)
        {
            Vector3 marginPos = targetPos + moveDirection.normalized * chunkMargin;
            int targetChunkX = Mathf.FloorToInt(marginPos.x / chunkSizeX);
            int targetChunkZ = Mathf.FloorToInt(marginPos.z / chunkSizeZ);
            var targetChunkPos = new ChunkPosition(targetChunkX, 0, targetChunkZ);

            if (WorldManager.Instance.HasChunk(targetChunkPos))
            {
                // �̵� ���
                _owner.CharacterController.Move(desiredMove);
            }
            else
            {
                // �̵� ����
                Debug.Log("�ε���� ���� ûũ�� �̵��� ���ҽ��ϴ�.");
                PopupManager.Instance.Open(EPopupType.UI_ChunkPopup);
                Vector3 verticalMove = new Vector3(0, desiredMove.y, 0);
                _owner.CharacterController.Move(verticalMove);
            }
        }
        else
        {
            // �Է��� ��� �߷��� ����
            Vector3 verticalMove = new Vector3(0, velocity.y * Time.deltaTime, 0);
            _owner.CharacterController.Move(verticalMove);
        }
    }
}
