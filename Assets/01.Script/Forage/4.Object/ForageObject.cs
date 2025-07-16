using UnityEngine;

public class ForageObject : MonoBehaviour
{
    public EForageType Type { get; private set; }
    public string ChunkId { get; private set; }
    private Transform _player;
    public float detectRadius = 2f;
    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (_player == null) return;

        float distance = Vector3.Distance(transform.position, _player.position);
        if (distance < detectRadius)
        {
            // �Ŵ����� �˷��� ����Ʈ���� ����
            ForageManager.Instance.RemoveForage(this);
        }
    }
    public void Init(Forage forage)
    {
        Type = forage.Type;
        ChunkId = forage.ChunkId;

        transform.position = forage.Position;
        transform.eulerAngles = forage.Rotation;
    }
}
