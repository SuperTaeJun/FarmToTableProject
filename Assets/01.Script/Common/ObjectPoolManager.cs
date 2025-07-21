using System.Collections.Generic;
using System;
using UnityEngine;
public enum PoolType
{
    FootStep,
    Dust,
    Spark,
    Smoke
}

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [Serializable]
    public class PoolConfig
    {
        public PoolType poolType;
        public GameObject prefab;
        public int poolSize = 20;
    }

    [SerializeField] private PoolConfig[] pools;

    private Dictionary<PoolType, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePools()
    {
        poolDictionary = new Dictionary<PoolType, Queue<GameObject>>();

        foreach (var pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // Ǯ ������Ʈ���� ���� �θ� ����
            GameObject poolParent = new GameObject($"Pool_{pool.poolType}");
            poolParent.transform.SetParent(transform);

            // �̸� ������Ʈ�� ����
            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab, poolParent.transform);
                obj.SetActive(false);

                // PoolableObject�� poolType ����
                var poolableObj = obj.GetComponent<PoolableObject>();
                if (poolableObj != null)
                {
                    poolableObj.poolType = pool.poolType;
                }

                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.poolType, objectPool);
        }
    }

    // ������Ʈ ��������
    public GameObject Get(PoolType poolType)
    {
        if (!poolDictionary.ContainsKey(poolType))
        {
            Debug.LogError($"Pool {poolType} doesn't exist!");
            return null;
        }

        GameObject obj = null;

        // ��� ������ ������Ʈ ã�� (null üũ ����)
        while (poolDictionary[poolType].Count > 0)
        {
            obj = poolDictionary[poolType].Dequeue();

            // ��ȿ�� ������Ʈ�� ã������ ���
            if (obj != null)
                break;
        }

        // ��� ������ ������Ʈ�� ������ ���� ����
        if (obj == null)
        {
            var poolConfig = System.Array.Find(pools, p => p.poolType == poolType);
            if (poolConfig?.prefab != null)
            {
                obj = Instantiate(poolConfig.prefab);

                // PoolableObject�� poolType ����
                var poolableObj = obj.GetComponent<PoolableObject>();
                if (poolableObj != null)
                {
                    poolableObj.poolType = poolType;
                }
            }
            else
            {
                Debug.LogError($"Pool config or prefab not found for {poolType}!");
                return null;
            }
        }

        obj.SetActive(true);

        // IPoolable �������̽��� ������ OnSpawn ȣ��
        var poolable = obj.GetComponent<IPoolable>();
        poolable?.OnSpawn();

        return obj;
    }

    // ��ġ �����ؼ� ��������
    public GameObject Get(PoolType poolType, Vector3 position)
    {
        GameObject obj = Get(poolType);
        if (obj != null)
            obj.transform.position = position;
        return obj;
    }

    // ��ġ, ȸ�� �����ؼ� ��������
    public GameObject Get(PoolType poolType, Vector3 position, Quaternion rotation)
    {
        GameObject obj = Get(poolType);
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }
        return obj;
    }

    // ������Ʈ ��ȯ
    public void Return(GameObject obj, PoolType poolType)
    {
        if (obj == null) return;

        // IPoolable �������̽��� ������ OnDespawn ȣ��
        var poolable = obj.GetComponent<IPoolable>();
        poolable?.OnDespawn();

        obj.SetActive(false);

        if (poolDictionary.ContainsKey(poolType))
        {
            poolDictionary[poolType].Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }
}
