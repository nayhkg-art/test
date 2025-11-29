using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// 成績表示用のクラス定義
[System.Serializable]
public class RankUI
{
    public GameType gameType;
    
    [Header("Score Text")]
    public TextMeshProUGUI scoreText; // 追加：スコア数値を表示するためのText

    [Header("Rank Images")]
    public GameObject rank_S_UI;
    public GameObject rank_A_UI;
    public GameObject rank_B_UI;
    public GameObject rank_C_UI;
    public GameObject rank_D_UI;
    public GameObject rank_E_UI;
    public GameObject rank_F_UI;
}

public class GoGameManager : MonoBehaviour
{
    [Header("Main Buttons")]
    public GameObject startButton1; // ゲーム開始ボタン1 (対戦モード)
    public GameObject startButton2; // ゲーム開始ボタン2 (一人用モード)
    
    [Header("Score UI")]
    [SerializeField] private Button scoreButton;         // 成績を表示するボタン
    [SerializeField] private GameObject scorePanel;      // 成績を表示するパネル
    [SerializeField] private Button closeScoreButton;    // 成績パネルを閉じるボタン
    [SerializeField] private List<RankUI> rankUIs;       // 成績パネル内の各ゲームモードの表示設定
    
    [Header("Network UI")]
    public GameObject NoInternetPanel;

    [Header("Help Panel UI")]
    [SerializeField] private GameObject helpPanel; // 説明を表示するパネル
    [SerializeField] private TextMeshProUGUI descriptionText; // 説明文を表示するTextコンポーネント
    [SerializeField] private Button closeHelpButton; // ヘルプパネルを閉じるボタン

    [SerializeField] private GameObject helpOverlayPanel; // ヘルプ内に表示するタップで消えるパネル
    [SerializeField] private Button helpOverlayCloseButton; // そのパネル自体にアタッチしたButtonコンポーネント

    [Header("Option Panel UI")]
    [SerializeField] private GameObject optionPanel; // オプション設定画面のパネル
    [SerializeField] private Button closeOptionButton; // オプション画面を閉じるボタン

    private bool isLoading = false;

    void Start()
    {
        if (startButton1 != null) startButton1.SetActive(true);
        if (startButton2 != null) startButton2.SetActive(true);
        if (NoInternetPanel != null) NoInternetPanel.SetActive(false);

        // 成績パネルの初期化
        if (scorePanel != null)
        {
            scorePanel.SetActive(false);
        }

        // ヘルプパネルの初期化
        if (helpPanel != null)
        {
            helpPanel.SetActive(false);
            if (descriptionText != null)
            {
                descriptionText.text = GameDescriptionManager.GetTitleDescription();
            }
        }

        if (helpOverlayPanel != null)
        {
             helpOverlayPanel.SetActive(true); 
        }

        // オプションパネルの初期化
        if (optionPanel != null)
        {
            optionPanel.SetActive(false);
        }

        // ボタンのリスナー登録
        if (scoreButton != null)
        {
            scoreButton.onClick.AddListener(OnScoreButtonClicked);
        }
        if (closeScoreButton != null)
        {
            closeScoreButton.onClick.AddListener(OnCloseScoreButtonClicked);
        }

        if (closeHelpButton != null)
        {
            closeHelpButton.onClick.AddListener(OnCloseHelpButtonClicked);
        }
        
        if (helpOverlayCloseButton != null)
        {
            helpOverlayCloseButton.onClick.AddListener(OnHelpOverlayClicked);
        }

        if (closeOptionButton != null)
        {
            closeOptionButton.onClick.AddListener(OnCloseOptionButtonClicked);
        }

        Debug.Log("GoGameManager: Start - 初期UIを設定しました。");
    }

    void OnDestroy()
    {
        // 登録したリスナーを解除
        if (scoreButton != null) scoreButton.onClick.RemoveListener(OnScoreButtonClicked);
        if (closeScoreButton != null) closeScoreButton.onClick.RemoveListener(OnCloseScoreButtonClicked);
        if (closeHelpButton != null) closeHelpButton.onClick.RemoveListener(OnCloseHelpButtonClicked);
        if (helpOverlayCloseButton != null) helpOverlayCloseButton.onClick.RemoveListener(OnHelpOverlayClicked);
        if (closeOptionButton != null) closeOptionButton.onClick.RemoveListener(OnCloseOptionButtonClicked);
    }

    // --- 成績表示関連の処理 ---

