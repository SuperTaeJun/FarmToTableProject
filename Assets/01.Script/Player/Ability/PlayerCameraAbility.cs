using UnityEngine;

public class PlayerCameraAbility : PlayerAbility
{
    [Header("ī�޶� ����")]
    public float mouseSensitivity = 2f;
    public float pitchMin = -30f;
    public float pitchMax = 60f;

    // ī�޶� ȸ����
    private float pitch = 0f;
    private float yaw = 0f;
    protected override void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
        // �ʱ� ī�޶� ���� ����
        yaw = _owner.cameraTarget.eulerAngles.y;
        pitch = _owner.cameraTarget.eulerAngles.x;

        _owner.InputController.OnCameraRotateInput.AddListener(HandleMouseLook);
    }
    private void LateUpdate()
    {
        UpdateCameraTarget();
    }
    void HandleMouseLook(Vector2 input)
    {
        float mouseX = input.x * mouseSensitivity;
        float mouseY = input.y * mouseSensitivity;

        // ī�޶� ȸ���� ������Ʈ
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
    }
    void UpdateCameraTarget()
    {
        // ī�޶� Ÿ�� ��ġ�� �÷��̾� ��ġ�� ������Ʈ
        _owner.cameraTarget.position = transform.position + Vector3.up * 1.7f; // �Ӹ� ����

        // ī�޶� Ÿ�� ȸ�� (���콺 �Է� ����)
        _owner.cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
