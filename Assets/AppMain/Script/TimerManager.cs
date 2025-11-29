using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static event Action OnTimerStarted;

    [Header("通常のタイマー表示")]
    public TextMeshProUGUI TimerText;
    
    [Header("最後の5秒を表示するテキスト")]
    public TextMeshProUGUI CountdownText; 

    public float LimitTime = 300;
    private bool isTimerStart;
    public bool IsTimerStart { get => isTimerStart; }
    private float mainTimer = 0f;

    public float CurrentTime { get => mainTimer; } 

    private float gameOverTimer = 0f;
    public GameOverManager gameOverManager;
    private Coroutine timerCoroutine;

    private GameSelectionManager.GameMode currentGameMode;

    // 変数：元のサイズと色を保存
    private float defaultFontSize;
    private Color defaultColor;

    void Start()
    {
        isTimerStart = false;
        if (GameSelectionManager.Instance != null)
        {
            currentGameMode = GameSelectionManager.Instance.CurrentGameMode;
        }

        // 元のフォントサイズと色を保存
        if (TimerText != null)
        {
            defaultFontSize = TimerText.fontSize;
            defaultColor = TimerText.color;

            // --- ▼▼▼ 修正：文字が大きくなっても枠からはみ出して表示されるように設定を強制変更 ▼▼▼ ---
            TimerText.enableWordWrapping = false; // 折り返しを無効化
            TimerText.overflowMode = TextOverflowModes.Overflow; // 枠を無視して表示
            // --- ▲▲▲ 修正完了 ▲▲▲ ---
        }

        if (CountdownText != null)
        {
            CountdownText.gameObject.SetActive(false);
        }
    }

    public void StartTimer()
    {
        OnTimerStarted?.Invoke();
        Debug.Log("Timer started and OnTimerStarted event invoked.");
        
        timerCoroutine = StartCoroutine(StartTimerAfterDelay(7.0f));
    }

    private IEnumerator StartTimerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isTimerStart = true;
        
        if (currentGameMode == GameSelectionManager.GameMode.SinglePlayer)
        {
            mainTimer = 0f;
        }
        else
        {
            mainTimer = LimitTime;
        }
    }

    void Update()
    {
        if (isTimerStart)
        {
            if (currentGameMode == GameSelectionManager.GameMode.SinglePlayer)
            {
                mainTimer += Time.deltaTime;
            }
            else
            {
                mainTimer -= Time.deltaTime;
                mainTimer = Mathf.Max(0, mainTimer);

                if (mainTimer <= 0)
                {
                    gameOverTimer += Time.deltaTime;
                    if (gameOverTimer >= 1)
                    {
                        gameOverManager.GameOver(GameOverReason.Score);
                    }
                }
            }

            // --- 既存タイマーの表示更新と演出 ---
            int minutes = (int)(mainTimer / 60);
            int seconds = (int)(mainTimer % 60);
            int centiseconds = (int)((mainTimer * 100) % 100);
            TimerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centiseconds);

            // マルチプレイ かつ 残り10秒以下の場合
            if (currentGameMode != GameSelectionManager.GameMode.SinglePlayer && mainTimer <= 10f)
            {
                TimerText.color = Color.red;              // 赤くする
                TimerText.fontSize = defaultFontSize * 1.9f; // 1.9倍にする
            }
            else
            {
                // 通常状態に戻す
                TimerText.color = defaultColor;
                TimerText.fontSize = defaultFontSize;
            }

            // --- 中央カウントダウン (5秒以下) ---
            if (currentGameMode != GameSelectionManager.GameMode.SinglePlayer && mainTimer <= 5f)
            {
                if (CountdownText != null)
                {
                    CountdownText.gameObject.SetActive(true);

                    // 0より大きく1以下の時は確実に「1」を表示
                    int displayCount;
                    if (mainTimer > 0 && mainTimer <= 1.0f)
                    {
                        displayCount = 1;
                    }
                    else
                    {
                        // それ以外は切り上げ (例: 1.1秒 -> 2)
                        displayCount = Mathf.CeilToInt(mainTimer);
                    }

                    CountdownText.text = displayCount.ToString();
                }
            }
            else
            {
                if (CountdownText != null)
                {
                    CountdownText.gameObject.SetActive(false);
                }
            }
        }
    }

    public void ResetTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        isTimerStart = false;

        if (currentGameMode == GameSelectionManager.GameMode.SinglePlayer)
        {
            mainTimer = 0f;
        }
        else
        {
            mainTimer = LimitTime;
        }
        gameOverTimer = 0f;
        
        int minutes = (int)(mainTimer / 60);
        int seconds = (int)(mainTimer % 60);
        int centiseconds = (int)((mainTimer * 100) % 100);
        TimerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centiseconds);

        if (TimerText != null)
        {
            TimerText.color = defaultColor;
            TimerText.fontSize = defaultFontSize;
        }

        if (CountdownText != null)
        {
            CountdownText.gameObject.SetActive(false);
        }
    }
}