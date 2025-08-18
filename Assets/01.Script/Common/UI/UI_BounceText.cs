using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class UI_BounceText : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] float jumpY = 25f;
    [SerializeField] float cycle = 0.4f;       // 한 글자 올라갔다 내려오는 전체 주기
    [SerializeField] float stagger = 0.05f;    // 글자 간 위상차(초)
    [SerializeField]
    AnimationCurve shape = AnimationCurve.EaseInOut(0, 0, 0.5f, 1);

    [SerializeField] private TextMeshProUGUI tmp;

    TMP_TextInfo textInfo;
    Vector3[][] baseVertsPerMesh;
    List<int> visibleCharIdx = new();

    float driver = 0f;     // 0~1 반복
    Tween driverTween;


    void OnEnable()
    {
        RebuildCaches();
        StartDriver();
    }

    void OnDisable()
    {
        driverTween?.Kill();
    }

    /// 텍스트 바뀌면 호출
    public void RebuildCaches()
    {
        driverTween?.Kill();

        tmp.ForceMeshUpdate();
        textInfo = tmp.textInfo;

        // 기준 정점 백업
        int meshCount = textInfo.meshInfo.Length;
        baseVertsPerMesh = new Vector3[meshCount][];
        for (int m = 0; m < meshCount; m++)
        {
            var src = textInfo.meshInfo[m].vertices;
            baseVertsPerMesh[m] = new Vector3[src.Length];
            System.Array.Copy(src, baseVertsPerMesh[m], src.Length);
        }

        // 보이는 문자 목록
        visibleCharIdx.Clear();
        for (int i = 0; i < textInfo.characterCount; i++)
            if (textInfo.characterInfo[i].isVisible)
                visibleCharIdx.Add(i);

        ApplyOffsets(0f); // 초기값
    }

    void StartDriver()
    {
        // 트윈 1개로 0~1 반복
        driver = 0f;
        driverTween = DOTween.To(() => driver, v =>
        {
            driver = v;
            ApplyOffsets(driver);
        }, 1f, cycle).SetEase(Ease.Linear).SetLoops(-1);
    }

    void ApplyOffsets(float t)
    {
        if (textInfo == null) return;

        // 각 보이는 문자에 대해 위상차 적용
        for (int k = 0; k < visibleCharIdx.Count; k++)
        {
            int i = visibleCharIdx[k];
            var ci = textInfo.characterInfo[i];

            int meshIndex = ci.materialReferenceIndex;
            int vertIndex = ci.vertexIndex;
            var dst = textInfo.meshInfo[meshIndex].vertices;

            // 위상차: 문자 인덱스 * stagger -> 0~1로 정규화
            float phase = Mathf.Repeat(t - (k * stagger / cycle), 1f);

            // 0~1~0 왕복 곡선 만들기 (shape 커브를 왕복 형태로 사용)
            float tri = phase <= 0.5f ? shape.Evaluate(phase * 2f)
                                      : shape.Evaluate((1f - phase) * 2f);

            float y = tri * jumpY;
            Vector3 offset = new Vector3(0, y, 0);

            dst[vertIndex + 0] = baseVertsPerMesh[meshIndex][vertIndex + 0] + offset;
            dst[vertIndex + 1] = baseVertsPerMesh[meshIndex][vertIndex + 1] + offset;
            dst[vertIndex + 2] = baseVertsPerMesh[meshIndex][vertIndex + 2] + offset;
            dst[vertIndex + 3] = baseVertsPerMesh[meshIndex][vertIndex + 3] + offset;
        }

        // 경량 업로드 (Vertices만)
        for (int m = 0; m < textInfo.meshInfo.Length; m++)
        {
            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
    }
}
