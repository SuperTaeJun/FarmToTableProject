using UnityEngine;

public class FragmentShrinkAndReturn : MonoBehaviour
{
    public float shrinkDuration = 10f;
    public float shrinkDelay = 6f; //작아지기 전 대기 시간

    private float elapsed;
    private bool shrinking = false;
    private bool shrinkStarted = false;

    private Transform[] children;
    private Vector3[] originalPositions;
    private Quaternion[] originalRotations;
    private Vector3[] originalScales;

    [SerializeField] private PoolType _type;

    public void StartShrink()
    {
        children = GetComponentsInChildren<Transform>(includeInactive: true);

        int count = children.Length;
        originalPositions = new Vector3[count];
        originalRotations = new Quaternion[count];
        originalScales = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            Transform t = children[i];
            originalPositions[i] = t.localPosition;
            originalRotations[i] = t.localRotation;
            originalScales[i] = t.localScale;
        }

        elapsed = 0f;
        shrinking = true;
        shrinkStarted = false; // 아직 축소 시작 안 함
    }

    private void Update()
    {
        if (!shrinking) return;

        elapsed += Time.deltaTime;

        // 아직 축소 시작 전 → 대기 시간 경과하면 축소 시작
        if (!shrinkStarted)
        {
            if (elapsed >= shrinkDelay)
            {
                elapsed = 0f;
                shrinkStarted = true;
            }
            return;
        }

        // 축소 중
        float t = Mathf.Clamp01(elapsed / shrinkDuration);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == transform) continue;
            children[i].localScale = Vector3.Lerp(originalScales[i], Vector3.zero, t);
        }

        if (t >= 1f)
        {
            shrinking = false;

            // 초기화
            for (int i = 0; i < children.Length; i++)
            {
                children[i].localPosition = originalPositions[i];
                children[i].localRotation = originalRotations[i];
                children[i].localScale = originalScales[i];
            }

            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
            transform.position = Vector3.zero;

            ObjectPoolManager.Instance.Return(gameObject, _type);
        }
    }

    private void OnEnable()
    {
        shrinking = false;
        shrinkStarted = false;
    }
}
