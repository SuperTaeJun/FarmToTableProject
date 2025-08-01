using UnityEngine;

public class ForageObject : MonoBehaviour
{
    public EForageType Type { get; private set; }
    public string ChunkId { get; private set; }

    private void Start()
    {
    }

    private void Update()
    {


    }
    public void Init(Forage forage)
    {
        Type = forage.Type;
        ChunkId = forage.ChunkId;

        transform.position = forage.Position;
        transform.eulerAngles = forage.Rotation;
    }
}
