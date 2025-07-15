using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    public DebugEvent<Vector2> OnMoveInput = new DebugEvent<Vector2>();
    public DebugEvent<Vector2> OnCameraRotateInput = new DebugEvent<Vector2>();
    public DebugEvent OnChunkPurchaseInput = new DebugEvent();
    private bool _isCursorLocked = true;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void Update()
    {
        HandleMoveInput();
        HandleCameraRotateInput();
        HandleMouseCursor();
        HandleChunkPurchaseInput();
    }

    private void HandleMouseCursor()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            ToggleCursorLock();
        }
    }
    private void HandleMoveInput()
    {
        if (!_isCursorLocked)
        {
            OnMoveInput.Invoke(new Vector2(0, 0));
            return; 
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        OnMoveInput.Invoke(new Vector2(horizontal, vertical));
    }
    private void HandleCameraRotateInput()
    {
        if (!_isCursorLocked) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        OnCameraRotateInput.Invoke(new Vector2(mouseX, mouseY));

    }
    private void ToggleCursorLock()
    {
        _isCursorLocked = !_isCursorLocked;

        if (_isCursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Debug.Log($"Cursor Lock Toggled: {_isCursorLocked}");
    }
    private void HandleChunkPurchaseInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            FadeManager.Instance.FadeScreenWithEvent(OnChunkPurchaseInput.Invoke);
            //OnInteractionInput.Invoke();
        }
    }
}
