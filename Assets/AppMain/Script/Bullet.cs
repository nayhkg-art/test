using UnityEngine;

public class Bullet : MonoBehaviour
{
    // インスペクターから時間を調整できるようにしておくと便利です
    [SerializeField] private float lifeTime = 2.0f;

    void Start()
    {
        // 生成されてから lifeTime秒後（2秒後）に、強制的に自分を破壊する予約を入れる
        // これで何にも当たらなくても自動で消えます
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // 敵、壁、または「シールド」に当たった場合
        if (collision.gameObject.CompareTag("Enemy") || 
            collision.gameObject.CompareTag("Wall") || 
            collision.gameObject.CompareTag("Shield") ||
            // ★追加提案: 「Untagged（タグなし）」や「Floor（床）」にも反応させる
            // 床に落ちて転がるのを防ぐため、地面やその他のオブジェクトでも消えるようにすると自然です
            collision.gameObject.CompareTag("Untagged") || 
            collision.gameObject.CompareTag("Ground")) 
        {
            // 自分（弾）を破壊する
            Destroy(gameObject);
        }
    }
}