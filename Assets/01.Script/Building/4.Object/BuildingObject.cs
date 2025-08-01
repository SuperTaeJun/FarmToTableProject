using DG.Tweening;
using UnityEngine;

public class BuildingObject : MonoBehaviour
{
    private IBuildingFunction[] _functions;

    private Vector3 _originalScale;
    [SerializeField] private float scaleUpDuration = 0.5f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    private Collider _collider;

    private void Start()
    {
        if (ObjectPoolManager.Instance)
            ObjectPoolManager.Instance.Get(PoolType.SomkeM, transform.position);

        _originalScale = transform.localScale;

        // 콜리더 가져오기 및 isTrigger 켜기
        _collider = GetComponent<Collider>();
        if (_collider != null)
        {
            _collider.isTrigger = true; // 트리거로 전환 (충돌 무시, 겹친 오브젝트 유도)
        }

        // 작게 시작
        transform.localScale = _originalScale * 0.1f;

        Vector3 midScale = new Vector3(_originalScale.x, transform.localScale.y, _originalScale.z);

        Sequence scaleSequence = DOTween.Sequence();
        scaleSequence.Append(transform.DOScale(midScale, scaleUpDuration * 0.5f).SetEase(scaleEase));
        scaleSequence.Append(transform.DOScale(_originalScale, scaleUpDuration * 0.5f).SetEase(scaleEase));

        // 완료 후 트리거 해제 (콜리전 활성화)
        scaleSequence.OnComplete(() =>
        {
            if (_collider != null)
            {
                StartCoroutine(DisableTriggerAfterDelay(0.1f));
            }
        });
    }

    private System.Collections.IEnumerator DisableTriggerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_collider != null)
        {
            _collider.isTrigger = false;
        }
    }
}
