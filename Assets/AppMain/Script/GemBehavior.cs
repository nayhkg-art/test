using UnityEngine;

public class GemBehavior : MonoBehaviour
{
    public int scoreValue = 100;
    private Rigidbody rb;
    private bool isHovering = false;
    private float originalY;
    public float minHeight = 0.8f;
    public float hoverSpeed = 1.0f;
    public float hoverAmount = 0.2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        isHovering = false;
        if (rb != null)
        {
            rb.useGravity = true;
        }
    }

    void Update()
    {
        if (!isHovering)
        {
            if (transform.position.y <= minHeight)
            {
                isHovering = true;
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                originalY = minHeight;
            }
        }
        else
        {
            float newY = originalY + Mathf.Sin(Time.time * hoverSpeed) * hoverAmount;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StatusManagerPlayer playerStatus = other.GetComponent<StatusManagerPlayer>();
            if (playerStatus != null)
            {
                playerStatus.AddJewels(1);

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