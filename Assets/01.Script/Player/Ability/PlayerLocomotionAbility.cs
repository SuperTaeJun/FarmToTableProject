using UnityEngine;
using UnityEngine.SceneManagement;

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

        _currentMoveSpeed = _owner.Data.WalkSpeed;
    }
    private void Update()
    {
        HandleMovement();

        if(Input.GetKeyDown(KeyCode.F1))
        {
            FadeManager.Instance.FadeToScene("CharacterSelectScene");
        }
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical);

        Vector3 moveDirection = Vector3.zero;

        if (inputDirection.magnitude > 0.1f)
        {
            Vector3 cameraForward = _owner.cameraTarget.forward;
            Vector3 cameraRight = _owner.cameraTarget.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;

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
        if (_owner.Controller.isGrounded)
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

        _owner.Controller.Move(velocity * Time.deltaTime);
    }

}
