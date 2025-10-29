using UnityEngine;

public class Bullet : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        // 敵または壁に当たった場合
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Wall"))
        {
            // 自分（弾）を破壊する
            Destroy(gameObject);
        }
    }
}