using UnityEngine;

public class PlayerSelectAbility : PlayerAbility
{
    [Header("�׸��� ����")]
    public float cellSize = 1f;
    public LayerMask groundLayer; // Ground ���̾�
    public bool snapToGrid = true;

    [Header("���� ����")]
    public Material lineMaterial;
    public Color lineColor = Color.white;
    public Color selectedColor = Color.green;
    public float lineWidth = 0.05f;

    [Header("���� ����")]
    public float maxSelectDistance = 1f; // ĳ���ͷκ��� �ִ� ���� �Ÿ�
    public float forwardDistance = 0.5f; // ĳ���� ���� �Ÿ�

    private LineRenderer currentLineRenderer;
    private Vector3 lastGridPosition = Vector3.zero;
    private bool isValidPosition = false;
    
    // 동적 그리드 크기 시스템
    private Vector2Int currentGridSize = Vector2Int.one;
    private bool isDynamicSizeMode = false;
    private Vector3 buildingCenterPosition = Vector3.zero;

    void Start()
    {
        CreateLineRenderer();
    }

    void Update()
    {
        UpdateForwardBlockPosition();
    }

    void CreateLineRenderer()
    {
        GameObject lineObject = new GameObject("GroundGridLineRenderer");
        lineObject.transform.SetParent(transform);
        currentLineRenderer = lineObject.AddComponent<LineRenderer>();

        // LineRenderer ����
        currentLineRenderer.material = lineMaterial;
        currentLineRenderer.startColor = lineColor;
        currentLineRenderer.endColor = lineColor;
        currentLineRenderer.startWidth = lineWidth;
        currentLineRenderer.endWidth = lineWidth;
        UpdatePositionCount(); // 동적으로 위치 개수 설정
        currentLineRenderer.useWorldSpace = true;
        currentLineRenderer.loop = false;

        // ó������ ��Ȱ��ȭ
        currentLineRenderer.enabled = false;
    }

    void UpdateForwardBlockPosition()
    {
        // ĳ������ ���� ���� ���
        Vector3 forwardDirection = transform.forward;
        Vector3 startPosition = transform.position + Vector3.up * 5f; // ĳ���� ���� ����
        Vector3 forwardPosition = startPosition + forwardDirection * forwardDistance;

        // ���� �������� �Ʒ��� ����ĳ��Ʈ
        RaycastHit hit;
        if (Physics.Raycast(forwardPosition, Vector3.down, out hit, 10f, groundLayer))
        {
            Vector3 gridPosition = WorldToGrid(hit.point);

            // ĳ���ͷκ����� �Ÿ� üũ
            float distanceFromPlayer = Vector3.Distance(transform.position, gridPosition);
            if (distanceFromPlayer <= maxSelectDistance)
            {
                if (gridPosition != lastGridPosition)
                {
                    // 건설모드가 아닐 때 해당 위치의 건물 크기 체크
                    CheckAndSetBuildingSize(gridPosition);
                    
                    UpdateGridLines(gridPosition);
                    lastGridPosition = gridPosition;
                    _owner.CurrentSelectedPos = gridPosition;
                    isValidPosition = true;
                }
            }
            else
            {
                HideGridLines();
            }
        }
        else
        {
            HideGridLines();
        }
    }

    Vector3 WorldToGrid(Vector3 worldPosition)
    {
        if (!snapToGrid) return worldPosition;

        // �ٴ� �׸���� Y���� �״�� �ΰ� X, Z�ุ ����
        float snappedX = Mathf.Round(worldPosition.x / cellSize) * cellSize;
        float snappedZ = Mathf.Round(worldPosition.z / cellSize) * cellSize;
        return new Vector3(snappedX, worldPosition.y, snappedZ);
    }

    void UpdateGridLines(Vector3 centerPosition)
    {
        if (currentLineRenderer == null) return;

        // 유효 위치에 있을때는 초록색, 아니면 기본 색상
        Color currentColor = isValidPosition ? selectedColor : lineColor;
        currentLineRenderer.startColor = currentColor;
        currentLineRenderer.endColor = currentColor;

        currentLineRenderer.enabled = true;

        if (isDynamicSizeMode)
        {
            DrawMultiCellGrid(centerPosition);
        }
        else
        {
            DrawSingleCellGrid(centerPosition);
        }
    }

