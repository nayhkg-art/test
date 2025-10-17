using UnityEngine;

public class ReturnToPoolAfterTime : MonoBehaviour
{
    public float lifeTime = 3f; // この時間後にプールに戻る

    private float timer;

    // オブジェクトがアクティブになった時に呼ばれる
    void OnEnable()
    {
        // タイマーをリセット
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > lifeTime)
        {
            // 自身を非アクティブにしてプールに戻す
            gameObject.SetActive(false);
        }
    }
}