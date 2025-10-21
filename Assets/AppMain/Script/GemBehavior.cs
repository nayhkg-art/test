using UnityEngine;

public class GemBehavior : MonoBehaviour
{
    public int scoreValue = 100;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // --- スコア加算や音の再生処理はそのまま ---
            StatusManagerPlayer playerStatus = other.GetComponent<StatusManagerPlayer>();
            if (playerStatus != null)
            {
                ScoreAnimeManager scoreAnimeManager = FindFirstObjectByType<ScoreAnimeManager>();
                if (scoreAnimeManager != null)
                {
                    scoreAnimeManager.AddScore(scoreValue);
                }
                else
                {
                    Debug.LogWarning("ScoreAnimeManagerが見つかりません。スコアが加算されませんでした。");
                }
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGemCollectSound(transform.position);
            }

            // --- ここを修正 ---
            // 宝石を破壊するのではなく、非アクティブにしてプールに戻す
            // Destroy(gameObject); // ← この行を削除
            gameObject.SetActive(false); // ← この行に変更
        }
    }
}