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

            // 풀 오브젝트들을 담을 부모 생성
            GameObject poolParent = new GameObject($"Pool_{pool.poolType}");
            poolParent.transform.SetParent(transform);

            // 미리 오브젝트들 생성
            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab, poolParent.transform);
                obj.SetActive(false);

                // PoolableObject에 poolType 설정
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

    // 오브젝트 가져오기
    public GameObject Get(PoolType poolType)
    {
        if (!poolDictionary.ContainsKey(poolType))
        {
            Debug.LogError($"Pool {poolType} doesn't exist!");
            return null;
        }

        GameObject obj = null;

        // 사용 가능한 오브젝트 찾기 (null 체크 포함)
        while (poolDictionary[poolType].Count > 0)
        {
            obj = poolDictionary[poolType].Dequeue();

            // 유효한 오브젝트를 찾았으면 사용
            if (obj != null)
                break;
        }

        // 사용 가능한 오브젝트가 없으면 새로 생성
        if (obj == null)
        {
            var poolConfig = System.Array.Find(pools, p => p.poolType == poolType);
            if (poolConfig?.prefab != null)
            {
                obj = Instantiate(poolConfig.prefab);

                // PoolableObject에 poolType 설정
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

        // IPoolable 인터페이스가 있으면 OnSpawn 호출
        var poolable = obj.GetComponent<IPoolable>();
        poolable?.OnSpawn();

        return obj;
    }

    // 위치 지정해서 가져오기
    public GameObject Get(PoolType poolType, Vector3 position)
    {
        GameObject obj = Get(poolType);
        if (obj != null)
            obj.transform.position = position;
        return obj;
    }

    // 위치, 회전 지정해서 가져오기
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

    // 오브젝트 반환
    public void Return(GameObject obj, PoolType poolType)
    {
        if (obj == null) return;

        // IPoolable 인터페이스가 있으면 OnDespawn 호출
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
