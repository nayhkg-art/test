using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TimerManager : MonoBehaviour
{
    public static event Action OnTimerStarted;

    public TextMeshProUGUI TimerText;
    public float LimitTime = 300;
    private bool isTimerStart;
    public bool IsTimerStart { get => isTimerStart; }
    public float MainTimer { get; private set; } = 0f;
    private float gameOverTimer = 0f;
    public GameOverManager gameOverManager;
    private Coroutine timerCoroutine;

    // GameSelectionManagerから現在のゲームモードを受け取る
    private GameSelectionManager.GameMode currentGameMode;

    void Start()
    {
        isTimerStart = false;
        // GameSelectionManagerのインスタンスから現在のゲームモードを取得
        if (GameSelectionManager.Instance != null)
        {
            currentGameMode = GameSelectionManager.Instance.CurrentGameMode;
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
        // ゲームモードに応じてタイマーの初期値を設定
        if (currentGameMode == GameSelectionManager.GameMode.SinglePlayer)
        {
            MainTimer = 0f;
        }
        else
        {
            MainTimer = LimitTime;
        }
    }

    void Update()
    {
        if (isTimerStart)
        {
            if (currentGameMode == GameSelectionManager.GameMode.SinglePlayer)
            {
                // シングルプレイヤーモード：タイマーを増加させる
                MainTimer += Time.deltaTime;
            }
            else
            {
                // マルチプレイヤーモード：タイマーを減少させる
                MainTimer -= Time.deltaTime;
                MainTimer = Mathf.Max(0, MainTimer);

                if (MainTimer <= 0)
                {
                    gameOverTimer += Time.deltaTime;
                    if (gameOverTimer >= 1)
                    {
                        gameOverManager.GameOver(GameOverReason.Score);
                    }
                }
            }

            int minutes = (int)(MainTimer / 60);
            int seconds = (int)(MainTimer % 60);
            int centiseconds = (int)((MainTimer * 100) % 100);

            TimerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centiseconds);
        }
    }

    public void ResetTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        isTimerStart = false;
        // ゲームモードに応じてタイマーのリセット値を設定
        if (currentGameMode == GameSelectionManager.GameMode.SinglePlayer)
        {
            MainTimer = 0f;
        }
        else
        {
            MainTimer = LimitTime;
        }
        gameOverTimer = 0f;
        int minutes = (int)(MainTimer / 60);
        int seconds = (int)(MainTimer % 60);
        int centiseconds = (int)((MainTimer * 100) % 100);
        TimerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centiseconds);
    }
}