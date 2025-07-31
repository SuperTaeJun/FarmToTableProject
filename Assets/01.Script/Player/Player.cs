using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class Player : MonoBehaviour
{
    public Transform cameraTarget; // ī�޶� ����ٴ� �� ������Ʈ
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

    private PlayerDataRepository _playerRepository;
    private float _saveInterval = 10f; // 10초마다 저장
    private float _saveTimer;

    public Vector3 CurrentSelectedPos = Vector3.zero;
    private void Awake()
    {
        _modeController = GetComponent<PlayerModeController>();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _inputController = GetComponent<PlayerInputController>();
        _visualController = GetComponentInChildren<PlayerVisualController>();
        _playerRepository = new PlayerDataRepository();
        
        // PlayerVehicleAbility 컴포넌트 추가 (없으면)
        if (GetComponent<PlayerVehicleAbility>() == null)
        {
            gameObject.AddComponent<PlayerVehicleAbility>();
        }
    }

    async void Start()
    {
        // Firebase에서 저장된 플레이어 데이터 로드
        await LoadPlayerData();

        // 기존 로컬 저장 데이터도 체크 (호환성)
        if (PlayerDataHolder.Instance.IsSavedData())
        {
            _characterController.gameObject.SetActive(false);
            gameObject.transform.position = PlayerDataHolder.Instance.SavedPos;
            gameObject.transform.rotation = PlayerDataHolder.Instance.SavedRot;
            _characterController.gameObject.SetActive(true);
        }

        InputController.OnChunkPurchaseInput.AddListener(TryGenerateChunk);
        
        // VehicleManager 이벤트 등록
        if (VehicleManager.Instance != null)
        {
            VehicleManager.Instance.OnVehicleDataChanged.AddListener(OnVehicleDataChanged);
        }
    }

    void Update()
    {
        // 자동 저장 타이머
        _saveTimer += Time.deltaTime;
        if (_saveTimer >= _saveInterval)
        {
            _ = SavePlayerData(); // 비동기 저장 (await 하지 않음)
            _saveTimer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            PlayerDataHolder.Instance.SavedData(gameObject.transform.position, gameObject.transform.rotation);
            // Firebase에도 저장
            _ = SavePlayerData();
            FadeManager.Instance.FadeToScene("CharacterSelectScene");
        }
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

        throw new Exception($"�����Ƽ {type.Name}�� {gameObject.name}���� ã�� �� �����ϴ�.");
    }
    public void SetPositionForCharacterController(Vector3 newPos)
    {
        _characterController.gameObject.SetActive(false);
        transform.position = newPos;
        _characterController.gameObject.SetActive(true);
    }

    private async void TryGenerateChunk()
    {
        bool canBuy  = await CurrencyManager.Instance.TrySpendCurrency(ECurrencyType.Money, 500);
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
        if (minDist > 3.0f)
        {
            Debug.Log("���� ������ �־ ûũ�� �������� ����.");
            return;
        }
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
        {
            Debug.Log("���� ���� ����");
            return;
        }

        int targetChunkX = chunkX + moveX;
        int targetChunkZ = chunkZ + moveZ;

        var targetPos = new ChunkPosition(targetChunkX, 0, targetChunkZ);

        if (!WorldManager.Instance.HasChunk(targetPos))
        {
            Debug.Log($"�� ûũ ����: {targetPos.X}, {targetPos.Z}");

            FadeManager.Instance.FadeScreenWithEvent(()=>WorldManager.Instance.GenerateAndBuildChunk(targetPos));
        }
        else
        {
            Debug.Log("�ش� ûũ �̹� ����!");
        }
    }

    public async Task SavePlayerData()
    {
        var currentData = await _playerRepository.LoadPlayerDataAsync();
        if (currentData == null)
        {
            currentData = new PlayerDataDto();
        }

        currentData.SetPosition(transform.position);
        currentData.SetRotation(transform.rotation);
        currentData.LastSaved = System.DateTime.UtcNow;
        
        // 차량 언락 정보도 함께 저장
        if (VehicleManager.Instance != null)
        {
            currentData.UnlockedVehicleTypes = VehicleManager.Instance.GetUnlockedVehicleTypesAsInt();
        }

        await _playerRepository.SavePlayerDataAsync(currentData);
        Debug.Log($"플레이어 데이터 저장됨: {transform.position}");
    }

    public async Task LoadPlayerData()
    {
        var playerData = await _playerRepository.LoadPlayerDataAsync();
        
        if (playerData != null)
        {
            // 위치 정보 로드
            _characterController.gameObject.SetActive(false);
            transform.position = playerData.GetPosition();
            transform.rotation = playerData.GetRotation();
            _characterController.gameObject.SetActive(true);
            Debug.Log($"플레이어 위치 로드됨: {transform.position}");
            
            // 차량 언락 정보를 VehicleManager에 전달
            if (VehicleManager.Instance != null && playerData.UnlockedVehicleTypes != null)
            {
                VehicleManager.Instance.LoadVehicleUnlockData(playerData.UnlockedVehicleTypes);
                Debug.Log($"차량 언락 정보 로드됨: {playerData.UnlockedVehicleTypes.Count}개");
            }
        }
        else
        {
            Debug.Log("저장된 플레이어 데이터가 없습니다.");
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            _ = SavePlayerData();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            _ = SavePlayerData();
        }
    }
    
    private void OnVehicleDataChanged()
    {
        _ = SavePlayerData();
    }
}
