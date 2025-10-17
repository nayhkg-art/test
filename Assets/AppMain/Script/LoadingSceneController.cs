using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingSceneController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text loadingText;
    [SerializeField] private Button goTitleButton; // 「タイトルに戻る」ボタン

    [Header("Loading Settings")]
    [Tooltip("最小ロード時間（秒）。この時間内は必ずロード画面を表示します。")]
    [SerializeField] private float minLoadTime = 2.0f;

    [Tooltip("ロード処理のタイムアウト時間（秒）。")]
    [SerializeField] private float loadTimeout = 30.0f; // 30秒でタイムアウト

    [Tooltip("タイムアウト後の最大リトライ回数。")]
    [SerializeField] private int maxRetries = 2; // 2回までリトライ

    private float _displayProgress = 0f;
    private int _currentRetryCount = 0;
    private Coroutine _loadCoroutine;

    void Start()
    {
        // UIの初期設定
        if (progressBar != null)
        {
            progressBar.value = 0;
        }
        if (loadingText != null)
        {
            loadingText.text = "Loading...";
        }
        if (goTitleButton != null)
        {
            goTitleButton.gameObject.SetActive(true);
            goTitleButton.onClick.AddListener(OnGoTitleButtonClicked);
        }

        // シーンロード処理を開始
        _loadCoroutine = StartCoroutine(LoadTargetSceneWithRetryAsync());
    }

    /// <summary>
    /// タイムアウトとリトライ機能付きのシーンロード処理
    /// </summary>
    private IEnumerator LoadTargetSceneWithRetryAsync()
    {
        // 遷移先シーンが設定されているか確認
        if (string.IsNullOrEmpty(SceneTransitionManager.sceneToLoadAfterLoading))
        {
            Debug.LogError("ローディングシーンの後に読み込むシーンが指定されていません！タイトルに戻ります。");
            UpdateLoadingText("Error: Target scene not set.");
            yield return new WaitForSeconds(2f); // エラー表示時間
            OnGoTitleButtonClicked(); // タイトルに戻る処理を呼び出す
            yield break;
        }

        // UIをリセット
        _displayProgress = 0f;
        UpdateLoadingText("Loading...");

        // 非同期でシーンをロード開始
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneTransitionManager.sceneToLoadAfterLoading);
        operation.allowSceneActivation = false;

        float startTime = Time.time;
        bool isTimedOut = false;

        // メインのロードループ
        while (!operation.isDone)
        {
            // タイムアウトチェック
            if (Time.time - startTime > loadTimeout)
            {
                isTimedOut = true;
                break;
            }
            
            // 進捗を計算してUIに反映
            float actualProgress = Mathf.Clamp01(operation.progress / 0.9f);
            _displayProgress = Mathf.Lerp(_displayProgress, actualProgress, Time.deltaTime * 5f);
            UpdateLoadingUI();
            
            // ロードが90%完了したら、アクティベーション準備
            if (operation.progress >= 0.9f && _displayProgress >= 0.99f)
            {
                UpdateLoadingUI(1.0f); // 100%表示にする

                // 最低ロード時間待機
                float elapsedTime = Time.time - startTime;
                if (elapsedTime < minLoadTime)
                {
                    yield return new WaitForSeconds(minLoadTime - elapsedTime);
                }

                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        // タイムアウトした場合の処理
        if (isTimedOut)
        {
            HandleTimeout();
        }
    }

    /// <summary>
    /// タイムアウト発生時の処理
    /// </summary>
    private void HandleTimeout()
    {
        _currentRetryCount++;
        Debug.LogWarning($"シーンのロードがタイムアウトしました。リトライします... ({_currentRetryCount}/{maxRetries})");

        if (_currentRetryCount <= maxRetries)
        {
            UpdateLoadingText($"Retrying... ({_currentRetryCount}/{maxRetries})");
            _loadCoroutine = StartCoroutine(LoadTargetSceneWithRetryAsync());
        }
        else
        {
            Debug.LogError("リトライ回数の上限に達しました。タイトルシーンに戻ります。");
            UpdateLoadingText("Failed to load. Returning to title...");
            OnGoTitleButtonClicked(); // タイトルに戻る処理を呼び出す
        }
    }

    /// <summary>
    /// タイトルに戻るボタンが押されたときの処理
    /// </summary>
    public void OnGoTitleButtonClicked()
    {
        Debug.Log("OnGoTitleButtonClicked: タイトルシーンへ遷移します。");

        if (goTitleButton != null)
        {
            goTitleButton.interactable = false; // ボタンを無効化して二重押し防止
        }

        // 進行中のロードコルーチンがあれば停止
        if (_loadCoroutine != null)
        {
            StopCoroutine(_loadCoroutine);
            _loadCoroutine = null;
        }
        
        // ★ここを修正します。GoTitleManagerへの依存をなくし、直接タイトルシーンをロード。
        // Time.timeScaleをリセット
        Time.timeScale = 1f;

        // AudioManagerのサウンドを停止 (前のシーンのBGMなどが残らないように)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllSounds();
            Debug.Log("OnGoTitleButtonClicked: AudioManager.StopAllSounds()を呼び出しました。");
        }
        
        // タイトルシーンへロード
        SceneManager.LoadScene("TitleScene");
        Debug.Log("OnGoTitleButtonClicked: 'TitleScene'をロードしました。");
    }
    
    // --- UI更新用ヘルパーメソッド ---
    private void UpdateLoadingUI(float? forceProgress = null)
    {
        float progress = forceProgress ?? _displayProgress;
        if (progressBar != null)
        {
            progressBar.value = progress;
        }
        if (loadingText != null)
        {
            loadingText.text = $"Loading... {(int)(progress * 100)}%";
        }
    }
    
    private void UpdateLoadingText(string message)
    {
         if (loadingText != null)
        {
            loadingText.text = message;
        }
    }
}