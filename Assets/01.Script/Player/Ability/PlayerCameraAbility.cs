using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class PlayerCameraAbility : PlayerAbility
{
    [Header("카메라 설정")]

    public float mouseSensitivity = 2f;
    public float pitchMin = -30f;
    public float pitchMax = 60f;

    // 카메라 회전값
    private float pitch = 0f;
    private float yaw = 0f;

    void Start()
    {
        // 초기 카메라 각도 설정

        yaw = _owner.cameraTarget.eulerAngles.y;
        pitch = _owner.cameraTarget.eulerAngles.x;
    }
    private void Update()
    {
        HandleMouseLook();

    }
    private void LateUpdate()
    {
        UpdateCameraTarget();
    }
    void HandleMouseLook()
    {
        // 마우스 입력 받기
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 카메라 회전값 업데이트
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
    }
    void UpdateCameraTarget()
    {
        // 카메라 타겟 위치를 플레이어 위치로 업데이트
        _owner.cameraTarget.position = transform.position + Vector3.up * 1.7f; // 머리 높이

        // 카메라 타겟 회전 (마우스 입력 기준)
        _owner.cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
