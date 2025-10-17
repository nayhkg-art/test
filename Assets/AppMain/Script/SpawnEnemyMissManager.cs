using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpawnEnemyMissManager : MonoBehaviour
{
    public static SpawnEnemyMissManager Instance; // シングルトンとして利用可能にする
    public GameObject EnemyMiss;
    public int RedEnemyCount = 0;
    void Awake()
    {
        if (Instance == null)
        {
            //他のスクリプトからMissSpawnEnemyManager.Instanceを使って、このクラスにアクセスできるようになります。
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MissEnemySpawn(GameObject killedEnemy)
    {
        Vector3 enemyPosition = killedEnemy.transform.position;
        Quaternion enemyRotation = killedEnemy.transform.rotation;

        //EnemyMissを生成
        GameObject newEnemyMiss = Instantiate(EnemyMiss, enemyPosition, enemyRotation);
        RedEnemyCount += 1;
        // Debug.Log($"誕生: {SpawnEnemyMissManager.Instance.RedEnemyCount}");
        //寿命で消滅
        StartCoroutine(DestroyEnemyMissAfterTime(newEnemyMiss, 4.5f));
    }
    private IEnumerator DestroyEnemyMissAfterTime(GameObject enemyMiss, float delay)
    {
        yield return new WaitForSeconds(delay);

        // EnemyMissがまだ存在している場合のみ削除する
        if (enemyMiss != null)
        {
            Destroy(enemyMiss);
            RedEnemyCount -= 1; // カウントを減らす
            // Debug.Log($"寿命: {SpawnEnemyMissManager.Instance.RedEnemyCount}");
        }
    }
}

