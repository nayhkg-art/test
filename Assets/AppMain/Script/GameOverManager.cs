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
    public SwipeCameraController swipeCameraController;
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
    public GameObject whiteScreen; // 親オブジェクト
    public Image whiteScreenImage;
    [SerializeField] private float fadeDuration = 1.0f; // フェードアウトにかかる時間
    private Text whiteScreenText;
    private TMP_Text whiteScreenTMPText;

    // --- ▼▼▼ 修正：TimeUp用とそれ以外（Finish/GameSet）用の2つのオブジェクトを用意 ▼▼▼ ---
    [Header("End Text Objects")]
    [SerializeField] private GameObject timeUpObject;  // 時間切れの時に表示
    [SerializeField] private GameObject finishObject;  // HP0やシングルの時に表示
    // --- ▲▲▲ 修正完了 ▲▲▲ ---

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
    [SerializeField] private TMP_Text singlePlayerFinalTimeText;

    private TimerManager timerManager;

    [Header("Stats Display Texts")]
    [SerializeField] private TMP_Text singlePlayerEnemiesDefeatedText;

    [Header("Single Player Rank UI")]
    [SerializeField] private float rankAppearDelay = 2.0f; 
    [SerializeField] private GameObject rank_S_UI;
    [SerializeField] private GameObject rank_A_UI;
    [SerializeField] private GameObject rank_B_UI;
    [SerializeField] private GameObject rank_C_UI;
    [SerializeField] private GameObject rank_D_UI;
    [SerializeField] private GameObject rank_E_UI;
    [SerializeField] private GameObject rank_F_UI;

    [Header("Single Player Rank Backgrounds")]
    [SerializeField] private GameObject rank_S_Bg_Object;
    [SerializeField] private GameObject rank_A_Bg_Object;
    [SerializeField] private GameObject rank_B_Bg_Object;
    [SerializeField] private GameObject rank_C_Bg_Object;
    [SerializeField] private GameObject rank_D_Bg_Object;
    [SerializeField] private GameObject rank_E_Bg_Object;
    [SerializeField] private GameObject rank_F_Bg_Object;

    [Header("Single Player Rank Sounds (SFX)")]
    [SerializeField] private AudioClip rankSlideSound; 
    [SerializeField] private AudioClip rank_S_Sound;
    [SerializeField] private AudioClip rank_A_Sound;
    [SerializeField] private AudioClip rank_B_Sound;
    [SerializeField] private AudioClip rank_C_Sound;
    [SerializeField] private AudioClip rank_D_Sound;
    [SerializeField] private AudioClip rank_E_Sound;
    [SerializeField] private AudioClip rank_F_Sound;

    [Range(0f, 1f)] [SerializeField] private float rankSoundVolume = 1.0f;
    [Range(1, 5)]   [SerializeField] private int rankSoundLayerCount = 2;

    [Header("Single Player Rank BGM")]
    [SerializeField] private AudioClip rank_S_Bgm;
    [SerializeField] private AudioClip rank_A_Bgm;
    [SerializeField] private AudioClip rank_B_Bgm;
    [SerializeField] private AudioClip rank_C_Bgm;
    [SerializeField] private AudioClip rank_D_Bgm;
    [SerializeField] private AudioClip rank_E_Bgm;
    [SerializeField] private AudioClip rank_F_Bgm;
    [Range(0f, 1f)] [SerializeField] private float rankBgmVolume = 0.5f;

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

        timerManager = FindFirstObjectByType<TimerManager>();
        if (timerManager == null)
        {
            Debug.LogError("TimerManagerが見つかりません。");
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

        // --- ▼▼▼ 修正：初期化時に両方のテキストオブジェクトを非表示にする ▼▼▼ ---
        if (timeUpObject != null) timeUpObject.SetActive(false);
        if (finishObject != null) finishObject.SetActive(false);
        // --- ▲▲▲ 修正完了 ▲▲▲ ---

        ResetRankBackgrounds();
    }

    private void ResetRankBackgrounds()
    {
        if (rank_S_Bg_Object != null) rank_S_Bg_Object.SetActive(false);
        if (rank_A_Bg_Object != null) rank_A_Bg_Object.SetActive(false);
        if (rank_B_Bg_Object != null) rank_B_Bg_Object.SetActive(false);
        if (rank_C_Bg_Object != null) rank_C_Bg_Object.SetActive(false);
        if (rank_D_Bg_Object != null) rank_D_Bg_Object.SetActive(false);
        if (rank_E_Bg_Object != null) rank_E_Bg_Object.SetActive(false);
        if (rank_F_Bg_Object != null) rank_F_Bg_Object.SetActive(false);
    }

    public void GameOver(GameOverReason reason)
    {
        if (isGameOver.Value) return;

        if (AudioManager.Instance != null) AudioManager.Instance.StopAllSounds();

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

            if (singlePlayerFinalTimeText != null && timerManager != null)
            {
                float finalTime = timerManager.CurrentTime;
                int minutes = (int)(finalTime / 60);
                int seconds = (int)(finalTime % 60);
                int centiseconds = (int)((finalTime * 100) % 100);
                singlePlayerFinalTimeText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, centiseconds);
            }
            else
            {
                if (singlePlayerFinalTimeText == null) Debug.LogError("singlePlayerFinalTimeTextが設定されていません！");
                if (timerManager == null) Debug.LogError("timerManagerが見つかりませんでした！");
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
            // --- ▼▼▼ 修正：シングルプレイは「Time Upではない」ので false を渡す（FinishObjectが表示される） ▼▼▼ ---
            StartCoroutine(WhiteOutAndResult(singlePlayerResultWindow, "None", false));
            // --- ▲▲▲ 修正完了 ▲▲▲ ---
        }

        StartCoroutine(ShowRankUI(reason));
    }

    private IEnumerator ShowRankUI(GameOverReason reason)
    {
        float accuracy = 0f;
        if (QuestionManager.TotalEnemyNum > 0)
        {
            accuracy = (float)QuestionManager.CorrectAnswerNum / QuestionManager.TotalEnemyNum;
        }

        RankManager.Rank rank = RankManager.GetRankFromAccuracy(accuracy);
        RankManager.SaveBestRank(GameSelectionManager.SelectedGameType, rank);
        if (scoreAnimeManager != null)
        {
            RankManager.SaveBestScore(GameSelectionManager.SelectedGameType, scoreAnimeManager.Score);
        }

        GameObject rankUIToShow = null;
        AudioClip rankSound = null;
        AudioClip rankBgm = null;
        GameObject rankBgObject = null;

        switch (rank)
        {
            case RankManager.Rank.S:
                rankUIToShow = rank_S_UI;
                rankSound = rank_S_Sound;
                rankBgm = rank_S_Bgm;
                rankBgObject = rank_S_Bg_Object;
                break;
            case RankManager.Rank.A:
                rankUIToShow = rank_A_UI;
                rankSound = rank_A_Sound;
                rankBgm = rank_A_Bgm;
                rankBgObject = rank_A_Bg_Object;
                break;
            case RankManager.Rank.B:
                rankUIToShow = rank_B_UI;
                rankSound = rank_B_Sound;
                rankBgm = rank_B_Bgm;
                rankBgObject = rank_B_Bg_Object;
                break;
            case RankManager.Rank.C:
                rankUIToShow = rank_C_UI;
                rankSound = rank_C_Sound;
                rankBgm = rank_C_Bgm;
                rankBgObject = rank_C_Bg_Object;
                break;
            case RankManager.Rank.D:
                rankUIToShow = rank_D_UI;
                rankSound = rank_D_Sound;
                rankBgm = rank_D_Bgm;
                rankBgObject = rank_D_Bg_Object;
                break;
            case RankManager.Rank.E:
                rankUIToShow = rank_E_UI;
                rankSound = rank_E_Sound;
                rankBgm = rank_E_Bgm;
                rankBgObject = rank_E_Bg_Object;
                break;
            case RankManager.Rank.F:
                rankUIToShow = rank_F_UI;
                rankSound = rank_F_Sound;
                rankBgm = rank_F_Bgm;
                rankBgObject = rank_F_Bg_Object;
                break;
        }

        ResetRankBackgrounds();
        if (rankBgObject != null)
        {
            rankBgObject.SetActive(true);
            Debug.Log($"[GameOverManager] 背景オブジェクト {rankBgObject.name} をすぐに表示しました。");
        }

        yield return new WaitForSecondsRealtime(rankAppearDelay);

        if (rankUIToShow != null)
        {
            RectTransform rankRectTransform = rankUIToShow.GetComponent<RectTransform>();
            if (rankRectTransform == null)
            {
                Debug.LogError("ランクUIにRectTransformがアタッチされていません！");
                yield break;
            }

            float animationDuration = 0.5f;
            float startPositionX = 800f;
            Vector2 targetPosition = rankRectTransform.anchoredPosition;
            Vector2 startPosition = new Vector2(startPositionX, targetPosition.y);

            rankRectTransform.anchoredPosition = startPosition;
            rankUIToShow.SetActive(true);

            if (rankSlideSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(rankSlideSound);
            }

            float elapsedTime = 0f;
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / animationDuration);
                rankRectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            rankRectTransform.anchoredPosition = targetPosition;

            if (rankSound != null && AudioManager.Instance != null)
            {
                for (int i = 0; i < rankSoundLayerCount; i++)
                {
                    AudioManager.Instance.PlaySFX(rankSound, rankSoundVolume);
                }
                Debug.Log($"[GameOverManager] ランク音を {rankSoundLayerCount} 回重ねて再生しました。");

                yield return new WaitForSecondsRealtime(rankSound.length);
            }

            if (rankBgm != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBGM(rankBgm, rankBgmVolume);
                Debug.Log($"[GameOverManager] ランクBGM {rankBgm.name} を再生しました。");
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

        // --- ▼▼▼ 修正: TimeUp(HP負けではない) か判定 ▼▼▼
        bool isTimeUp = (playerWhoLostByHP.Value == ulong.MaxValue);
        // --- ▲▲▲ 修正完了 ▲▲▲ ---

        if (windowToShow != null)
        {
            Debug.Log($"[GameOverManager] 結果ウィンドウ {windowToShow.name} を表示し、ホワイトアウト処理を開始します。");
            // --- ▼▼▼ 修正: 第3引数にフラグを渡す ▼▼▼
            StartCoroutine(WhiteOutAndResult(windowToShow, soundMethodName, isTimeUp));
            // --- ▲▲▲ 修正完了 ▲▲▲ ---
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

    private void SetAlpha(float alpha)
    {
        if (whiteScreenImage != null)
        {
            Color c = whiteScreenImage.color;
            c.a = alpha;
            whiteScreenImage.color = c;
        }
        if (whiteScreenText != null)
        {
            Color c = whiteScreenText.color;
            c.a = alpha;
            whiteScreenText.color = c;
        }
        if (whiteScreenTMPText != null)
        {
            Color c = whiteScreenTMPText.color;
            c.a = alpha;
            whiteScreenTMPText.color = c;
        }
    }

    // --- ▼▼▼ 修正: TimeUpかどうかで表示を分岐させる処理に変更 ▼▼▼ ---
    IEnumerator WhiteOutAndResult(GameObject resultWindow, string soundMethodName, bool isTimeUp = false)
    {
        Debug.Log("[GameOverManager] WhiteOutAndResult コルーチン開始。");
        if (whiteScreenImage == null) { Debug.LogError("[GameOverManager] WhiteScreenImageがnullです。"); yield break; }
        if (audioManager == null) { Debug.LogError("[GameOverManager] AudioManagerがnullです。"); yield break; }

        if (whiteScreen != null) 
        {
            whiteScreen.SetActive(true);
            whiteScreen.transform.SetAsLastSibling(); 
        }
        whiteScreenImage.gameObject.SetActive(true);

        // --- ▼▼▼ 修正: どちらか一方だけを表示する（排他制御） ▼▼▼ ---
        if (isTimeUp)
        {
            // 時間切れの場合
            if (timeUpObject != null) timeUpObject.SetActive(true);
            if (finishObject != null) finishObject.SetActive(false);
        }
        else
        {
            // それ以外（HP0 または シングル）の場合
            if (timeUpObject != null) timeUpObject.SetActive(false);
            if (finishObject != null) finishObject.SetActive(true);
        }
        // --- ▲▲▲ 修正完了 ▲▲▲ ---

        audioManager.StopAllSounds();

        SetAlpha(1.0f);
        Debug.Log("[GameOverManager] ホワイトスクリーンを一瞬で表示しました (Alpha=1)。");

        if (audioManager != null)
        {
            audioManager.PlayFinishSound();
            Debug.Log("[GameOverManager] PlayFinishSound を再生しました。");
        }

        yield return new WaitForSecondsRealtime(2f);

        // --- ▼▼▼ 修正: 両方のテキストを非表示にしてから結果を表示 ▼▼▼ ---
        if (timeUpObject != null) timeUpObject.SetActive(false);
        if (finishObject != null) finishObject.SetActive(false);
        // --- ▲▲▲ 修正完了 ▲▲▲ ---

        if (audioManager != null)
        {
            Debug.Log($"[GameOverManager] 結果サウンド ({soundMethodName}) を再生します。");
            switch (soundMethodName)
            {
                case "PlayWinSound": audioManager.PlayWinSound(); break;
                case "PlayLoseSound": audioManager.PlayLoseSound(); break;
                case "PlayDrawSound": audioManager.PlayDrawSound(); break;
                case "PlaySingleResultBgm": audioManager.PlaySingleResultBgm(); break;
                case "None": Debug.Log("[GameOverManager] BGM再生をスキップしました。"); break;
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

        float elapsedTime = 0f;
        // インスペクターで設定した fadeDuration を使用
        Debug.Log("[GameOverManager] ホワイトスクリーン フェードアウト開始。");
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; 
            float alpha = Mathf.Lerp(1.0f, 0f, elapsedTime / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }
        
        Debug.Log("[GameOverManager] ホワイトスクリーン フェードアウト完了。");

        SetAlpha(0f);
        if (whiteScreen != null) whiteScreen.SetActive(false);
        if (whiteScreenImage != null) whiteScreenImage.gameObject.SetActive(false);
        
        Debug.Log("[GameOverManager] WhiteOutAndResult コルーチン終了。");
    }

    public void stop()
    {
        Debug.Log("[GameOverManager] stop() 呼び出し。プレイヤー操作とUIを停止します。");
        if (mainCanvas != null) mainCanvas.SetActive(false);
        if (joystickPlayerController != null) joystickPlayerController.enabled = false;
        if (swipeCameraController != null) swipeCameraController.enabled = false;
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