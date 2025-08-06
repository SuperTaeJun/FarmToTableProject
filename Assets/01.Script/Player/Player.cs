using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform cameraTarget;
    [SerializeField] private SO_PlayerData _data;
    public SO_PlayerData Data => _data;


    private CharacterController _characterController;
    public CharacterController CharacterController => _characterController;
    private Animator _animator;
    public Animator Animator => _animator;


    private Dictionary<Type, PlayerAbility> _abilitiesCache = new();

    private PlayerDataController _dataController;
    private PlayerEventController _eventController;
    private PlayerChunkController _chunkController;

    // 컨트롤러들 접근자
    public PlayerDataController DataController => _dataController;
    public PlayerEventController EventController => _eventController;
    public PlayerChunkController ChunkController => _chunkController;



    public Vector3 CurrentSelectedPos = Vector3.zero;
    private BuildingObject _currentInteractableBuilding;
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _dataController = new PlayerDataController(this);
        _eventController = new PlayerEventController(this);
        _chunkController = new PlayerChunkController(this);
    }

    private async void Start()
    {
        // Firebase에서 저장된 플레이어 데이터 로드
        await _dataController.LoadPlayerDataAsync();

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
        await _chunkController.TryGenerateChunkAsync();
    }


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
            _currentInteractableBuilding.FunctionInteract();
        }
    }
}
