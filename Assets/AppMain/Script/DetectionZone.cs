using UnityEngine;

public class DetectionZone : MonoBehaviour
{
    private EnemyController enemyController;

    void Start()
    {
        // 親オブジェクトにいるはずのEnemyControllerを取得する
        enemyController = GetComponentInParent<EnemyController>();
        if (enemyController == null)
        {
            Debug.LogError("親オブジェクトにEnemyControllerが見つかりません！", this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // プレイヤーを発見したら、EnemyControllerにターゲットを教える
            if (enemyController != null)
            {
                enemyController.SetTarget(other.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // プレイヤーが索敵範囲から出たら、EnemyControllerにターゲットを失ったことを教える
            if (enemyController != null)
            {
                enemyController.LoseTarget();
            }
        }
    }
}