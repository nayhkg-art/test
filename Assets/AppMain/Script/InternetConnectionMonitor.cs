using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class InternetConnectionMonitor : MonoBehaviour
{
    public event Action OnConnectionTimeout; //接続タイムアウトを通知するイベント

    [Header("UI設定")]
    [SerializeField] private TextMeshProUGUI internetWarningText; // 警告文を表示するTextMeshProUGUI
    [SerializeField] private GameObject warningBackground; // 背景画像用のImageを追加

    private string warningMessage = "Internet connection is unstable or lost.";

    [Header("インターネットを切る")]
    public bool debug_ForceNoInternet = false;

    [Header("監視設定")]
    private float showWarningThreshold = 4.0f; // 警告を表示するまでの切断継続時間（秒）

    private bool isConnectionLost = false;      // 現在インターネット接続が失われているか
    private float timeDisconnected = 0f;         // 接続が失われてからの経過時間
    private bool isWarningCurrentlyShowing = false; // 現在警告が表示されているか

    void Start()
    {
        // ★★★ 修正ポイント ★★★
        // GameModeManager.Instance.CurrentMode から GameSelectionManager.Instance.CurrentGameMode に変更
        // GameSelectionManagerがシングルトンになったため、Instanceを介してアクセスします。
        if (GameSelectionManager.Instance != null && GameSelectionManager.Instance.CurrentGameMode == GameSelectionManager.GameMode.SinglePlayer)
        {
            Debug.Log("InternetConnectionMonitor: 一人用モードのため、インターネット接続の監視をスキップします。");
            this.enabled = false; // このコンポーネントを無効化
            return;
        }
        else if (GameSelectionManager.Instance == null)
        {
            // GameSelectionManagerがまだ初期化されていない場合のフォールバック（稀なケース）
            // PlayerPrefsから直接読み込むロジックを残す
            int gameModeInt = PlayerPrefs.GetInt("GameMode", (int)GameSelectionManager.GameMode.None);
            GameSelectionManager.GameMode currentSelectedMode = (GameSelectionManager.GameMode)gameModeInt;
            if (currentSelectedMode == GameSelectionManager.GameMode.SinglePlayer)
            {
                Debug.LogWarning("InternetConnectionMonitor: GameSelectionManager.Instance が null のため、PlayerPrefsからモードを読み込みました (一人用)。監視をスキップします。");
                this.enabled = false;
                return;
            }
        }


        if (internetWarningText == null)
        {
            Debug.LogError("[InternetConnectionMonitor] 警告テキスト用のTextMeshProUGUIがインスペクターで設定されていません!このスクリプトは無効になります。");
            enabled = false;
            return;
        }

        // 初期状態では警告を非表示
        if (warningBackground != null)
        {
            warningBackground.SetActive(false);
        }
        isWarningCurrentlyShowing = false;
    }

    void Update()
    {
        NetworkReachability currentReachability;

        if (debug_ForceNoInternet)
        {
            currentReachability = NetworkReachability.NotReachable;
        }
        else
        {
            currentReachability = Application.internetReachability;
        }

        if (currentReachability == NetworkReachability.NotReachable)
        {
            if (!isConnectionLost)
            {
                isConnectionLost = true;
                Debug.LogWarning("[InternetConnectionMonitor] インターネット接続が失われました。監視を開始します。");
                timeDisconnected = 0f;
            }

            timeDisconnected += Time.deltaTime;

            if (timeDisconnected >= showWarningThreshold && !isWarningCurrentlyShowing)
            {
                OnConnectionTimeout?.Invoke();
                ShowWarning();
            }
        }
        else
        {
            if (isConnectionLost)
            {
                isConnectionLost = false;
                Debug.Log("[InternetConnectionMonitor] インターネット接続が回復しました。");
                HideWarning();
            }
            timeDisconnected = 0f;
        }
    }

    private void ShowWarning()
    {
        if (internetWarningText != null && !isWarningCurrentlyShowing)
        {
            Debug.LogWarning($"[InternetConnectionMonitor] {showWarningThreshold}秒以上インターネット接続がありません。警告を表示します。");
            internetWarningText.text = warningMessage;
            if (warningBackground != null)
            {
                warningBackground.SetActive(true);
            }
            isWarningCurrentlyShowing = true;
        }
    }

    private void HideWarning()
    {
        if (internetWarningText != null && isWarningCurrentlyShowing)
        {
            if (warningBackground != null)
            {
                warningBackground.SetActive(false);
            }
            isWarningCurrentlyShowing = false;
            Debug.Log("[InternetConnectionMonitor] 警告を非表示にしました。");
        }
    }

    public void ResetMonitor()
    {
        isConnectionLost = false;
        timeDisconnected = 0f;
        HideWarning();
        Debug.Log("[InternetConnectionMonitor] モニターをリセットしました。");
    }
}
