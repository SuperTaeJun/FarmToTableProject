using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class Player : MonoBehaviour
{
    public Transform cameraTarget;
    [SerializeField] private SO_PlayerData _data;
    public SO_PlayerData Data => _data;

    private Dictionary<Type, PlayerAbility> _abilitiesCache = new();

    private CharacterController _characterController;
    public CharacterController CharacterController => _characterController;
    private Animator _animator;
    public Animator Animator => _animator;
    private PlayerInputController _inputController;
    public PlayerInputController InputController => _inputController;
    private PlayerModeController _modeController;
    public PlayerModeController ModeController => _modeController;
    private PlayerVisualController _visualController;
    public PlayerVisualController VisualController => _visualController;

    private PlayerDataController _dataController;
    private PlayerEventController _eventController;

    public Vector3 CurrentSelectedPos = Vector3.zero;
    private BuildingObject _currentInteractableBuilding;
    private void Awake()
    {
        _modeController = GetComponent<PlayerModeController>();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _inputController = GetComponent<PlayerInputController>();
        _visualController = GetComponentInChildren<PlayerVisualController>();
        _dataController = new PlayerDataController(this);
        _eventController = new PlayerEventController(this);

        // PlayerVehicleAbility 컴포넌트 추가 (없으면)
        if (GetComponent<PlayerVehicleAbility>() == null)
        {
            gameObject.AddComponent<PlayerVehicleAbility>();
        }
    }

    private async void Start()
    {
        // Firebase에서 저장된 플레이어 데이터 로드
        await _dataController.LoadPlayerDataAsync();

        // 기존 로컬 저장 데이터도 체크 (호환성)
        if (PlayerDataHolder.Instance.IsSavedData())
        {
            _characterController.gameObject.SetActive(false);
            gameObject.transform.position = PlayerDataHolder.Instance.SavedPos;
            gameObject.transform.rotation = PlayerDataHolder.Instance.SavedRot;
            _characterController.gameObject.SetActive(true);
        }

        // 플레이어 이벤트 구독
        _eventController.Initialize();
    }
    private void OnDestroy()
    {
        // 플레이어 이벤트 해제
        _eventController?.Cleanup();
    }


    void Update()
    {
        _dataController.Update();
    }
    public T GetAbility<T>() where T : PlayerAbility
    {
        var type = typeof(T);

        if (_abilitiesCache.TryGetValue(type, out PlayerAbility ability))
        {
            return ability as T;
        }

        ability = GetComponentInChildren<T>();

        if (ability != null)
        {
            _abilitiesCache[ability.GetType()] = ability;

            return ability as T;
        }

        throw new Exception($"어빌리티 {type.Name}을(를) {gameObject.name}에서 찾을 수 없습니다.");
    }
    public void SetPositionForCharacterController(Vector3 newPos)
    {
        _characterController.gameObject.SetActive(false);
        transform.position = newPos;
        _characterController.gameObject.SetActive(true);
    }

    public async void TryGenerateChunk()
    {
        bool canBuy = await CurrencyManager.Instance.TrySpendCurrency(ECurrencyType.Money, 500);
        if (canBuy == false)
        {
            GetAbility<PlayerNotificationAbility>()?.ActiveDialogBox(EPlayerNotificationType.LackOfMoney);
            return;
        }

        Vector3 pos = transform.position;

        float chunkSizeX = Chunk.ChunkSize * WorldManager.Instance.dynamicGenerator.blockOffset.x;
        float chunkSizeZ = Chunk.ChunkSize * WorldManager.Instance.dynamicGenerator.blockOffset.z;

        int chunkX = Mathf.FloorToInt(pos.x / chunkSizeX);
        int chunkZ = Mathf.FloorToInt(pos.z / chunkSizeZ);

        float chunkOriginX = chunkX * chunkSizeX;
        float chunkOriginZ = chunkZ * chunkSizeZ;

        float localX = pos.x - chunkOriginX;
        float localZ = pos.z - chunkOriginZ;

        float distLeft = localX;
        float distRight = chunkSizeX - localX;
        float distBack = localZ;
        float distForward = chunkSizeZ - localZ;

        float minDist = Mathf.Min(distLeft, distRight, distBack, distForward);
        if (minDist > 3.0f) return;

        int moveX = 0;
        int moveZ = 0;

        if (minDist == distLeft)
            moveX = -1;
        else if (minDist == distRight)
            moveX = +1;
        else if (minDist == distBack)
            moveZ = -1;
        else if (minDist == distForward)
            moveZ = +1;

        if (moveX == 0 && moveZ == 0)
            return;


        int targetChunkX = chunkX + moveX;
        int targetChunkZ = chunkZ + moveZ;

        var targetPos = new ChunkPosition(targetChunkX, 0, targetChunkZ);

        if (!WorldManager.Instance.HasChunk(targetPos))
        {
            FadeManager.Instance.FadeScreenWithEvent(() => WorldManager.Instance.GenerateAndBuildChunk(targetPos));
        }
    }

    // 컨트롤러들 접근자
    public PlayerDataController DataController => _dataController;
    public PlayerEventController EventController => _eventController;

    public void OnBuildingEnterRange(BuildingObject building)
    {
        _currentInteractableBuilding = building;
        GetAbility<PlayerNotificationAbility>()?.ActiveDialogBox(EPlayerNotificationType.BuildingInteraction);
    }

    public void OnBuildingExitRange(BuildingObject building)
    {
        if (_currentInteractableBuilding == building)
        {
            _currentInteractableBuilding = null;
            GetAbility<PlayerNotificationAbility>()?.DisActiveDialogBox();
        }
    }

    public void TryInteractWithBuilding()
    {
        if (_currentInteractableBuilding != null && _currentInteractableBuilding.CanInteract())
        {
            _currentInteractableBuilding.Interact();
        }
    }
}
