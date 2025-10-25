using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public enum GameOverReason
{
    Score,
    HPLoss
}

public class GameOverManager : NetworkBehaviour
{
    public JoystickPlayerController joystickPlayerController;
    public SwipeCameraController swipeCameraController; // ★ 修正点: LookController を SwipeCameraController に変更
    public CharacterController characterController;
    public Shooting shooting;

    public GameObject WinWindow;
    public GameObject LoseWindow;
    public GameObject DrawWindow;
    public GameObject DisconnectedWindow;
    public GameObject Player;
    public GameObject Gun;

    public NetworkVariable<bool> isGameOver = new NetworkVariable<bool>(false);
    public NetworkVariable<ulong> playerWhoLostByHP = new NetworkVariable<ulong>(ulong.MaxValue);

    [SerializeField] private ScoreAnimeManager scoreAnimeManager;
    [SerializeField] private Heartbeat heartbeat;

    public Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();
    private bool isRequestingGameOver = false;
    private Coroutine gameOverCoroutine;
    private AudioManager audioManager;
    private GameOverReason localGameOverReason;

    [SerializeField] private GameObject mainCanvas;
    [Header("White Screen Fade")]
    public GameObject whiteScreen;
    public Image whiteScreenImage;
    private Text whiteScreenText;
    private TMP_Text whiteScreenTMPText;

    [Header("Score Display Texts")]
    [SerializeField] private TMP_Text winMyScoreText;
    [SerializeField] private TMP_Text winFriendScoreText;
    [SerializeField] private TMP_Text loseMyScoreText;
    [SerializeField] private TMP_Text loseFriendScoreText;
    [SerializeField] private TMP_Text drawMyScoreText;
    [SerializeField] private TMP_Text drawFriendScoreText;

    [Header("Single Player UI")]
    [SerializeField] private GameObject singlePlayerResultWindow;
    [SerializeField] private TMP_Text singlePlayerScoreText;

    [Header("Stats Display Texts")]
    [SerializeField] private TMP_Text singlePlayerEnemiesDefeatedText;

    [Header("Single Player Rank UI")]
    [SerializeField] private GameObject rank_S_UI;
    [SerializeField] private GameObject rank_A_UI;
    [SerializeField] private GameObject rank_B_UI;
    [SerializeField] private GameObject rank_C_UI;

    [Header("Single Player Rank Sounds")]
    [SerializeField] private AudioClip rank_S_Sound;
    [SerializeField] private AudioClip rank_A_Sound;
    [SerializeField] private AudioClip rank_B_Sound;
    [SerializeField] private AudioClip rank_C_Sound;


    private void Awake()
    {
        isGameOver.OnValueChanged += OnGameOverChanged;
    }

