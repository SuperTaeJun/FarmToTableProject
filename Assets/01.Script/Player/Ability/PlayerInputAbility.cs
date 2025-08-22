using Unity.VisualScripting;
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
        PopupManager.Instance.OnPopupStateChanged.AddListener(OnPopupStateChanged);
        _owner.GetAbility<PlayerModeAbility>()?.OnModeChanged.AddListener(OnModeChanged);
    }
    private void OnDestroy()
    {
        PopupManager.Instance.OnPopupStateChanged.RemoveListener(OnPopupStateChanged);
        _owner.GetAbility<PlayerModeAbility>()?.OnModeChanged.RemoveListener(OnModeChanged);
    }
    public void SetPlayerMoveInputLock(bool able)
    {
        _playerMoveInputLock = able;
    }

    private void OnModeChanged(EPlayerMode newMode)
    {
        _currentMode = newMode;
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
        HandlePopupInput();


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


    private void HandlePopupInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPopupOpen)
            {
                PopupManager.Instance.PopUpClose(() => SetPlayerMoveInputLock(true));
                return;
            }
            else
                PopupManager.Instance.Open(EPopupType.UI_OptionPopup);

            return;
        }

        if (_isPopupOpen) return;

        if (Input.GetKeyDown(KeyCode.F1))
        {
            PopupManager.Instance.Open(EPopupType.UI_SeedSelectPopup);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            PopupManager.Instance.Open(EPopupType.UI_InventoryPopup);
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            PopupManager.Instance.Open(EPopupType.UI_BuildingPopup);
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            PopupManager.Instance.Open(EPopupType.UI_Vehicle);
        }
    }
    private void HandleModeChangeInput()
    {
        if (_currentMode == EPlayerMode.Vehicle) return;

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
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            OnModeChangeInput.Invoke(EPlayerMode.Forage);
            _currentMode = EPlayerMode.Forage;
        }
    }
    private void HandleRightMouseInput()
    {
        if (_currentMode == EPlayerMode.Vehicle) return;

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
        if (_isPopupOpen) return;


        if (Input.GetKeyDown(KeyCode.F))
        {
            OnBuildingInteractionInput.Invoke();
            OnChunkPurchaseInput.Invoke();
        }
    }
}
