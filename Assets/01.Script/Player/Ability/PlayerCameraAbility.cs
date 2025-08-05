using UnityEngine;

public class PlayerCameraAbility : PlayerAbility
{
    public float mouseSensitivity = 2f;
    public float pitchMin = -30f;
    public float pitchMax = 60f;

    private float pitch = 0f;
    private float yaw = 0f;
    protected override void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
        yaw = _owner.cameraTarget.eulerAngles.y;
        pitch = _owner.cameraTarget.eulerAngles.x;

        _owner.GetAbility<PlayerInputAbility>()?.OnCameraRotateInput.AddListener(HandleMouseLook);
    }
    private void LateUpdate()
    {
        UpdateCameraTarget();
    }
    void HandleMouseLook(Vector2 input)
    {
        float mouseX = input.x * mouseSensitivity;
        float mouseY = input.y * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
    }
    void UpdateCameraTarget()
    {
        _owner.cameraTarget.position = transform.position + Vector3.up * 1.7f; 
        _owner.cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
