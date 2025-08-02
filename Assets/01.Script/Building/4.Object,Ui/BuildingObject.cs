using DG.Tweening;
using UnityEngine;

public class BuildingObject : MonoBehaviour
{
    private IBuildingFunction[] _functions;
    [SerializeField] private EBuildingType buildingType;

    private Vector3 _originalScale;
    [SerializeField] private float scaleUpDuration = 0.5f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    private Collider _collider;
    
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3.5f;
    private bool _playerInRange = false;
    private Transform _player;
    public Transform Player => _player;

    private void Start()
    {
        if (ObjectPoolManager.Instance)
            ObjectPoolManager.Instance.Get(PoolType.SomkeM, transform.position);

        _originalScale = transform.localScale;

        InitializeFunctions();

        BuildAnimation();
    }

    private void BuildAnimation()
    {
        // 생성될 때 충돌 방지를 위해 isTrigger 설정
        _collider = GetComponent<Collider>();
        if (_collider != null)
        {
            _collider.isTrigger = true; // 트리거로 전환 (충돌 감지 안 하고, 통과만 가능하게 함)
        }

        // 크기 축소 초기화
        transform.localScale = _originalScale * 0.1f;

        Vector3 midScale = new Vector3(_originalScale.x, transform.localScale.y, _originalScale.z);

        Sequence scaleSequence = DOTween.Sequence();
        scaleSequence.Append(transform.DOScale(midScale, scaleUpDuration * 0.5f).SetEase(scaleEase));
        scaleSequence.Append(transform.DOScale(_originalScale, scaleUpDuration * 0.5f).SetEase(scaleEase));

        // 애니메이션 완료 후 트리거 해제 (충돌 활성화)
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

    private void InitializeFunctions()
    {
        switch (buildingType)
        {
            case EBuildingType.Store:
                _functions = new IBuildingFunction[] { new StoreFunction(this) };
                break;
            case EBuildingType.Pashion:
                _functions = new IBuildingFunction[] { new PashionFunction(this) };
                break;
            case EBuildingType.Home:
                _functions = new IBuildingFunction[] { new HomeFunction(this) };
                break;
            default:
                _functions = new IBuildingFunction[0];
                break;
        }
    }

    public void Interact()
    {
        if (_functions != null)
        {
            foreach (var function in _functions)
            {
                function.Execute();
            }
        }
    }

    public void SetBuildingType(EBuildingType type)
    {
        buildingType = type;
        InitializeFunctions();
    }

    private void Update()
    {
        CheckPlayerDistance();
    }

    private void CheckPlayerDistance()
    {
        if (!EnsurePlayerReference()) return;

        float distance = Vector3.Distance(transform.position, _player.position);
        bool wasInRange = _playerInRange;
        _playerInRange = distance <= interactionRange;

        if (_playerInRange && !wasInRange)
        {
            OnPlayerEnterRange();
        }
        else if (!_playerInRange && wasInRange)
        {
            OnPlayerExitRange();
        }
    }

    private void OnPlayerEnterRange()
    {
        if (HasFunction())
        {
            BuildingManager.Instance?.OnBuildingEnterRange.Invoke(this);
        }
    }

    private void OnPlayerExitRange()
    {
        if (HasFunction())
        {
            BuildingManager.Instance?.OnBuildingExitRange.Invoke(this);
        }
    }

    private bool EnsurePlayerReference()
    {
        if (_player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                _player = playerObject.transform;
            }
        }
        return _player != null;
    }

    private bool HasFunction()
    {
        return _functions != null && _functions.Length > 0;
    }

    public bool CanInteract()
    {
        return _playerInRange && HasFunction();
    }

}
