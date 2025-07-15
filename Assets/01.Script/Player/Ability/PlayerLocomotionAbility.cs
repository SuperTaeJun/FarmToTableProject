using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLocomotionAbility : PlayerAbility
{
    [Header("�̵� ����")]
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
    private void Update()
    {
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

        _owner.CharacterController.Move(velocity * Time.deltaTime);
    }

}
