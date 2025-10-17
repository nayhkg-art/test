using System.Collections;
using UnityEngine;

public class EnemyScaler : MonoBehaviour
{
    private Vector3 initialScale; // 初期スケール
    public Vector3 targetScale = new Vector3(2f, 2f, 2f); // 目標スケール
    public float scaleDuration = 3f; // 拡大にかける時間

    void Start()
    {
        // 初期スケールを保存
        initialScale = transform.localScale;

        // 拡大処理を開始
        StartCoroutine(ScaleOverTime(initialScale, targetScale, scaleDuration));
    }

    IEnumerator ScaleOverTime(Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // スケールを線形補間（Lerp）で変化
            transform.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / duration);

            // 経過時間を増加
            elapsedTime += Time.deltaTime;

            yield return null; // 次のフレームまで待機
        }

        // 最終的なスケールを確定
        transform.localScale = endScale;
    }
}
