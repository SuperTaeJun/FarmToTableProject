using UnityEngine;

public class PlayerSelectAbility : PlayerAbility
{
    [Header("그리드 설정")]
    public float cellSize = 1f;
    public LayerMask groundLayer = 1; // Ground 레이어
    public bool snapToGrid = true;

    [Header("라인 설정")]
    public Material lineMaterial;
    public Color lineColor = Color.white;
    public Color selectedColor = Color.green;
    public float lineWidth = 0.05f;

    [Header("선택 범위")]
    public float maxSelectDistance = 1f; // 캐릭터로부터 최대 선택 거리
    public float forwardDistance = 0.5f; // 캐릭터 앞쪽 거리

    private LineRenderer currentLineRenderer;
    private Vector3 lastGridPosition = Vector3.zero;
    private bool isValidPosition = false;

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

        // LineRenderer 설정
        currentLineRenderer.material = lineMaterial;
        currentLineRenderer.startColor = lineColor;
        currentLineRenderer.endColor = lineColor;
        currentLineRenderer.startWidth = lineWidth;
        currentLineRenderer.endWidth = lineWidth;
        currentLineRenderer.positionCount = 5; // 사각형 (시작점 = 끝점)
        currentLineRenderer.useWorldSpace = true;
        currentLineRenderer.loop = false;

        // 처음에는 비활성화
        currentLineRenderer.enabled = false;
    }

    void UpdateForwardBlockPosition()
    {
        // 캐릭터의 앞쪽 방향 계산
        Vector3 forwardDirection = transform.forward;
        Vector3 startPosition = transform.position + Vector3.up * 5f; // 캐릭터 가슴 높이
        Vector3 forwardPosition = startPosition + forwardDirection * forwardDistance;

        // 앞쪽 지점에서 아래로 레이캐스트
        RaycastHit hit;
        if (Physics.Raycast(forwardPosition, Vector3.down, out hit, 10f, groundLayer))
        {
            Vector3 gridPosition = WorldToGrid(hit.point);

            // 캐릭터로부터의 거리 체크
            float distanceFromPlayer = Vector3.Distance(transform.position, gridPosition);
            if (distanceFromPlayer <= maxSelectDistance)
            {
                if (gridPosition != lastGridPosition)
                {
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

        // 바닥 그리드는 Y축은 그대로 두고 X, Z축만 스냅
        float snappedX = Mathf.Round(worldPosition.x / cellSize) * cellSize;
        float snappedZ = Mathf.Round(worldPosition.z / cellSize) * cellSize;
        return new Vector3(snappedX, worldPosition.y, snappedZ);
    }

    void UpdateGridLines(Vector3 centerPosition)
    {
        if (currentLineRenderer == null) return;

        // 선택 가능한 위치면 초록색, 아니면 기본 색상
        Color currentColor = isValidPosition ? selectedColor : lineColor;
        currentLineRenderer.startColor = currentColor;
        currentLineRenderer.endColor = currentColor;

        currentLineRenderer.enabled = true;

        float halfSize = cellSize * 0.5f;
        float offset = 0.02f; // 바닥에서 살짝 떨어뜨리기
        Vector3 offsetPos = centerPosition + Vector3.up * offset;

        Vector3[] corners = new Vector3[5];
        corners[0] = offsetPos + new Vector3(-halfSize, 0, -halfSize);
        corners[1] = offsetPos + new Vector3(halfSize, 0, -halfSize);
        corners[2] = offsetPos + new Vector3(halfSize, 0, halfSize);
        corners[3] = offsetPos + new Vector3(-halfSize, 0, halfSize);
        corners[4] = corners[0]; // 사각형 완성

        currentLineRenderer.SetPositions(corners);
    }

    void HideGridLines()
    {
        if (currentLineRenderer != null)
            currentLineRenderer.enabled = false;
        lastGridPosition = Vector3.zero;
        isValidPosition = false;
    }

    public bool HasValidSelection => isValidPosition;

}