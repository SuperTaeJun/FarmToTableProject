using UnityEngine;

public class PlayerSelectAbility : PlayerAbility
{
    [Header("�׸��� ����")]
    public float cellSize = 1f;
    public LayerMask groundLayer = 1; // Ground ���̾�
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
        currentLineRenderer.positionCount = 5; // �簢�� (������ = ����)
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

        // ���� ������ ��ġ�� �ʷϻ�, �ƴϸ� �⺻ ����
        Color currentColor = isValidPosition ? selectedColor : lineColor;
        currentLineRenderer.startColor = currentColor;
        currentLineRenderer.endColor = currentColor;

        currentLineRenderer.enabled = true;

        float halfSize = cellSize * 0.5f;
        float offset = 0.02f; // �ٴڿ��� ��¦ ����߸���
        Vector3 offsetPos = centerPosition + Vector3.up * offset;

        Vector3[] corners = new Vector3[5];
        corners[0] = offsetPos + new Vector3(-halfSize, 0, -halfSize);
        corners[1] = offsetPos + new Vector3(halfSize, 0, -halfSize);
        corners[2] = offsetPos + new Vector3(halfSize, 0, halfSize);
        corners[3] = offsetPos + new Vector3(-halfSize, 0, halfSize);
        corners[4] = corners[0]; // �簢�� �ϼ�

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