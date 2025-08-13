using DG.Tweening;
using UnityEngine;

public class ForageObject : MonoBehaviour
{
    public EForageType Type { get; private set; }
    public string ChunkId { get; private set; }


    [SerializeField] private float jumpPower = 3f;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float rotationAngle = 720f;
    [SerializeField] private float fadeDuration = 0.8f;

    private Renderer[] renderers;
    private Material[] materials;
    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        materials = new Material[renderers.Length];
    }
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
    public void RemoveWithAnim()
    {
        PlayDisappear();
    }

    public void PlayDisappear()
    {
        Sequence seq = DOTween.Sequence();

        // 1) 높이 점프
        seq.Append(transform
            .DOJump(transform.position, jumpPower, 1, duration)
            .SetEase(Ease.OutQuad));

        // 2) 점프 중 회전 + 점점 작아짐
        seq.Join(transform
            .DORotate(new Vector3(0, rotationAngle, rotationAngle), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutSine));
        seq.Join(transform
            .DOScale(Vector3.zero, duration)
            .SetEase(Ease.InBack));

        // 3) 페이드 아웃 (LOD 전부)
        foreach (var mat in materials)
        {
            seq.Join(mat.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));
        }

        // 4) 끝나면 오브젝트 삭제
        seq.OnComplete(() => Destroy(gameObject));
    }
}