    void Start()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogError("AudioManagerが見つかりません。");
        }
        if (heartbeat == null)
        {
            Debug.LogError("Heartbeatが設定されていません!");
        }
        if (whiteScreenImage != null && whiteScreenImage.transform.childCount > 0)
        {
            whiteScreenText = whiteScreenImage.GetComponentInChildren<Text>();
            whiteScreenTMPText = whiteScreenImage.GetComponentInChildren<TMP_Text>();
        }
    }

    public void GameOver(GameOverReason reason)
    {
        if (isGameOver.Value) return;

        if (AudioManager.Instance != null) AudioManager.Instance.StopAllSounds();

        // --- ▼▼▼ ここから変更 ▼▼▼ ---
        // 敵を消す処理を OnGameOverChanged に移動させたため、ここからは削除します。
        /*
        if (SpawnEnemyManager.Instance != null)
        {
            SpawnEnemyManager.Instance.StopAllSpawning(); 
            SpawnEnemyManager.Instance.DestroyAllActiveEnemies();
        }
        */
        // --- ▲▲▲ ここまで変更 ▲▲▲ ---

        if (GameSelectionManager.Instance != null && GameSelectionManager.Instance.CurrentGameMode == GameSelectionManager.GameMode.SinglePlayer)
        {
            isGameOver.Value = true;
            HandleSinglePlayerGameOver(reason);
            return;
        }
        else if (GameSelectionManager.Instance == null)
        {
            int gameModeInt = PlayerPrefs.GetInt("GameMode", (int)GameSelectionManager.GameMode.None);
            GameSelectionManager.GameMode currentSelectedMode = (GameSelectionManager.GameMode)gameModeInt;
            if (currentSelectedMode == GameSelectionManager.GameMode.SinglePlayer)
            {
                isGameOver.Value = true;
                HandleSinglePlayerGameOver(reason);
                Debug.LogWarning("[GameOverManager] GameSelectionManager.Instance が null のため、PlayerPrefsからモードを読み込みました (一人用)。");
                return;
            }
        }

        if (!isRequestingGameOver)
        {
            isRequestingGameOver = true;
            localGameOverReason = reason;
            gameOverCoroutine = StartCoroutine(SendGameOverRepeatedly());
        }
        if (joystickPlayerController != null)
        {
            joystickPlayerController.enabled = false;
        }
    }

    private void HandleSinglePlayerGameOver(GameOverReason reason)
    {
        Debug.Log("一人用モードのゲームオーバー処理を開始します。");
        stop();

        if (SpawnEnemyManager.Instance != null)
        {
            SpawnEnemyManager.Instance.StopAllSpawning();
            SpawnEnemyManager.Instance.DestroyAllActiveEnemies();
        }

        if (singlePlayerResultWindow != null)
        {
            if (singlePlayerScoreText != null && scoreAnimeManager != null)
            {
                singlePlayerScoreText.text = $"{scoreAnimeManager.Score}";
            }
            if (singlePlayerEnemiesDefeatedText != null)
            {
                singlePlayerEnemiesDefeatedText.text = $"{(int)QuestionManager.CorrectAnswerNum} / {(int)QuestionManager.TotalEnemyNum}";
            }
        }
        else
        {
            Debug.LogError("一人用モードの結果ウィンドウが設定されていません！");
        }

        if (audioManager != null)
        {
            StartCoroutine(WhiteOutAndResult(singlePlayerResultWindow, "PlayWinSound"));
        }

        // ランク計算と表示
        StartCoroutine(ShowRankUI(reason));
    }

    private IEnumerator ShowRankUI(GameOverReason reason)
    {
        yield return new WaitForSeconds(2.0f); // 結果パネル表示から数秒待つ

        GameObject rankUIToShow = null;
        AudioClip rankSound = null;

        // ランク判定
        if (reason == GameOverReason.HPLoss || (reason == GameOverReason.Score && QuestionManager.DefeatEnemyNum < QuestionManager.TotalEnemyNum))
        {
            // HPが0になった、または時間切れで敵を全て倒せなかった
            rankUIToShow = rank_C_UI;
            rankSound = rank_C_Sound;
        }
        else
        {
            // 全ての敵を倒した
            float accuracy = (float)QuestionManager.CorrectAnswerNum / QuestionManager.TotalEnemyNum;
            if (accuracy >= 1.0f)
            {
                rankUIToShow = rank_S_UI;
                rankSound = rank_S_Sound;
            }
            else if (accuracy >= 0.8f)
            {
                rankUIToShow = rank_A_UI;
                rankSound = rank_A_Sound;
            }
            else
            {
                rankUIToShow = rank_B_UI;
                rankSound = rank_B_Sound;
            }
        }

        if (rankUIToShow != null)
        {
            rankUIToShow.SetActive(true);
            // ここでアニメーションを再生する（例：Animatorを使い "SlideIn" トリガーを起動）
            Animator animator = rankUIToShow.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("SlideIn");
            }

            // アニメーションの長さに合わせて待機（またはアニメーションイベントを使う）
            // ここでは仮に1秒待つ
            yield return new WaitForSeconds(1.0f);

            if (rankSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayOneShotSFX(rankSound);
            }
        }
    }

    private IEnumerator SendGameOverRepeatedly()
    {
        if (NetworkManager.Singleton == null || !(NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer))
        {
            Debug.LogWarning("NetworkManagerが利用できないか、クライアント/サーバーとして動作していません。DisconnectedWindowを表示します。");
            if (DisconnectedWindow != null) DisconnectedWindow.SetActive(true);
            stop();
            yield break;
        }

        if (!IsHost && !NetworkManager.Singleton.IsClient)
        {
            Debug.Log("ホストが接続されていません (クライアントが接続できていない状態)。DisconnectedWindowを表示します。");
            if (DisconnectedWindow != null) DisconnectedWindow.SetActive(true);
            stop();
            yield break;
        }

        while (!isGameOver.Value)
        {
            if (NetworkManager.Singleton.IsClient)
            {
                NotifyGameOverServerRpc(NetworkManager.Singleton.LocalClientId, localGameOverReason);
            }
            yield return new WaitForSeconds(1f);
        }
        isRequestingGameOver = false;
    }

    [ServerRpc(RequireOwnership = false)]
    void NotifyGameOverServerRpc(ulong playerID, GameOverReason reason)
    {
        if (!isGameOver.Value)
        {
            Debug.Log($"[GameOverManager] サーバーRPC: GameOver通知受信。PlayerID: {playerID}, Reason: {reason}");
            if (reason == GameOverReason.HPLoss)
            {
                playerWhoLostByHP.Value = playerID;
            }
            isGameOver.Value = true;
            RequestFinalScoreClientRpc();
        }
        else
        {
            Debug.Log($"[GameOverManager] サーバーRPC: GameOverは既に発生しています。通知を無視します。PlayerID: {playerID}, Reason: {reason}");
        }
    }

    private void OnGameOverChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            Debug.Log($"[GameOverManager] OnGameOverChanged: isGameOverが {newValue} に変更されました。");

            // --- ▼▼▼ ここから変更 ▼▼▼ ---
            // isGameOverがtrueになった時、全クライアントで敵の生成を停止し、既存の敵を全て破壊する
            if (SpawnEnemyManager.Instance != null)
            {
                Debug.Log("[GameOverManager] 敵の生成を停止し、全ての敵を破壊します。");
                SpawnEnemyManager.Instance.StopAllSpawning();
                SpawnEnemyManager.Instance.DestroyAllActiveEnemies();
            }
            else
            {
                 Debug.LogError("[GameOverManager] SpawnEnemyManager.Instanceが見つかりません！");
            }
            // --- ▲▲▲ ここまで変更 ▲▲▲ ---

            if (heartbeat != null)
            {
                heartbeat.NotifyGameOver();
                Debug.Log("[GameOverManager] ゲームオーバーのため、Heartbeatに通知しました。");
            }
            else
            {
                Debug.LogError("[GameOverManager] OnGameOverChanged: Heartbeatが設定されていません！");
            }
            stop();
        }
    }


    [ClientRpc]
    void RequestFinalScoreClientRpc()
    {
        if (scoreAnimeManager == null)
        {
            Debug.LogError("[GameOverManager] RequestFinalScoreClientRpc: ScoreAnimeManagerが設定されていません！");
            ReportFinalScoreServerRpc(NetworkManager.Singleton.LocalClientId, 0);
            return;
        }
        int finalScore = scoreAnimeManager.Score;
        Debug.Log($"[GameOverManager] クライアントRPC: 最終スコアを報告します。PlayerID: {NetworkManager.Singleton.LocalClientId}, Score: {finalScore}");
        ReportFinalScoreServerRpc(NetworkManager.Singleton.LocalClientId, finalScore);
    }

    [ServerRpc(RequireOwnership = false)]
    void ReportFinalScoreServerRpc(ulong playerID, int finalScore)
    {
        Debug.Log($"[GameOverManager] サーバーRPC: 最終スコア報告受信。PlayerID: {playerID}, Score: {finalScore}");
        if (!playerScores.ContainsKey(playerID))
        {
            playerScores[playerID] = finalScore;
            Debug.Log($"[GameOverManager] サーバー: Player {playerID} のスコアを {finalScore} に設定しました。");
        }
        else
        {
            Debug.LogWarning($"[GameOverManager] サーバー: Player {playerID} のスコアは既に存在します。更新しません。");
        }

        if (IsServer && playerScores.Count >= NetworkManager.Singleton.ConnectedClients.Count)
        {
            Debug.Log($"[GameOverManager] サーバー: 全てのクライアントからスコアを受信しました ({playerScores.Count}/{NetworkManager.Singleton.ConnectedClients.Count})。勝敗判定を開始します。");

            if (NetworkManager.Singleton.ConnectedClients.Count == 1)
            {
                Debug.Log("[GameOverManager] サーバー: 1人プレイモード。");
                DecideWinner(playerScores[NetworkManager.ServerClientId], null);
                return;
            }

            int? hostScore = null;
            int? clientScore = null;

            if (playerScores.ContainsKey(NetworkManager.ServerClientId))
            {
                hostScore = playerScores[NetworkManager.ServerClientId];
            }
            else
            {
                Debug.LogError("[GameOverManager] サーバー: ホストのスコアが見つかりません！");
            }

            foreach (var kvp in playerScores)
            {
                if (kvp.Key != NetworkManager.ServerClientId)
                {
                    clientScore = kvp.Value;
                    break;
                }
            }

            if (!clientScore.HasValue)
            {
                Debug.LogWarning("[GameOverManager] サーバー: クライアントのスコアが見つかりません！相手が退出した可能性があります。");
            }

            DecideWinner(hostScore, clientScore);
        }
        else
        {
            Debug.Log($"[GameOverManager] サーバー: スコア報告待ち。現在のスコア数: {playerScores.Count}, 接続クライアント数: {NetworkManager.Singleton.ConnectedClients.Count}");
        }
    }

    private void DecideWinner(int? hostScore, int? clientScore)
    {
        Debug.Log($"[GameOverManager] 勝敗判定: HostScore={hostScore}, ClientScore={clientScore}");
        int result = 0;

        if (playerWhoLostByHP.Value != ulong.MaxValue)
        {
            Debug.Log($"[GameOverManager] 勝敗判定: HP損失による決着。LostPlayerID: {playerWhoLostByHP.Value}");
            result = (playerWhoLostByHP.Value == NetworkManager.ServerClientId) ? 2 : 1;
        }
        else
        {
            Debug.Log("[GameOverManager] 勝敗判定: スコアによる決着。");

            if (!hostScore.HasValue || !clientScore.HasValue)
            {
                if (hostScore.HasValue)
                {
                    Debug.LogWarning("[GameOverManager] クライアントのスコアがないため、ホストの勝利とします。");
                    result = 1;
                }
                else if (clientScore.HasValue)
                {
                    Debug.LogWarning("[GameOverManager] ホストのスコアがないため、クライアントの勝利とします。");
                    result = 2;
                }
                else
                {
                    Debug.LogError("[GameOverManager] 両プレイヤーのスコアがないため、エラーとして処理します。");
                    result = 9;
                }
            }
            else if (hostScore > clientScore)
            {
                result = 1;
            }
            else if (hostScore < clientScore)
            {
                result = 2;
            }
            else
            {
                result = 0;
            }
        }
        Debug.Log($"[GameOverManager] 勝敗判定結果: Result={result}");
        ResultClientRpc(result, hostScore.GetValueOrDefault(), clientScore.GetValueOrDefault(0));
    }


    [ClientRpc]
    void ResultClientRpc(int result, int hostScore, int clientScore)
    {
        Debug.Log($"[GameOverManager] クライアントRPC: 結果受信。Result={result}, HostScore={hostScore}, ClientScore={clientScore}");

        int myScore = IsHost ? hostScore : clientScore;
        int friendScore = IsHost ? clientScore : hostScore;
        GameObject windowToShow = null;
        string soundMethodName = "";
        TMP_Text myScoreTextUI = null;
        TMP_Text friendScoreTextUI = null;

        if (result == 9)
        {
            Debug.Log("[GameOverManager] 結果: 切断またはエラー。DisconnectedWindowを表示。");
            if (DisconnectedWindow != null) DisconnectedWindow.SetActive(true);
            stop();
            return;
        }

        bool isDraw = (result == 0);
        bool amIWinner = (IsHost && result == 1) || (!IsHost && result == 2);

        if (isDraw)
        {
            windowToShow = DrawWindow;
            soundMethodName = "PlayDrawSound";
            myScoreTextUI = drawMyScoreText;
            friendScoreTextUI = drawFriendScoreText;
            Debug.Log("[GameOverManager] 結果: 引き分け。");
        }
        else if (amIWinner)
        {
            windowToShow = WinWindow;
            soundMethodName = "PlayWinSound";
            myScoreTextUI = winMyScoreText;
            friendScoreTextUI = winFriendScoreText;
            Debug.Log("[GameOverManager] 結果: 勝利。");
        }
        else
        {
            windowToShow = LoseWindow;
            soundMethodName = "PlayLoseSound";
            myScoreTextUI = loseMyScoreText;
            friendScoreTextUI = loseFriendScoreText;
            Debug.Log("[GameOverManager] 結果: 敗北。");
        }

        SetScoreText(myScoreTextUI, friendScoreTextUI, myScore, friendScore);

        if (windowToShow != null)
        {
            Debug.Log($"[GameOverManager] 結果ウィンドウ {windowToShow.name} を表示し、ホワイトアウト処理を開始します。");
            StartCoroutine(WhiteOutAndResult(windowToShow, soundMethodName));
        }
        else
        {
            Debug.LogError("[GameOverManager] 表示する結果ウィンドウがnullです！");
        }
    }

    private void SetScoreText(TMP_Text myScoreText, TMP_Text friendScoreText, int myScore, int friendScore)
    {
        if (myScoreText != null) myScoreText.text = $"YOUR SCORE   : {myScore}";
        if (friendScoreText != null) friendScoreText.text = $"FRIEND SCORE: {friendScore}";
        Debug.Log($"[GameOverManager] スコアUIを更新: Your Score={myScore}, Friend Score={friendScore}");
    }

    IEnumerator WhiteOutAndResult(GameObject resultWindow, string soundMethodName)
    {
        Debug.Log("[GameOverManager] WhiteOutAndResult コルーチン開始。");
        if (whiteScreenImage == null) { Debug.LogError("[GameOverManager] WhiteScreenImageがnullです。"); yield break; }
        if (audioManager == null) { Debug.LogError("[GameOverManager] AudioManagerがnullです。"); yield break; }

        whiteScreenImage.gameObject.SetActive(true);
        if (whiteScreenText != null) whiteScreenText.gameObject.SetActive(true);
        if (whiteScreenTMPText != null) whiteScreenTMPText.gameObject.SetActive(true);

        audioManager.StopAllSounds();
        Debug.Log("[GameOverManager] すべてのサウンドを停止しました。");

        float elapsedTime = 0f;
        float startAlpha = 1f;

        Color initialImageColor = whiteScreenImage.color;
        initialImageColor.a = 0f;
        whiteScreenImage.color = initialImageColor;

        Color initialTextColor = Color.white;
        if (whiteScreenText != null) { initialTextColor = whiteScreenText.color; initialTextColor.a = 0f; whiteScreenText.color = initialTextColor; }

        Color initialTMPTextColor = Color.white;
        if (whiteScreenTMPText != null) { initialTMPTextColor = whiteScreenTMPText.color; initialTMPTextColor.a = 0f; whiteScreenTMPText.color = initialTMPTextColor; }

        Debug.Log("[GameOverManager] ホワイトスクリーン フェードイン開始。");
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, startAlpha, elapsedTime / 1f);

            Color imageColor = whiteScreenImage.color;
            imageColor.a = alpha;
            whiteScreenImage.color = imageColor;

            if (whiteScreenText != null) { Color textColor = whiteScreenText.color; textColor.a = alpha; whiteScreenText.color = textColor; }
            if (whiteScreenTMPText != null) { Color tmpTextColor = whiteScreenTMPText.color; tmpTextColor.a = alpha; whiteScreenTMPText.color = tmpTextColor; }
            yield return null;
        }
        Debug.Log("[GameOverManager] ホワイトスクリーン フェードイン完了。");

        if (audioManager != null)
        {
            audioManager.PlayFinishSound();
            Debug.Log("[GameOverManager] PlayFinishSound を再生しました。");
        }
        yield return new WaitForSeconds(2f);

        if (audioManager != null)
        {
            Debug.Log($"[GameOverManager] 結果サウンド ({soundMethodName}) を再生します。");
            switch (soundMethodName)
            {
                case "PlayWinSound": audioManager.PlayWinSound(); break;
                case "PlayLoseSound": audioManager.PlayLoseSound(); break;
                case "PlayDrawSound": audioManager.PlayDrawSound(); break;
                default:
                    Debug.LogWarning($"[GameOverManager] 不明なサウンドメソッド名: {soundMethodName}");
                    break;
            }
        }

        if (resultWindow != null)
        {
            resultWindow.SetActive(true);
            Debug.Log($"[GameOverManager] 結果ウィンドウ {resultWindow.name} を有効にしました。");
        }

        float fadeOutStartTime = elapsedTime;
        float fadeOutDuration = 3f;
        Debug.Log("[GameOverManager] ホワイトスクリーン フェードアウト開始。");
        while (elapsedTime < fadeOutStartTime + fadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, (elapsedTime - fadeOutStartTime) / fadeOutDuration);

            Color imageColor = whiteScreenImage.color;
            imageColor.a = alpha;
            whiteScreenImage.color = imageColor;

            if (whiteScreenText != null) { Color textColor = whiteScreenText.color; textColor.a = alpha; whiteScreenText.color = textColor; }
            if (whiteScreenTMPText != null) { Color tmpTextColor = whiteScreenTMPText.color; tmpTextColor.a = alpha; whiteScreenTMPText.color = tmpTextColor; }
            yield return null;
        }
        Debug.Log("[GameOverManager] ホワイトスクリーン フェードアウト完了。");

        if (whiteScreen != null) whiteScreen.SetActive(false);
        if (whiteScreenText != null) whiteScreenText.gameObject.SetActive(false);
        if (whiteScreenTMPText != null) whiteScreenTMPText.gameObject.SetActive(false);

        Color finalImageColor = whiteScreenImage.color;
        finalImageColor.a = 0f;
        whiteScreenImage.color = finalImageColor;

        if (whiteScreenText != null) { Color finalTextColor = whiteScreenText.color; finalTextColor.a = 0f; whiteScreenText.color = finalTextColor; }
        if (whiteScreenTMPText != null) { Color finalTMPTextColor = whiteScreenTMPText.color; finalTMPTextColor.a = 0f; whiteScreenTMPText.color = finalTMPTextColor; }
        Debug.Log("[GameOverManager] WhiteOutAndResult コルーチン終了。");
    }

    public void stop()
    {
        Debug.Log("[GameOverManager] stop() 呼び出し。プレイヤー操作とUIを停止します。");
        if (mainCanvas != null) mainCanvas.SetActive(false);
        if (joystickPlayerController != null) joystickPlayerController.enabled = false;
        if (swipeCameraController != null) swipeCameraController.enabled = false; // ★ 修正点: lookController を swipeCameraController に変更
        if (characterController != null) characterController.enabled = false;
        if (shooting != null) shooting.enabled = false;
        if (Gun != null) Gun.SetActive(false);
        if (audioManager != null)
        {
            audioManager.StopAllSounds();
            Debug.Log("[GameOverManager] すべてのサウンドを停止しました (stop)。");
        }
    }
}