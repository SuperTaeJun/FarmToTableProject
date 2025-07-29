using UnityEngine;

public class UI_Billboard : MonoBehaviour
{
    private Transform mainCam;

    [SerializeField] private float rotationSpeed = 20f;

    private void LateUpdate()
    {
        if (mainCam == null)
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            mainCam = cam.transform;
        }

        // 목표 회전 계산 (카메라의 forward 방향과 up 벡터 사용)
        Quaternion targetRotation = Quaternion.LookRotation(
            transform.position + mainCam.rotation * Vector3.forward - transform.position,
            mainCam.rotation * Vector3.up
        );

        // 현재 회전에서 목표 회전으로 부드럽게 보간
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
    }
}
