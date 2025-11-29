using UnityEngine;

public class ReturnToPoolAfterTime : MonoBehaviour
{
    public float lifeTime = 3f; // この時間後にプールに戻る

    // オブジェクトがアクティブになった時に呼ばれる
    void OnEnable()
    {
        // lifeTime秒後に Deactivate 関数を実行予約する
        Invoke(nameof(Deactivate), lifeTime);
    }

    // オブジェクトが非アクティブになる時に呼ばれる
    void OnDisable()
    {
        // もし途中で手動で消された場合などに備え、予約をキャンセルする
        CancelInvoke(nameof(Deactivate));
    }

    void Deactivate()
    {
        // 自身を非アクティブにしてプールに戻す
        gameObject.SetActive(false);
    }
}