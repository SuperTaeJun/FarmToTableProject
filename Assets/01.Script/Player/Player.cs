using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform cameraTarget; // 카메라가 따라다닐 빈 오브젝트
    [SerializeField]private SO_PlayerData _data;
    public SO_PlayerData Data => _data;
    private CharacterController _controller;
    public CharacterController Controller => _controller;
    private Animator _animator;
    public Animator Animator => _animator;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        // 마우스 커서 설정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