    void DrawSingleCellGrid(Vector3 centerPosition)
    {
        float halfSize = cellSize * 0.5f;
        float offset = 0.02f; // 바닥에서 살짝 위로올림
        Vector3 offsetPos = centerPosition + Vector3.up * offset;

        Vector3[] corners = new Vector3[5];
        corners[0] = offsetPos + new Vector3(-halfSize, 0, -halfSize);
        corners[1] = offsetPos + new Vector3(halfSize, 0, -halfSize);
        corners[2] = offsetPos + new Vector3(halfSize, 0, halfSize);
        corners[3] = offsetPos + new Vector3(-halfSize, 0, halfSize);
        corners[4] = corners[0]; // 사각형 완성

        currentLineRenderer.SetPositions(corners);
    }

    void DrawMultiCellGrid(Vector3 centerPosition)
    {
        float offset = 0.02f;
        
        // 건물이 있을 때는 건물의 중심점을 사용, 없을 때는 선택된 위치 사용
        Vector3 gridCenter = (buildingCenterPosition != Vector3.zero) ? buildingCenterPosition : centerPosition;
        Vector3 offsetPos = gridCenter + Vector3.up * offset;
        
        // 그리드 크기에 따른 전체 크기 계산
        float totalWidth = currentGridSize.x * cellSize;
        float totalHeight = currentGridSize.y * cellSize;
        float halfWidth = totalWidth * 0.5f;
        float halfHeight = totalHeight * 0.5f;

        // 외곽 테두리만 그리기 (성능 최적화)
        Vector3[] corners = new Vector3[5];
        corners[0] = offsetPos + new Vector3(-halfWidth, 0, -halfHeight);
        corners[1] = offsetPos + new Vector3(halfWidth, 0, -halfHeight);
        corners[2] = offsetPos + new Vector3(halfWidth, 0, halfHeight);
        corners[3] = offsetPos + new Vector3(-halfWidth, 0, halfHeight);
        corners[4] = corners[0]; // 사각형 완성

        currentLineRenderer.SetPositions(corners);
    }

    void HideGridLines()
    {
        if (currentLineRenderer != null)
            currentLineRenderer.enabled = false;
        lastGridPosition = Vector3.zero;
        isValidPosition = false;
        buildingCenterPosition = Vector3.zero; // 건물 중심점 리셋
    }

    public bool HasValidSelection => isValidPosition;

    // 동적 그리드 크기 제어 메서드들
    public void SetGridSize(Vector2Int size)
    {
        currentGridSize = size;
        isDynamicSizeMode = size != Vector2Int.one;
        UpdatePositionCount();
    }

    public void ResetToSingleCell()
    {
        currentGridSize = Vector2Int.one;
        isDynamicSizeMode = false;
        buildingCenterPosition = Vector3.zero; // 건물 중심점 리셋
        UpdatePositionCount();
    }

    public void RotateGridSize()
    {
        if (isDynamicSizeMode)
        {
            // X와 Y 값을 서로 바꿔서 회전 효과
            currentGridSize = new Vector2Int(currentGridSize.y, currentGridSize.x);
            UpdatePositionCount();
        }
    }

    private void UpdatePositionCount()
    {
        if (currentLineRenderer != null)
        {
            currentLineRenderer.positionCount = 5; // 외곽 테두리만 그리므로 항상 5개
        }
    }

    // 현재 그리드 크기 정보
    public Vector2Int CurrentGridSize => currentGridSize;
    public bool IsDynamicSizeMode => isDynamicSizeMode;

    // 건물 크기 체크 및 설정
    private void CheckAndSetBuildingSize(Vector3 position)
    {
        // 건설모드가 아닐 때만 실행
        PlayerModeController modeController = _owner.GetComponent<PlayerModeController>();
        if (modeController != null && modeController.CurrentMode == EPlayerMode.Construction)
        {
            return;
        }

        // BuildingManager를 통해 해당 위치의 건물 크기 정보 가져오기
        if (BuildingManager.Instance != null)
        {
            Vector2Int? buildingSize = BuildingManager.Instance.GetBuildingSizeAtPosition(position, cellSize * 0.5f);
            
            if (buildingSize.HasValue)
            {
                // 건물이 있는 경우
                // 건물의 중심점은 position을 그리드에 맞춰서 사용
                buildingCenterPosition = WorldToGrid(position);
                
                // 현재 그리드 크기와 다르면 업데이트
                if (currentGridSize != buildingSize.Value)
                {
                    SetGridSize(buildingSize.Value);
                }
                
                return;
            }
        }
        
        // 건물이 없으면 1x1로 리셋
        if (currentGridSize != Vector2Int.one || buildingCenterPosition != Vector3.zero)
        {
            ResetToSingleCell();
        }
    }

}