using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform cameraTarget; // ī�޶� ����ٴ� �� ������Ʈ
    [SerializeField]private SO_PlayerData _data;
    public SO_PlayerData Data => _data;


    private CharacterController _controller;
    public CharacterController Controller => _controller;
    private Animator _animator;
    public Animator Animator => _animator;
    private PlayerEffectController _effectController;
    public PlayerEffectController EffectController => _effectController;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _effectController = GetComponent<PlayerEffectController>();
    }

    void Start()
    {
        // ���콺 Ŀ�� ����
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if(PlayerDataHolder.Instance.IsSavedData())
        {
            _controller.gameObject.SetActive(false);
            gameObject.transform.position = PlayerDataHolder.Instance.SavedPos;
            gameObject.transform.rotation = PlayerDataHolder.Instance.SavedRot;
            _controller.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            PlayerDataHolder.Instance.SavedData(gameObject.transform.position, gameObject.transform.rotation);
            FadeManager.Instance.FadeToScene("CharacterSelectScene");
        }
    }
}
