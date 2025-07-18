using UnityEngine;

public class UI_Billboard : MonoBehaviour
{
    private Transform mainCam;

    private void Start()
    {
    }

    private void LateUpdate()
    {
        if(mainCam == null) mainCam = Camera.main.transform;

        transform.LookAt(transform.position + mainCam.rotation * Vector3.forward,
            mainCam.rotation * Vector3.up);
    }
}
