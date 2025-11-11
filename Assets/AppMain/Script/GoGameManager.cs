using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class GoGameManager : MonoBehaviour
{
    public GameObject startButton1; // ゲーム開始ボタン1 (対戦モード)
    public GameObject startButton2; // ゲーム開始ボタン2 (一人用モード)
    public GameObject NoInternetPanel;

    [Header("Help Panel UI")]
    [SerializeField] private GameObject helpPanel; // 説明を表示するパネル
    [SerializeField] private TextMeshProUGUI descriptionText; // 説明文を表示するTextコンポーネント
    [SerializeField] private Button closeHelpButton; // ヘルプパネルを閉じるボタン

    [Header("Option Panel UI")]
    [SerializeField] private GameObject optionPanel; // オプション設定画面のパネル
    [SerializeField] private Button closeOptionButton; // オプション画面を閉じるボタン

    private bool isLoading = false;

    void Start()
    {
        if (startButton1 != null) startButton1.SetActive(true);
        if (startButton2 != null) startButton2.SetActive(true);
        if (NoInternetPanel != null) NoInternetPanel.SetActive(false);

        // ヘルプパネルの初期化
        if (helpPanel != null)
        {
            helpPanel.SetActive(false);
            if (descriptionText != null)
            {
                descriptionText.text = GameDescriptionManager.GetTitleDescription();
            }
        }

        // オプションパネルの初期化
        if (optionPanel != null)
        {
            optionPanel.SetActive(false);
        }

        // ボタンのリスナー登録
        if (closeHelpButton != null)
        {
            closeHelpButton.onClick.AddListener(OnCloseHelpButtonClicked);
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
        if (closeHelpButton != null)
        {
            closeHelpButton.onClick.RemoveListener(OnCloseHelpButtonClicked);
        }
        if (closeOptionButton != null)
        {
            closeOptionButton.onClick.RemoveListener(OnCloseOptionButtonClicked);
        }
    }

    // OnStartButton1Pressed - 対戦モード用
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

    // 一人用モード開始ボタン用のメソッド
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

    /// <summary>
    /// ヘルプボタンの OnClick イベントから呼び出すためのメソッド。
    /// </summary>
    public void OnHelpButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSound();
        }
        if (helpPanel != null)
        {
            helpPanel.SetActive(true);
        }
    }

    /// <summary>
    /// ヘルプパネルの閉じるボタンの OnClick イベントから呼び出すためのメソッド。
    /// </summary>
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

    /// <summary>
    /// オプションボタンの OnClick イベントから呼び出すためのメソッド。
    /// </summary>
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

    /// <summary>
    /// オプションパネルの閉じるボタンの OnClick イベントから呼び出すためのメソッド。
    /// </summary>
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