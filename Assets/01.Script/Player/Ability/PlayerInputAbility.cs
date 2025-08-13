using UnityEngine;

public class PlayerInputAbility : PlayerAbility
{
    public DebugEvent<Vector2> OnMoveInput = new DebugEvent<Vector2>();
    public DebugEvent<Vector2> OnCameraRotateInput = new DebugEvent<Vector2>();
    public DebugEvent OnChunkPurchaseInput = new DebugEvent();
    public DebugEvent<EPlayerMode> OnRightMouseInput = new DebugEvent<EPlayerMode>();
    public DebugEvent OnWateringInput = new DebugEvent();
    public DebugEvent<EPlayerMode> OnModeChangeInput = new DebugEvent<EPlayerMode>();
    public DebugEvent OnBuildingInteractionInput = new DebugEvent();

    private bool _isCursorLocked = true;
    private bool _playerMoveInputLock = false;
    private EPlayerMode _currentMode;
    private bool _isPopupOpen = false;

    protected override void Awake()
    {
        base.Awake();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Start()
    {
        PopupManager.Instance.OnPopupStateChanged += OnPopupStateChanged;
    }
    private void OnDestroy()
    {
        PopupManager.Instance.OnPopupStateChanged -= OnPopupStateChanged;
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

        }
        HandleCameraRotateInput();

        HandleRightMouseInput();
        HandleMouseCursor();
        HandleInteractionInput();
        HandleModeChangeInput();
        HandleOptionPopupInput();

        if(Input.GetKeyDown(KeyCode.F3))
        {
            PopupManager.Instance.Open(EPopupType.UI_Vehicle);
        }
    }

    private void OnPopupStateChanged(bool isOpen)
    {
        _isPopupOpen = isOpen;

        if (isOpen)
        {
            _isCursorLocked = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            _isCursorLocked = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }


    private void HandleOptionPopupInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PopupManager.Instance.Open(EPopupType.UI_OptionPopup);
        }
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
            _currentMode = EPlayerMode.Farming;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            OnModeChangeInput.Invoke(EPlayerMode.Construction);
            _currentMode = EPlayerMode.Construction;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            OnModeChangeInput.Invoke(EPlayerMode.Forage);
            _currentMode = EPlayerMode.Forage;
        }
    }
    private void HandleRightMouseInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            OnRightMouseInput.Invoke(_currentMode);
        }
    }
    private void HandleMouseCursor()
    {
        if (_isPopupOpen) return;

        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            _isCursorLocked = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            _isCursorLocked = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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
    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnBuildingInteractionInput.Invoke();
            OnChunkPurchaseInput.Invoke();
        }
    }
}
