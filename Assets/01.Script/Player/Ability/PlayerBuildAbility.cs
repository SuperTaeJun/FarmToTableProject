using UnityEngine;

public class PlayerBuildAbility : PlayerAbility
{
    [SerializeField] private LayerMask groundLayerMask = 1;
    [SerializeField] private Material _previewMaterial;

    private EBuildingType _selectedType;
    private GameObject _previewInstance;
    private bool _canPlace = false;
    private Quaternion _currentRotation = Quaternion.identity;

    private void Start()
    {
        _selectedType = EBuildingType.None;
        _owner.InputController.OnLeftMouseInput.AddListener(TryBuild);
    }

    private void Update()
    {
        if (_owner.ModeController.CurrentMode != EPlayerMode.Construction || _selectedType == EBuildingType.None)
        {
            HidePreview();
            return;
        }

        if (_previewInstance == null)
            RefreshPreviewInstance();

        UpdatePreview();
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotatePreview();
        }

    }

    public void SetSelectedType(EBuildingType selectedType)
    {
        DestroyPreview();
        _selectedType = selectedType;

        if (_selectedType != EBuildingType.None)
        {
            RefreshPreviewInstance();
        }
    }

    private async void TryBuild(EPlayerMode playerMode)
    {
        if (_owner.ModeController.CurrentMode != EPlayerMode.Construction || _selectedType == EBuildingType.None) return;
        if (!_canPlace) return;

        // 스냅된 위치로 건물 배치
        SO_Building buildingInfo = BuildingManager.Instance.GetBuildingInfo(_selectedType);
        Vector3 snappedPos = BuildingManager.Instance.SnapToGrid(_owner.CurrentSelectedPos, buildingInfo.Size);

        Chunk chunk = WorldManager.Instance.GetChunkAtWorldPosition(snappedPos);
        string chunkID = chunk.Position.ToChunkId();
        bool success = await BuildingManager.Instance.TryPlaceBuilding(_selectedType, chunkID, snappedPos, _currentRotation.eulerAngles);

        if (success)
        {
            Debug.Log("건물 배치 성공!");
        }
        else
        {
            Debug.LogWarning("건물 배치에 실패 했습니다.");
        }
    }

    private void RefreshPreviewInstance()
    {
        DestroyPreview();
        SO_Building buildingInfo = BuildingManager.Instance.GetBuildingInfo(_selectedType);

        if (buildingInfo?.PreviewPrefab != null)
        {
            _previewInstance = Instantiate(buildingInfo.PreviewPrefab);
            _currentRotation = Quaternion.identity;
            _previewInstance.transform.rotation = _currentRotation;
        }
    }

    private void DestroyPreview()
    {
        if (_previewInstance != null)
        {
            Destroy(_previewInstance);
            _previewInstance = null;
        }
    }

    private void HidePreview()
    {
        if (_previewInstance != null)
        {
            _previewInstance.SetActive(false);
        }
    }

    private void UpdatePreview()
    {
        if (_previewInstance == null) return;

        SO_Building buildingInfo = BuildingManager.Instance.GetBuildingInfo(_selectedType);

        // 스냅된 위치 계산
        Vector3 snappedPos = BuildingManager.Instance.SnapToGrid(_owner.CurrentSelectedPos, buildingInfo.Size);

        _previewInstance.transform.position = snappedPos;
        _previewInstance.transform.rotation = _currentRotation;
        _previewInstance.SetActive(true);

        // 배치 가능 여부 확인 - 스냅된 위치 사용
        Chunk chunk = WorldManager.Instance.GetChunkAtWorldPosition(snappedPos);
        string chunkID = chunk.Position.ToChunkId();
        _canPlace = BuildingManager.Instance.CanPlaceBuilding(chunkID, snappedPos, buildingInfo.Size);

        // 머티리얼 색상 변경
        if (_previewMaterial != null)
        {
            Color previewColor = _canPlace ? Color.green : Color.red;
            previewColor.a = 0.7f;
            _previewMaterial.color = previewColor;
        }
    }

    // 회전 기능 (R키 등으로 호출)
    public void RotatePreview()
    {
        _currentRotation *= Quaternion.Euler(0, 90f, 0);
        if (_previewInstance != null)
        {
            _previewInstance.transform.rotation = _currentRotation;
        }
    }

    private void OnDestroy()
    {
        DestroyPreview();
    }

}