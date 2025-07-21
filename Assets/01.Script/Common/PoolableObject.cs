using UnityEngine;

public class PoolableObject : MonoBehaviour, IPoolable
{
    [SerializeField] public PoolType poolType;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private float customDuration = 0f; // Ŀ���� ���ӽð� (0�̸� ��ƼŬ �ð� ���)

    private void Awake()
    {
        if (particles == null)
            particles = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        // ��ƼŬ�� �ڵ����� ����ǵ���
        if (particles != null)
        {
            particles.Play();

            // ���ӽð� ���
            float duration;
            if (customDuration > 0)
            {
                duration = customDuration;
            }
            else
            {
                var main = particles.main;
                duration = main.duration;

                // startLifetime�� constant�� �ƴ� �� �����Ƿ� �����ϰ� ó��
                if (main.startLifetime.mode == ParticleSystemCurveMode.Constant)
                {
                    duration += main.startLifetime.constant;
                }
                else
                {
                    duration += main.startLifetime.constantMax;
                }
            }

            // �ּ� ���ӽð� ����
            duration = Mathf.Max(duration, 0.1f);

            Invoke(nameof(ReturnToPool), duration);
        }
        else
        {
            // ��ƼŬ�� ������ �⺻ �ð� �� ��ȯ
            Invoke(nameof(ReturnToPool), customDuration > 0 ? customDuration : 1f);
        }
    }

    public void OnSpawn()
    {
        // ��ƼŬ�� OnEnable���� �ڵ� ����ǹǷ� ���⼭�� �߰� ������
    }

    public void OnDespawn()
    {
        // ��ƼŬ ����
        if (particles != null)
            particles.Stop();

        CancelInvoke();
    }

    private void ReturnToPool()
    {
        ObjectPoolManager.Instance.Return(gameObject, poolType);
    }
}
