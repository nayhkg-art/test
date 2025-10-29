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
    private float mainTimer = 0f;
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
                // シングルプレイヤーモード：タイマーを増加させる
                mainTimer += Time.deltaTime;
            }
            else
            {
                // マルチプレイヤーモード：タイマーを減少させる
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

            int minutes = (int)(mainTimer / 60);
            int seconds = (int)(mainTimer % 60);
            int centiseconds = (int)((mainTimer * 100) % 100);

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
    }
}