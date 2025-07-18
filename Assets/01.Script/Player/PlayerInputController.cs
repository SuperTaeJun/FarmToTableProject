using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    public DebugEvent<Vector2> OnMoveInput = new DebugEvent<Vector2>();
    public DebugEvent<Vector2> OnCameraRotateInput = new DebugEvent<Vector2>();
    public DebugEvent OnChunkPurchaseInput = new DebugEvent();
    public DebugEvent OnFarmingInput = new DebugEvent();
    public DebugEvent OnWateringInput = new DebugEvent();
    private bool _isCursorLocked = true;

    private bool _playerMoveInputLock = false;
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetPlayerMoveInput(bool able)
    {
        _playerMoveInputLock = able;
    }
    private void Update()
    {
        if (!_playerMoveInputLock)
        {
            HandleMoveInput();
            HandleCameraRotateInput();
        }
        HandleMouseCursor();
        HandleInteractionInput();
        HandleFarmingInput();
    }

    private void HandleFarmingInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            OnFarmingInput.Invoke();
        }
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
    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnChunkPurchaseInput.Invoke();
            OnWateringInput.Invoke();
        }
    }
}