    public void OnScoreButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSound();
        }

        UpdateScoreDisplay(); // パネルを開く前に成績を更新

        if (scorePanel != null)
        {
            scorePanel.SetActive(true);
        }
    }

    private void OnCloseScoreButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSound();
        }
        if (scorePanel != null)
        {
            scorePanel.SetActive(false);
        }
    }

    private void UpdateScoreDisplay()
    {
        // 設定されたRankUIリストに基づいて成績を表示
        foreach (var rankUI in rankUIs)
        {
            SetAllRankUIInactive(rankUI);
            
            // ランクの読み込みと表示
            RankManager.Rank bestRank = RankManager.LoadBestRank(rankUI.gameType);
            if (bestRank != RankManager.Rank.None)
            {
                SetRankUIActive(rankUI, bestRank, true);
            }

            // 追加部分：スコアの読み込みと表示
            if (rankUI.scoreText != null)
            {
                int bestScore = RankManager.LoadBestScore(rankUI.gameType);
                // 必要に応じて "Score: " などを追加してください
                rankUI.scoreText.text = bestScore.ToString(); 
            }
        }
    }

    private void SetAllRankUIInactive(RankUI rankUI)
    {
        if (rankUI.rank_S_UI != null) rankUI.rank_S_UI.SetActive(false);
        if (rankUI.rank_A_UI != null) rankUI.rank_A_UI.SetActive(false);
        if (rankUI.rank_B_UI != null) rankUI.rank_B_UI.SetActive(false);
        if (rankUI.rank_C_UI != null) rankUI.rank_C_UI.SetActive(false);
        if (rankUI.rank_D_UI != null) rankUI.rank_D_UI.SetActive(false);
        if (rankUI.rank_E_UI != null) rankUI.rank_E_UI.SetActive(false);
        if (rankUI.rank_F_UI != null) rankUI.rank_F_UI.SetActive(false);
    }

    private void SetRankUIActive(RankUI rankUI, RankManager.Rank rank, bool isActive)
    {
        GameObject uiObject = null;
        switch (rank)
        {
            case RankManager.Rank.S: uiObject = rankUI.rank_S_UI; break;
            case RankManager.Rank.A: uiObject = rankUI.rank_A_UI; break;
            case RankManager.Rank.B: uiObject = rankUI.rank_B_UI; break;
            case RankManager.Rank.C: uiObject = rankUI.rank_C_UI; break;
            case RankManager.Rank.D: uiObject = rankUI.rank_D_UI; break;
            case RankManager.Rank.E: uiObject = rankUI.rank_E_UI; break;
            case RankManager.Rank.F: uiObject = rankUI.rank_F_UI; break;
        }

        if (uiObject != null)
        {
            uiObject.SetActive(isActive);
        }
    }

    // --- ゲーム開始関連の処理 ---

    public void OnStartButton1Pressed()
    {
        if (isLoading) return;
        Debug.Log("対戦モード開始ボタンが押されました。");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSound();
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("インターネット接続がありません。NoInternetPanel を表示します。");
            if (startButton1 != null) startButton1.SetActive(false);
            if (startButton2 != null) startButton2.SetActive(false);
            if (NoInternetPanel != null) NoInternetPanel.SetActive(true);
            return;
        }
        else
        {
            if (GameSelectionManager.Instance != null)
            {
                GameSelectionManager.Instance.SetCurrentGameMode(GameSelectionManager.GameMode.Multiplayer);
                Debug.Log("[GoGameManager] GameSelectionManagerにMultiplayerモードを設定しました。");
            }
            else
            {
                Debug.LogError("[GoGameManager] GameSelectionManager.Instance が見つかりません。");
                PlayerPrefs.SetInt("GameMode", (int)GameSelectionManager.GameMode.Multiplayer);
                PlayerPrefs.Save();
            }

            Debug.Log("インターネットに接続されています。ゲーム選択シーンへ遷移を開始します。");
            StartCoroutine(LoadGameSelectionSceneWithSound());
        }
    }

    public void OnStartButton2Pressed()
    {
        if (isLoading) return;
        Debug.Log("一人用モード開始ボタンが押されました。");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSound();
        }

        if (GameSelectionManager.Instance != null)
        {
            GameSelectionManager.Instance.SetCurrentGameMode(GameSelectionManager.GameMode.SinglePlayer);
            Debug.Log("[GoGameManager] GameSelectionManagerにSinglePlayerモードを設定しました。");
        }
        else
        {
            Debug.LogError("[GoGameManager] GameSelectionManager.Instance が見つかりません。");
            PlayerPrefs.SetInt("GameMode", (int)GameSelectionManager.GameMode.SinglePlayer);
            PlayerPrefs.Save();
        }

        Debug.Log("一人用モードを開始します。ゲーム選択シーンへ遷移を開始します。");
        StartCoroutine(LoadGameSelectionSceneWithSound());
    }

    public void OnReturnFromNoInternetPanelClicked()
    {
        Debug.Log("NoInternetPanelの戻るボタンが押されました。タイトルUIをリセットします。");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSound();
        }

        if (startButton1 != null) startButton1.SetActive(true);
        if (startButton2 != null) startButton2.SetActive(true);
        if (NoInternetPanel != null) NoInternetPanel.SetActive(false);
    }

    // --- ヘルプ・オプション関連 ---

    public void OnHelpButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSound();
        }
        if (helpPanel != null)
        {
            helpPanel.SetActive(true);
            if (helpOverlayPanel != null)
            {
                helpOverlayPanel.SetActive(true);
            }
        }
    }

    private void OnCloseHelpButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSound();
        }
        if (helpPanel != null)
        {
            helpPanel.SetActive(false);
        }
    }

    private void OnHelpOverlayClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSound();
        }
        if (helpOverlayPanel != null)
        {
            helpOverlayPanel.SetActive(false);
        }
    }

    public void OnOptionButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSound();
        }
        if (optionPanel != null)
        {
            optionPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Option Panelが割り当てられていません。Inspectorで設定してください。");
        }
    }

    private void OnCloseOptionButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSound();
        }
        if (optionPanel != null)
        {
            optionPanel.SetActive(false);
        }
    }

    private IEnumerator LoadGameSelectionSceneWithSound()
    {
        isLoading = true;

        if (AudioManager.Instance != null && AudioManager.Instance.clickSound != null)
        {
            yield return new WaitForSeconds(AudioManager.Instance.clickSound.length);
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }

        SceneTransitionManager.LoadSceneWithLoadingScreen("GameSelectionScene");
        isLoading = false;
    }
}