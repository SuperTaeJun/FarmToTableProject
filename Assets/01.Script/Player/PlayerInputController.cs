using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    public DebugEvent<Vector2> OnMoveInput = new DebugEvent<Vector2>();
    public DebugEvent<Vector2> OnCameraRotateInput = new DebugEvent<Vector2>();
    public DebugEvent OnChunkPurchaseInput = new DebugEvent();
    public DebugEvent<EPlayerMode> OnLeftMouseInput = new DebugEvent<EPlayerMode>();
    public DebugEvent OnWateringInput = new DebugEvent();
    public DebugEvent<EPlayerMode> OnModeChangeInput = new DebugEvent<EPlayerMode>();

    private bool _isCursorLocked = true;
    private bool _playerMoveInputLock = false;
    private EPlayerMode _currentMode;
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetPlayerMoveInputLock(bool able)
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
        HandleLeftMouseInput();
        HandleMouseCursor();
        HandleInteractionInput();
        HandleModeChangeInput(); // ��� ���� �Է� �߰�
    }
    private void HandleModeChangeInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            OnModeChangeInput.Invoke(EPlayerMode.BlockEdit);
            _currentMode = EPlayerMode.BlockEdit;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            OnModeChangeInput.Invoke(EPlayerMode.Farming);
            _currentMode= EPlayerMode.Farming;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            OnModeChangeInput.Invoke(EPlayerMode.Construction);
            _currentMode= EPlayerMode.Construction;
        }
    }
    private void HandleLeftMouseInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            OnLeftMouseInput.Invoke(_currentMode);
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
        }
    }
}
