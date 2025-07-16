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
    public float maxSelectDistance = 3f; // 캐릭터로부터 최대 선택 거리

    private LineRenderer currentLineRenderer;
    private Vector3 lastGridPosition = Vector3.zero;
    //private Vector3 currentGroundPosition = Vector3.zero;
    private bool isValidPosition = false;

    void Start()
    {
        CreateLineRenderer();
    }

    void Update()
    {
        UpdateGroundPosition();
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

    void UpdateGroundPosition()
    {
        // 캐릭터 발 아래로 레이캐스트
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        RaycastHit hit;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 5f, groundLayer))
        {
            Vector3 gridPosition = WorldToGrid(hit.point);

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
