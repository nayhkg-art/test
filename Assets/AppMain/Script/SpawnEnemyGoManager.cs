using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//正解を倒した時の出発
public class SpawnEnemyGoManager : MonoBehaviour
{
    public static SpawnEnemyGoManager Instance; // シングルトンとして利用可能にする
    public GameObject EnemyGo;
    // public int BlueEnemyCount = 0;
    void Awake()
    {
        if (Instance == null)
        {
            //他のスクリプトからGoSpawnEnemyManager.Instanceを使って、このクラスにアクセスできるようになります。
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GoEnemySpawn(GameObject killedEnemy)
    {
        Vector3 enemyPosition = killedEnemy.transform.position;
        Quaternion enemyRotation = killedEnemy.transform.rotation;

        // Y座標を調整する
        enemyPosition.y += -0.2f;

        //EnemyGoを生成
        GameObject newEnemyGo = Instantiate(EnemyGo, enemyPosition, enemyRotation);
        // BlueEnemyCount += 1;
        //寿命で消滅
        StartCoroutine(DestroyEnemyGoAfterTime(newEnemyGo, 5f));
    }
    private IEnumerator DestroyEnemyGoAfterTime(GameObject enemyGo, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (enemyGo != null) // EnemyGoがまだ存在している場合のみ削除する
        {
            Destroy(enemyGo);
            // BlueEnemyCount -= 1; // カウントを減らす
        }
    }
}

