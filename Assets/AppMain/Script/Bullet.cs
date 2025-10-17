using UnityEngine;

public class Bullet : MonoBehaviour
{
    // void OnCollisionEnter(Collision collision)
    void OnTriggerEnter(Collider other)
    {
        // 敵に当たった場合
        // if (collision.gameObject.CompareTag("Enemy"))
         if (other.CompareTag("Enemy"))
        {
            // 自分（弾）を破壊する
            Destroy(gameObject);
        }
    }
}
