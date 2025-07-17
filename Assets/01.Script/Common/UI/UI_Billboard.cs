using UnityEngine;

public class UI_Billboard : MonoBehaviour
{
    private Transform mainCam;

    private void Start()
    {
        mainCam = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + mainCam.rotation * Vector3.forward,
            mainCam.rotation * Vector3.up);
    }
}
