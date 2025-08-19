using DG.Tweening;
using UnityEngine;

public class ForageObject : MonoBehaviour
{
    public EForageType Type { get; private set; }
    public string ChunkId { get; private set; }

    [SerializeField] private float jumpPower = 3f;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float rotationAngle = 720f;


    [SerializeField] private GameObject _mesh;

    public void Init(Forage forage)
    {
        Type = forage.Type;
        ChunkId = forage.ChunkId;

        transform.position = forage.Position;
        transform.eulerAngles = forage.Rotation;
    }
    public void RemoveWithAnim()
    {
        PlayDisappear();
    }

    public void PlayDisappear()
    {
        if (gameObject.TryGetComponent<FractureExplosion>(out var fractureExplosion))
        {
            _mesh.SetActive(false);
            fractureExplosion.Explode();
            return;
        }
        else
        {

            Sequence seq = DOTween.Sequence();

            seq.Append(transform
                .DOJump(transform.position, jumpPower, 1, duration)
                .SetEase(Ease.OutQuad));

            seq.Join(transform
                .DORotate(new Vector3(0, rotationAngle, rotationAngle), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutSine));
            seq.Join(transform
                .DOScale(Vector3.zero, duration)
                .SetEase(Ease.InBack));

            seq.OnComplete(() => Destroy(gameObject));
        }
    }

}
