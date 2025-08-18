using DG.Tweening;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GachaBoxAnim : MonoBehaviour
{
    [Header("대상 박스(4개)")]
    public Transform[] boxes;

    [Header("이펙트 파라미터")]
    [Tooltip("점프 높이(월드 Y 기준)")]
    public float jumpHeight = 0.4f;
    [Tooltip("한 번 점프하는 데 걸리는 시간")]
    public float jumpDuration = 0.6f;
    [Tooltip("박스마다 시작 지연(스태거)")]
    public float stagger = 0.12f;
    [Tooltip("착지 순간 스케일 압축 강도")]
    public float squashAmount = 0.18f;
    [Tooltip("스쿼시/스트레치가 복원되는 시간")]
    public float squashDuration = 0.12f;

    [Header("랜덤성")]
    [Tooltip("점프 높이의 랜덤 가감 범위")]
    public float randomHeight = 0.08f;
    [Tooltip("점프 시간의 랜덤 가감 범위")]
    public float randomDuration = 0.06f;

    [Header("이즈 타입")]
    public Ease upEase = Ease.OutQuad;
    public Ease downEase = Ease.InQuad;

    // 초기 위치/스케일 기억용
    private Vector3[] basePos;
    private Vector3[] baseScale;

    void Awake()
    {
        if (boxes == null || boxes.Length == 0) return;

        basePos = new Vector3[boxes.Length];
        baseScale = new Vector3[boxes.Length];

        for (int i = 0; i < boxes.Length; i++)
        {
            basePos[i] = boxes[i].position;
            baseScale[i] = boxes[i].localScale;
        }
    }

    void Start()
    {
        if (boxes == null || boxes.Length == 0) return;

        // 각 박스에 개별 시퀀스 적용
        for (int i = 0; i < boxes.Length; i++)
        {
            SetupBounce(i);
        }

        GachaScene.Instance.OnGachaPerformed += () => PauseAll();

    }

    void SetupBounce(int idx)
    {
        var t = boxes[idx];
        t.DOKill();
        t.position = basePos[idx];
        t.localScale = baseScale[idx];

        // 랜덤 가감
        float h = jumpHeight + Random.Range(-randomHeight, randomHeight);
        float dur = Mathf.Max(0.05f, jumpDuration + Random.Range(-randomDuration, randomDuration));

        Sequence seq = DOTween.Sequence();

        // 시작 지연(스태거)
        seq.PrependInterval(stagger * idx);

        // 살짝 스쿼시 (착지 직후 느낌)
        seq.Append(t.DOScale(new Vector3(
                baseScale[idx].x * (1 + squashAmount),
                baseScale[idx].y * (1 - squashAmount),
                baseScale[idx].z),
            squashDuration).SetEase(Ease.OutSine));

        // 위로 이동(업)
        seq.Append(t.DOMoveY(basePos[idx].y + h, dur * 0.5f)
            .SetEase(upEase));

        // 공중에서 살짝 스트레치
        seq.Join(t.DOScale(new Vector3(
                baseScale[idx].x * (1 - squashAmount * 0.7f),
                baseScale[idx].y * (1 + squashAmount * 0.7f),
                baseScale[idx].z),
            dur * 0.5f).SetEase(Ease.OutSine));

        // 아래로 이동(다운)
        seq.Append(t.DOMoveY(basePos[idx].y, dur * 0.5f)
            .SetEase(downEase));

        // 착지 순간 다시 스쿼시 -> 원래 스케일 복원
        seq.Join(t.DOScale(new Vector3(
                baseScale[idx].x * (1 + squashAmount),
                baseScale[idx].y * (1 - squashAmount),
                baseScale[idx].z),
            squashDuration).SetEase(Ease.InSine));
        seq.Append(t.DOScale(baseScale[idx], squashDuration).SetEase(Ease.OutSine));

        // 무한 루프
        seq.SetLoops(-1);
    }

    // 필요 시 정지/재시작용 메서드
    public void PauseAll() => DOTween.Pause(this);
    public void PlayAll() => DOTween.Play(this);
    public void KillAll() => DOTween.Kill(this);
}
