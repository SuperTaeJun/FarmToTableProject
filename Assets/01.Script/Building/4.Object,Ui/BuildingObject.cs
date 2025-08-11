using DG.Tweening;
using UnityEngine;
using System.Collections;
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
    public Transform ExecuteVfxTransform;
    public Transform ExecuteInfoTransform;
    private void Start()
    {
        if (ObjectPoolManager.Instance)
            ObjectPoolManager.Instance.Get(PoolType.SomkeM, transform.position);

        _originalScale = transform.localScale;

        InitializeFunctions();
        BuildAnimation();
    }

    private void Update()
    {
        CheckPlayerDistance();
        FunctionUpdate();
    }
    private void BuildAnimation()
    {
        _collider = GetComponent<Collider>();
        if (_collider != null)
        {
            _collider.isTrigger = true;
        }

        transform.localScale = _originalScale * 0.1f;

        Vector3 midScale = new Vector3(_originalScale.x, transform.localScale.y, _originalScale.z);

        Sequence scaleSequence = DOTween.Sequence();
        scaleSequence.Append(transform.DOScale(midScale, scaleUpDuration * 0.5f).SetEase(scaleEase));
        scaleSequence.Append(transform.DOScale(_originalScale, scaleUpDuration * 0.5f).SetEase(scaleEase));

        scaleSequence.OnComplete(() =>
        {
            if (_collider != null)
            {
                StartCoroutine(DisableTriggerAfterDelay(0.1f));
            }
        });
    }

    private IEnumerator DisableTriggerAfterDelay(float delay)
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
                _functions = new IBuildingFunction[] { new StoreFunction() };
                break;
            case EBuildingType.Pashion:
                _functions = new IBuildingFunction[] { new PashionFunction() };
                break;
            case EBuildingType.Home:
                _functions = new IBuildingFunction[] { new HomeFunction() };
                break;
            case EBuildingType.AutoWatering:
                _functions = new IBuildingFunction[] { new AutoWateringFunction(this) };
                break;
            case EBuildingType.AutoHarvest:
                _functions = new IBuildingFunction[] { new AutoHarvestFunction(this) };
                break;
            case EBuildingType.PigFarm:
                _functions = new IBuildingFunction[] { new PigFarmFunction(this) };
                break;
            case EBuildingType.ChickenFarm:
                _functions = new IBuildingFunction[] { new ChickenFarmFunction(this) };
                break;
            default:
                _functions = new IBuildingFunction[0];
                break;
        }
    }

    public void FunctionInteract()
    {
        if (_functions != null)
        {
            foreach (var function in _functions)
            {
                function.Execute();
            }
        }
    }
    private void FunctionUpdate()
    {
        if (_functions != null)
        {
            foreach (var function in _functions)
            {
                function.Update();
            }
        }
    }
    public void SetBuildingType(EBuildingType type)
    {
        buildingType = type;
        InitializeFunctions();
    }

    private void CheckPlayerDistance()
    {
        if (interactionRange == 0) return;

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
    
    public bool IsFarmReadyToHarvest()
    {
        if (_functions == null) return false;
        
        foreach (var function in _functions)
        {
            if (function is ChickenFarmFunction chickenFarm)
            {
                return chickenFarm.IsReadyToHarvest;
            }
            else if (function is PigFarmFunction pigFarm)
            {
                return pigFarm.IsReadyToHarvest;
            }
        }
        return true; // 농장이 아닌 경우 항상 상호작용 가능
    }
    
    public float GetFarmHoursUntilReady()
    {
        if (_functions == null) return 0f;
        
        foreach (var function in _functions)
        {
            if (function is ChickenFarmFunction chickenFarm)
            {
                return chickenFarm.HoursUntilReady;
            }
            else if (function is PigFarmFunction pigFarm)
            {
                return pigFarm.HoursUntilReady;
            }
        }
        return 0f;
    }

}
