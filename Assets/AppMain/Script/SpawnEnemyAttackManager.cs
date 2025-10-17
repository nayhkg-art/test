using UnityEngine;
using System.Collections;
//相手側に登場する
public class SpawnEnemyAttackManager : MonoBehaviour
{
    public GameObject attackEnemy;

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
        Instantiate(attackEnemy, enemyPosition, Quaternion.identity); // プレハブを生成
    }
}
