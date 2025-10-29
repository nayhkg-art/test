using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnEnemyAttackManager : MonoBehaviour
{
    public static SpawnEnemyAttackManager Instance { get; private set; }
    public GameObject attackEnemy;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        NetworkMessageSender.OnSpawnAttackEnemy += SpawnEnemy;
    }

    void OnDisable()
    {
        NetworkMessageSender.OnSpawnAttackEnemy -= SpawnEnemy;
    }

    void SpawnEnemy(Vector3 enemyPosition)
    {
        StartCoroutine(SpawnEnemyCoroutine(enemyPosition));
    }

    IEnumerator SpawnEnemyCoroutine(Vector3 enemyPosition)
    {
        yield return new WaitForSeconds(2.0f); // 待ってから生成
        enemyPosition.y += 1.0f; // 高さを調整
        GameObject enemy = Instantiate(attackEnemy, enemyPosition, Quaternion.identity); // プレハブを生成
        spawnedEnemies.Add(enemy);
    }

    public void DestroyAllAttackEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                enemy.SetActive(false);
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
        Debug.Log("[SpawnEnemyAttackManager] 全ての攻撃用の敵を破壊しました。");
    }
}
