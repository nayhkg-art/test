using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // ★Actionを使うために追加

public class TimerManager : MonoBehaviour
{
    // ★追加: タイマー開始を通知するための静的イベント
    public static event Action OnTimerStarted;

    public TextMeshProUGUI TimerText;
    public float LimitTime = 300;
    private bool isTimerStart;
    public bool IsTimerStart { get => isTimerStart; }
    private float mainTimer = 0f;
    private float gameOverTimer = 0f;
    public GameOverManager gameOverManager;
    private Coroutine timerCoroutine;

    void Start()
    {
        isTimerStart = false;
    }

    public void StartTimer()
    {
        // イベントを発生させて、購読しているスクリプトに通知する
        // ?.Invoke() とすることで、購読者がいなくてもエラーにならない
        OnTimerStarted?.Invoke();
        Debug.Log("Timer started and OnTimerStarted event invoked.");
        
        timerCoroutine = StartCoroutine(StartTimerAfterDelay(7.0f));
    }

    private IEnumerator StartTimerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isTimerStart = true;
        mainTimer = LimitTime;

        
    }

    void Update()
    {
        if (isTimerStart)
        {
            mainTimer -= Time.deltaTime;
            mainTimer = Mathf.Max(0, mainTimer);

            int minutes = (int)(mainTimer / 60);
            int seconds = (int)(mainTimer % 60);
            int centiseconds = (int)((mainTimer * 100) % 100);

            TimerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centiseconds);

            if (mainTimer <= 0)
            {
                gameOverTimer += Time.deltaTime;
                if (gameOverTimer >= 1)
                {
                    gameOverManager.GameOver(GameOverReason.Score);
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
        mainTimer = LimitTime;
        gameOverTimer = 0f;
        int minutes = (int)(LimitTime / 60);
        int seconds = (int)(LimitTime % 60);
        int centiseconds = (int)((LimitTime * 100) % 100);
        TimerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centiseconds);
    }
}