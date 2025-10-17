using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreAnimeManager : MonoBehaviour
{
    public int score; // 現在のスコア
    private int targetScore; // 目標スコア
    public float scoreChangeSpeed = 5f; // スコアの増減速度
    public TextMeshProUGUI scoreText; // TMP用のTextオブジェクト
    public int Score { get => score; }

    // --- ▼▼▼ ここから追加 ▼▼▼ ---
    [Header("数字変化音設定")]
    [Tooltip("スコアが変化している時に鳴らす短い効果音（カチッという音など）")]
    public AudioClip scoreTickSound;
    // --- ▲▲▲ ここまで追加 ▲▲▲ ---

    private void Start()
    {
        // 初期スコアを表示
        scoreText.text = $"{score}<size=40%>PTS</size>";
    }

    private void Update()
    {
        // スコアを滑らかに変更
        if (score != targetScore)
        {
            // --- ▼▼▼ ここから変更 ▼▼▼ ---
            // 変化前のスコアを一時的に覚えておく
            int previousScore = score;

            // Lerpを使って目標スコアに近づける
            score = Mathf.RoundToInt(Mathf.Lerp(score, targetScore, Time.deltaTime * scoreChangeSpeed));
            
            // 実際に表示されている数字が変わった瞬間に音を鳴らす
            if (score != previousScore && AudioManager.Instance != null && scoreTickSound != null)
            {
                // AudioManagerのPlayOneShotSFXを呼び出す
                AudioManager.Instance.PlayOneShotSFX(scoreTickSound);
            }
            // --- ▲▲▲ ここまで変更 ▲▲▲ ---

            scoreText.text = $"{score}<size=40%>PTS</size>";
        }
    }

    public void AddScore(int amount)
    {
        targetScore += amount;
    }
}