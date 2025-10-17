using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance; // シングルトンインスタンス

    [Header("Pool Settings")]
    public GameObject objectToPool; // プールするオブジェクトのPrefab
    public int amountToPool;        // 最初に生成しておく数

    private List<GameObject> pooledObjects; // プールを管理するリスト

    void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 1. オブジェクトの事前生成
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject obj = Instantiate(objectToPool);
            obj.SetActive(false); // 非アクティブにしておく
            pooledObjects.Add(obj);
        }
    }

    /// <summary>
    /// プールから非アクティブなオブジェクトを取得する
    /// </summary>
    /// <returns>プール内の利用可能なGameObject</returns>
    public GameObject GetPooledObject()
    {
        // 2. プール内から利用可能なオブジェクトを探す
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            // 非アクティブなオブジェクトが見つかったら
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i]; // そのオブジェクトを返す
            }
        }

        // 3. 利用可能なオブジェクトがない場合（任意）
        // 新しく生成してプールを拡張することもできる
        Debug.LogWarning("Pool is running out of objects. Expanding pool.");
        GameObject obj = Instantiate(objectToPool);
        obj.SetActive(false);
        pooledObjects.Add(obj);
        return obj;
    }
}