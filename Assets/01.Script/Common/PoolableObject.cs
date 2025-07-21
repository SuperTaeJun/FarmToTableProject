using UnityEngine;

public class PoolableObject : MonoBehaviour, IPoolable
{
    [SerializeField] public PoolType poolType;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private float customDuration = 0f; // 커스텀 지속시간 (0이면 파티클 시간 사용)

    private void Awake()
    {
        if (particles == null)
            particles = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        // 파티클이 자동으로 재생되도록
        if (particles != null)
        {
            particles.Play();

            // 지속시간 계산
            float duration;
            if (customDuration > 0)
            {
                duration = customDuration;
            }
            else
            {
                var main = particles.main;
                duration = main.duration;

                // startLifetime이 constant가 아닐 수 있으므로 안전하게 처리
                if (main.startLifetime.mode == ParticleSystemCurveMode.Constant)
                {
                    duration += main.startLifetime.constant;
                }
                else
                {
                    duration += main.startLifetime.constantMax;
                }
            }

            // 최소 지속시간 보장
            duration = Mathf.Max(duration, 0.1f);

            Invoke(nameof(ReturnToPool), duration);
        }
        else
        {
            // 파티클이 없으면 기본 시간 후 반환
            Invoke(nameof(ReturnToPool), customDuration > 0 ? customDuration : 1f);
        }
    }

    public void OnSpawn()
    {
        // 파티클은 OnEnable에서 자동 재생되므로 여기서는 추가 로직만
    }

    public void OnDespawn()
    {
        // 파티클 정지
        if (particles != null)
            particles.Stop();

        CancelInvoke();
    }

    private void ReturnToPool()
    {
        ObjectPoolManager.Instance.Return(gameObject, poolType);
    }
}
