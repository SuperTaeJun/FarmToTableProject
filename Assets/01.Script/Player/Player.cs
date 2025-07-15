using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform cameraTarget; // 카메라가 따라다닐 빈 오브젝트
    [SerializeField] private SO_PlayerData _data;
    public SO_PlayerData Data => _data;


    private CharacterController _characterController;
    public CharacterController CharacterController => _characterController;
    private Animator _animator;
    public Animator Animator => _animator;
    private PlayerInputController _inputController;
    public PlayerInputController InputController => _inputController;
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _inputController = GetComponent<PlayerInputController>();
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

//        InputController.OnInteractionInput.AddListener(TryGenerateChunk);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            PlayerDataHolder.Instance.SavedData(gameObject.transform.position, gameObject.transform.rotation);
            FadeManager.Instance.FadeToScene("CharacterSelectScene");
        }
    }

    public void SetPositionForCharacterController(Vector3 newPos)
    {
        _characterController.gameObject.SetActive(false);
        transform.position = newPos;
        _characterController.gameObject.SetActive(true);
    }


}
