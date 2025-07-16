using System;
using System.Collections.Generic;
using UnityEngine;

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
    private PlayerUiController _uiController;
    public PlayerUiController UiController => _uiController;

    public Vector3 CurrentSelectedPos = Vector3.zero;
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _inputController = GetComponent<PlayerInputController>();
        _uiController = GetComponentInChildren<PlayerUiController>();
    }

    void Start()
    {
        if (PlayerDataHolder.Instance.IsSavedData())
        {
            _characterController.gameObject.SetActive(false);
            gameObject.transform.position = PlayerDataHolder.Instance.SavedPos;
            gameObject.transform.rotation = PlayerDataHolder.Instance.SavedRot;
            _characterController.gameObject.SetActive(true);
        }

        InputController.OnChunkPurchaseInput.AddListener(TryGenerateChunk);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            PlayerDataHolder.Instance.SavedData(gameObject.transform.position, gameObject.transform.rotation);
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

        // ������ �ʱ�ȭ/�ε� -> ó���� ��ٷ� �ʱ�ȭ/�ε��� �ϴ°� �ƴ϶�
        //                    �ʿ��Ҷ��� �ϴ�.. �ڷ� �̷�� ���
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

    private void TryGenerateChunk()
    {

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
            WorldManager.Instance.GenerateAndBuildChunk(targetPos);
        }
        else
        {
            Debug.Log("�ش� ûũ �̹� ����!");
        }

    }
}
