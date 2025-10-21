using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

public class Heartbeat : NetworkBehaviour
{
    [Header("Game Logic Managers")]
    [SerializeField] private CountdownManager countdownManager;
    [SerializeField] private SpawnEnemyManager spawnEnemyManager;
    [SerializeField] private TimerManager timerManager;
    [SerializeField] private ScoreAnimeManager scoreAnimeManager;
    [SerializeField] private GameOverManager gameOverManager;

    [Header("Utility Components")]
    [SerializeField] private InternetConnectionMonitor internetMonitor;

    [Header("UI References")]
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject lobbyCanvas;
    [SerializeField] private LobbyUIManager lobbyUIManager;
    [SerializeField] private GameObject friendScoreObject;
    [SerializeField] private TMP_Text friendScoreText;
    [SerializeField] private Image friendHpGage;
    [SerializeField] private List<GameObject> attackMarkIcons = new List<GameObject>();

    [Header("Gauge Settings")]
    [SerializeField] private Gradient hpGradient;
    [SerializeField] private float gageSpeed = 3f;

    [Header("UI References for Connection")]
    [SerializeField] private GameObject disconnectTimeoutFirstPhaseUI;
    [SerializeField] private GameObject disconnectTimeoutSecondPhaseUI;

    [Header("Camera Shake")]
    [Tooltip("揺らす対象のカメラコントローラー")]
    [SerializeField] private CameraCustomController cameraController;

    [Header("Thunder Attack")]
    [Tooltip("5コンボごとに有効になる雷攻撃ボタン")]
    [SerializeField] private Button thunderAttackButton;
    [Tooltip("相手フィールドに表示する雷エフェクトのPrefab")]
    [SerializeField] private GameObject thunderEffectPrefab;
    [Tooltip("雷エフェクトを表示する場所のTransformのリスト")]
    [SerializeField] private List<Transform> opponentFieldTransforms = new List<Transform>();
    [Tooltip("雷攻撃のダメージ量")]
    [SerializeField] private int thunderAttackDamage = 10;
    [Tooltip("雷攻撃の効果音(相手側で鳴る)")]
    [SerializeField] private AudioClip thunderSoundEffect;
    [Tooltip("雷攻撃の効果音の音量。1以上でブースト可能")]
    [Range(0f, 5f)]
    [SerializeField] private float thunderSoundVolume = 1.0f;
    [Tooltip("コンボボタンを押した時の効果音(自分側でのみ鳴る)")]
    [SerializeField] private AudioClip thunderButtonClickSound;
    [Tooltip("攻撃送信時に表示する通知イメージ（雷アイコン）")]
    [SerializeField] private GameObject attackSentNotificationImage;
    [Tooltip("雷アイコンの背景として表示するイメージ")]
    [SerializeField] private GameObject attackSentBackgroundImage;
    [Tooltip("雷アイコンの点滅回数")]
    [SerializeField] private int thunderFlashCount = 4;

    private NetworkVariable<bool> isPlayingGame = new NetworkVariable<bool>(false);
    public NetworkVariable<int> connectedClientsCount = new NetworkVariable<int>(0);
    private NetworkVariable<bool> gameStartSignal = new NetworkVariable<bool>(false);

    // Private Fields
    private StatusManagerPlayer localPlayerStatus;
    private Dictionary<ulong, bool> clientReadyStatus = new Dictionary<ulong, bool>();
    private float lastClientHeartbeatTime = 0.0f;
    private float lastHostHeartbeatTime = 0.0f;
    private float heartbeatInterval = 1.0f;
    private float disconnectTimeoutFirstPhase = 5.0f;
    private float disconnectTimeoutSecondPhase = 10.0f;
    private float disconnectTimeout = 20.0f;
    private bool isDisconnectHandled = false;
    private bool isHostInstance = false;
    private bool isClientOnlyInstance = false;
    private bool isSinglePlayer = false;
    private float friendHpTarget = 0f;
    private bool isIntentionalDisconnect = false;

    private int localSentEnemiesCount = 0;
    private int defeatedAttackEnemiesCount = 0;
    private int opponentDefeatedAttackEnemiesCount = 0;

    private Coroutine buttonFlashCoroutine;
    // --- ▼▼▼ ここから変更 ▼▼▼ ---
    private bool isGameOver = false; // ゲームオーバー状態を管理するフラグ
    // --- ▲▲▲ ここまで変更 ▲▲▲ ---

    private void Awake()
    {
        if (GameSelectionManager.Instance != null)
        {
            isSinglePlayer = GameSelectionManager.Instance.CurrentGameMode == GameSelectionManager.GameMode.SinglePlayer;
        }
        else
        {
            int gameModeInt = PlayerPrefs.GetInt("GameMode", (int)GameSelectionManager.GameMode.None);
            isSinglePlayer = (GameSelectionManager.GameMode)gameModeInt == GameSelectionManager.GameMode.SinglePlayer;
        }
    }

    void Start()
    {
        EnsureReferences();
        localPlayerStatus = FindFirstObjectByType<StatusManagerPlayer>();
        if (localPlayerStatus == null && !isSinglePlayer)
        {
            Debug.LogError("[Heartbeat] シーン内に StatusManagerPlayer が見つかりません！");
        }

        disconnectTimeoutFirstPhaseUI?.SetActive(false);
        disconnectTimeoutSecondPhaseUI?.SetActive(false);
        
        if (attackSentNotificationImage != null)
        {
            attackSentNotificationImage.SetActive(false);
        }
        if (attackSentBackgroundImage != null)
        {
            attackSentBackgroundImage.SetActive(false);
        }

        if (isSinglePlayer)
        {
            UpdateUIForGame();
            if (friendScoreObject != null) friendScoreObject.SetActive(false);
            UpdateAttackMarkUI(0);
            countdownManager?.StartCountdown();
            timerManager?.StartTimer();

            if (thunderAttackButton != null) thunderAttackButton.gameObject.SetActive(false);

            this.enabled = false;
        }
        else
        {
            UpdateUIForLobby();

            if (thunderAttackButton != null)
            {
                thunderAttackButton.interactable = false;
                thunderAttackButton.onClick.AddListener(OnThunderAttackButtonClicked);
            }
            else
            {
                Debug.LogError("Thunder Attack Buttonが設定されていません！");
            }
        }
    }

    private void OnEnable()
    {
        if (IsSpawned)
        {
            StartHeartbeatLogic();
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    void Update()
    {
        // --- ▼▼▼ ここから変更 ▼▼▼ ---
        // isGameOver フラグもチェック条件に追加
        if (isGameOver || isDisconnectHandled || isIntentionalDisconnect || !isPlayingGame.Value)
        {
            return;
        }
        // --- ▲▲▲ ここまで変更 ▲▲▲ ---

        float lastHeartbeatTime = 0;
        if (isHostInstance)
        {
            lastHeartbeatTime = lastClientHeartbeatTime;
        }
        else if (isClientOnlyInstance)
        {
            lastHeartbeatTime = lastHostHeartbeatTime;
        }

        if (lastHeartbeatTime > 0)
        {
            float elapsedTime = Time.time - lastHeartbeatTime;

            if (elapsedTime > disconnectTimeout)
            {
                HandleLocalDisconnection();
                return;
            }
            else if (elapsedTime > disconnectTimeoutSecondPhase)
            {
                disconnectTimeoutFirstPhaseUI?.SetActive(false);
                disconnectTimeoutSecondPhaseUI?.SetActive(true);
            }
            else if (elapsedTime > disconnectTimeoutFirstPhase)
            {
                disconnectTimeoutFirstPhaseUI?.SetActive(true);
                disconnectTimeoutSecondPhaseUI?.SetActive(false);
            }
            else
            {
                disconnectTimeoutFirstPhaseUI?.SetActive(false);
                disconnectTimeoutSecondPhaseUI?.SetActive(false);
            }
        }

        if (isPlayingGame.Value)
        {
            if (friendHpGage != null)
            {
                friendHpGage.fillAmount = Mathf.Lerp(friendHpGage.fillAmount, friendHpTarget, gageSpeed * Time.deltaTime);
                if (hpGradient != null)
                {
                    friendHpGage.color = hpGradient.Evaluate(friendHpGage.fillAmount);
                }
            }

            int displayCount = localSentEnemiesCount - opponentDefeatedAttackEnemiesCount;
            UpdateAttackMarkUI(displayCount);
        }
    }

    private void UpdateAttackMarkUI(int count)
    {
        int activeCount = Mathf.Max(0, count);

        for (int i = 0; i < attackMarkIcons.Count; i++)
        {
            if (attackMarkIcons[i] != null)
            {
                attackMarkIcons[i].SetActive(i < activeCount);
            }
        }
    }

    public void IncrementLocalSentEnemiesCount()
    {
        localSentEnemiesCount++;
    }

    public void IncrementDefeatedAttackEnemiesCount()
    {
        defeatedAttackEnemiesCount++;
    }

    private void HandleLocalDisconnection()
    {
        // --- ▼▼▼ ここから変更 ▼▼▼ ---
        // ゲームオーバー後、または既に切断処理済みなら何もしない
        if (isGameOver || isDisconnectHandled || isIntentionalDisconnect) return;
        // --- ▲▲▲ ここまで変更 ▲▲▲ ---
        isDisconnectHandled = true;

        Debug.LogWarning($"[{(IsHost ? "Host" : "Client")}] 相手との通信が途絶しました。ローカルのゲームを終了します。");

        disconnectTimeoutFirstPhaseUI?.SetActive(false);
        disconnectTimeoutSecondPhaseUI?.SetActive(false);

        if (gameOverManager != null)
        {
            gameOverManager.WinWindow?.SetActive(false);
            gameOverManager.LoseWindow?.SetActive(false);
            gameOverManager.DrawWindow?.SetActive(false);
            if (gameOverManager.DisconnectedWindow != null)
            {
                gameOverManager.DisconnectedWindow.SetActive(true);
            }
            gameOverManager.stop();
        }
        else
        {
            Debug.LogError("GameOverManager への参照がありません！");
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning($"[{(IsHost ? "Host" : "Client")}] NetworkManager.Shutdown() を呼び出します。");
            NetworkManager.Singleton.Shutdown();
        }
    }

    public void NotifyIntentionalDisconnect()
    {
        Debug.Log("[Heartbeat] 意図的な切断が通知されました。");
        isIntentionalDisconnect = true;
    }
    
    // --- ▼▼▼ ここから追加 ▼▼▼ ---
    /// <summary>
    /// GameOverManagerからゲーム終了を通知してもらうためのメソッド
    /// </summary>
    public void NotifyGameOver()
    {
        isGameOver = true;
        // isPlayingGameもfalseにしておくことで、Update内の処理を確実に停止させる
        if (IsServer)
        {
            isPlayingGame.Value = false;
        }
    }
    // --- ▲▲▲ ここまで追加 ▲▲▲ ---

    public override void OnNetworkSpawn()
    {
        if (isSinglePlayer)
        {
            this.enabled = false;
            return;
        }
        base.OnNetworkSpawn();

        isPlayingGame.OnValueChanged += OnGameStartChanged;
        gameStartSignal.OnValueChanged += OnGameStartSignalChanged;
        connectedClientsCount.OnValueChanged += OnConnectedClientsCountChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedServerSide;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedServerSide;
        }

        StartHeartbeatLogic();
        UpdateUIForLobby();
    }

    private void StartHeartbeatLogic()
    {
        StopAllCoroutines();

        isHostInstance = IsHost;
        isClientOnlyInstance = IsClient && !IsHost;
        isDisconnectHandled = false;
        isIntentionalDisconnect = false;
        // --- ▼▼▼ ここから変更 ▼▼▼ ---
        isGameOver = false; // ゲーム開始時にリセット
        // --- ▲▲▲ ここまで変更 ▲▲▲ ---


        disconnectTimeoutFirstPhaseUI?.SetActive(false);
        disconnectTimeoutSecondPhaseUI?.SetActive(false);

        if (IsServer)
        {
            if (!clientReadyStatus.ContainsKey(NetworkManager.Singleton.LocalClientId))
            {
                clientReadyStatus.Add(NetworkManager.Singleton.LocalClientId, false);
            }
            UpdateConnectedClientsCount();
        }

        if (IsClient)
        {
            StartCoroutine(ClientReadyRoutine());
        }

        lastHostHeartbeatTime = Time.time;
        lastClientHeartbeatTime = Time.time;
        StartCoroutine(SendHeartbeat());

        Debug.Log("[Heartbeat] ハートビートロジックを開始/再開しました。");
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        isPlayingGame.OnValueChanged -= OnGameStartChanged;
        gameStartSignal.OnValueChanged -= OnGameStartSignalChanged;
        connectedClientsCount.OnValueChanged -= OnConnectedClientsCountChanged;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedServerSide;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedServerSide;
        }
    }

    void OnClientConnectedServerSide(ulong clientId)
    {
        if (!clientReadyStatus.ContainsKey(clientId))
        {
            clientReadyStatus.Add(clientId, false);
            UpdateConnectedClientsCount();
        }
    }

    void OnClientDisconnectedServerSide(ulong clientId)
    {
        // --- ▼▼▼ ここから変更 ▼▼▼ ---
        // ゲームオーバー後、または既に切断処理済みなら何もしない
        if (isGameOver || isDisconnectHandled)
        {
            return;
        }
        // --- ▲▲▲ ここまで変更 ▲▲▲ ---

        Debug.Log($"[Heartbeat Host] クライアント(ID: {clientId})の切断を検知しました。 isPlayingGame: {isPlayingGame.Value}");

        if (clientReadyStatus.ContainsKey(clientId))
        {
            clientReadyStatus.Remove(clientId);
        }
        UpdateConnectedClientsCount();

        if (isPlayingGame.Value)
        {
            Debug.LogWarning($"[Heartbeat Host] ゲーム中の切断を検知したため、ホスト側のゲームを終了します。");
            HandleLocalDisconnection();
        }
    }

    void UpdateConnectedClientsCount()
    {
        if (!IsServer) return;
        connectedClientsCount.Value = NetworkManager.Singleton.ConnectedClientsIds.Count;
    }

    private void OnConnectedClientsCountChanged(int previousValue, int newValue)
    {
        if (lobbyUIManager != null)
        {
            lobbyUIManager.UpdateUI();
        }
    }

    IEnumerator ClientReadyRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        SetClientReadyServerRpc(true);
    }

    [ServerRpc(RequireOwnership = false)]
    void SetClientReadyServerRpc(bool ready, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (clientReadyStatus.ContainsKey(clientId))
        {
            clientReadyStatus[clientId] = ready;
        }
    }

    public void HandleGameStartRequested()
    {
        if (IsServer && CheckIfAllReady())
        {
            gameStartSignal.Value = true;
        }
    }

    private bool CheckIfAllReady()
    {
        if (!IsServer) return false;
        var connectedIds = NetworkManager.Singleton.ConnectedClientsIds;
        if (connectedIds.Count != 2) return false;

        foreach (ulong clientId in connectedIds)
        {
            if (!clientReadyStatus.ContainsKey(clientId) || !clientReadyStatus[clientId]) return false;
        }
        return true;
    }

    private async void OnGameStartSignalChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            if (IsClient && !IsHost)
            {
                bool actuallyConnectedToRelay = RelayManager.Instance != null && RelayManager.Instance.IsClientConnected;
                if (!actuallyConnectedToRelay && LobbyServiceManager.Instance != null && LobbyServiceManager.Instance.JoinedLobby != null && RelayManager.Instance != null)
                {
                    if (LobbyServiceManager.Instance.JoinedLobby.Data.TryGetValue(LobbyServiceManager.KEY_RELAY_CODE, out DataObject relayCodeObject))
                    {
                        string relayCode = relayCodeObject.Value;
                        if (relayCode != "0")
                        {
                            actuallyConnectedToRelay = await RelayManager.Instance.JoinRelay(relayCode);
                        }
                    }
                }

                if (actuallyConnectedToRelay)
                {
                    lobbyUIManager?.TransitionToGameView();
                    ProceedToGameLogic();
                }
                else
                {
                    await LobbyServiceManager.Instance?.LeaveLobbyAsync();
                }
            }
            else if (IsHost)
            {
                lobbyUIManager?.TransitionToGameView();
                ProceedToGameLogic();
            }
        }
    }

    private void ProceedToGameLogic()
    {
        if (LobbyServiceManager.Instance != null)
        {
            LobbyServiceManager.Instance.StopPolling();
            LobbyServiceManager.Instance.NotifyGameStarted();
        }

        countdownManager?.StartCountdown();
        timerManager?.StartTimer();
        if (IsServer) isPlayingGame.Value = true;
    }

    private void OnGameStartChanged(bool previousValue, bool newValue)
    {
        friendScoreObject?.SetActive(newValue);
        UpdateAttackMarkUI(0);

        if (newValue && internetMonitor != null)
        {
            internetMonitor.ResetMonitor();
            internetMonitor.enabled = false;
        }
    }

    IEnumerator SendHeartbeat()
    {
        while (true)
        {
            yield return new WaitForSeconds(heartbeatInterval);
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) yield break;

            int currentScore = scoreAnimeManager != null ? scoreAnimeManager.Score : 0;
            int currentHP = 0;
            int maxHP = 1;
            if (localPlayerStatus != null)
            {
                currentHP = localPlayerStatus.HP;
                maxHP = localPlayerStatus.MaxHP;
            }

            if (IsHost)
            {
                UpdateClientStateClientRpc(currentScore, currentHP, maxHP, defeatedAttackEnemiesCount);
            }
            else if (IsClient)
            {
                UpdateHostStateServerRpc(currentScore, currentHP, maxHP, defeatedAttackEnemiesCount);
            }
        }
    }

    [ClientRpc]
    void UpdateClientStateClientRpc(int hostScore, int hostHP, int hostMaxHP, int hostDefeatedCount)
    {
        if (!IsHost)
        {
            lastHostHeartbeatTime = Time.time;
            if (isPlayingGame.Value)
            {
                if (friendScoreText != null) friendScoreText.text = $"{hostScore}";
                if (hostMaxHP > 0) friendHpTarget = (float)hostHP / hostMaxHP;
                opponentDefeatedAttackEnemiesCount = hostDefeatedCount;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateHostStateServerRpc(int clientScore, int clientHP, int clientMaxHP, int clientDefeatedCount, ServerRpcParams rpcParams = default)
    {
        lastClientHeartbeatTime = Time.time;
        if (IsHost && isPlayingGame.Value)
        {
            if (friendScoreText != null) friendScoreText.text = $"{clientScore}";
            if (clientMaxHP > 0) friendHpTarget = (float)clientHP / clientMaxHP;
            opponentDefeatedAttackEnemiesCount = clientDefeatedCount;
        }
    }

    private void UpdateUIForLobby()
    {
        mainCanvas?.SetActive(false);
        lobbyCanvas?.SetActive(true);
        friendScoreObject?.SetActive(false);
        UpdateAttackMarkUI(0);
        if (internetMonitor != null)
        {
            internetMonitor.enabled = true;
            internetMonitor.ResetMonitor();
        }
    }

    private void UpdateUIForGame()
    {
        mainCanvas?.SetActive(true);
        lobbyCanvas?.SetActive(false);
    }

    public void ResetHeartBeat()
    {
        StopAllCoroutines();
        if (RelayManager.Instance != null) RelayManager.Instance.Disconnect();
        else if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening) NetworkManager.Singleton.Shutdown();

        if (IsServer)
        {
            isPlayingGame.Value = false;
            gameStartSignal.Value = false;
            connectedClientsCount.Value = 0;
        }
        clientReadyStatus.Clear();

        localSentEnemiesCount = 0;
        defeatedAttackEnemiesCount = 0;
        opponentDefeatedAttackEnemiesCount = 0;

        UpdateAttackMarkUI(0);

        if (thunderAttackButton != null)
        {
            thunderAttackButton.interactable = false;
            if (buttonFlashCoroutine != null)
            {
                StopCoroutine(buttonFlashCoroutine);
                buttonFlashCoroutine = null;
                Graphic targetGraphic = thunderAttackButton.targetGraphic;
                if (targetGraphic != null)
                {
                    targetGraphic.color = new Color(targetGraphic.color.r, targetGraphic.color.g, targetGraphic.color.b, 1f);
                }
            }
        }

        disconnectTimeoutFirstPhaseUI?.SetActive(false);
        disconnectTimeoutSecondPhaseUI?.SetActive(false);
    }

    private void EnsureReferences()
    {
        if (countdownManager == null) Debug.LogError("[Heartbeat] EnsureReferences: CountdownManager が未設定！");
        if (spawnEnemyManager == null) Debug.LogError("[Heartbeat] EnsureReferences: SpawnEnemyManager が未設定！");
        if (timerManager == null) Debug.LogError("[Heartbeat] EnsureReferences: TimerManager が未設定！");
        if (scoreAnimeManager == null) Debug.LogError("[Heartbeat] EnsureReferences: ScoreAnimeManager が未設定！");
        if (lobbyUIManager == null) Debug.LogError("[Heartbeat] EnsureReferences: LobbyUIManager が未設定！");
        if (mainCanvas == null) Debug.LogError("[Heartbeat] EnsureReferences: MainCanvas が未設定！");
        if (lobbyCanvas == null) Debug.LogError("[Heartbeat] EnsureReferences: LobbyCanvas が未設定！");
        if (friendScoreObject == null) Debug.LogError("[Heartbeat] EnsureReferences: friendScoreObject が未設定！");
        if (friendScoreText == null) Debug.LogError("[Heartbeat] EnsureReferences: friendScoreText が未設定！");
        if (internetMonitor == null) Debug.LogWarning("[Heartbeat] EnsureReferences: InternetConnectionMonitor が未設定です。");
        if (disconnectTimeoutFirstPhaseUI == null) Debug.LogWarning("[Heartbeat] EnsureReferences: disconnectTimeoutFirstPhaseUI が未設定です。");
        if (disconnectTimeoutSecondPhaseUI == null) Debug.LogWarning("[Heartbeat] EnsureReferences: disconnectTimeoutSecondPhaseUI が未設定です。");
    }

    public void ActivateThunderButton()
    {
        if (thunderAttackButton != null && !thunderAttackButton.interactable)
        {
            thunderAttackButton.interactable = true;
            
            if (buttonFlashCoroutine != null)
            {
                StopCoroutine(buttonFlashCoroutine);
            }
            
            Graphic targetGraphic = thunderAttackButton.targetGraphic;
            if (targetGraphic != null)
            {
                buttonFlashCoroutine = StartCoroutine(FlashButtonRoutine(targetGraphic));
            }
        }
    }

    private void OnThunderAttackButtonClicked()
    {
        if (thunderAttackButton == null || !thunderAttackButton.interactable) return;
        
        if (buttonFlashCoroutine != null)
        {
            StopCoroutine(buttonFlashCoroutine);
            buttonFlashCoroutine = null;
            Graphic targetGraphic = thunderAttackButton.targetGraphic;
            if (targetGraphic != null)
            {
                targetGraphic.color = new Color(targetGraphic.color.r, targetGraphic.color.g, targetGraphic.color.b, 1f);
            }
        }
        
        if (AudioManager.Instance != null && thunderButtonClickSound != null)
        {
            AudioManager.Instance.PlaySFX_2D(thunderButtonClickSound, 1.0f);
        }
        
        StartCoroutine(ShowNotificationRoutine());

        thunderAttackButton.interactable = false;

        RequestThunderAttackServerRpc();
    }
    
    private IEnumerator ShowNotificationRoutine()
    {
        if (attackSentNotificationImage == null && attackSentBackgroundImage == null)
        {
            yield break;
        }
        
        if (attackSentBackgroundImage != null)
        {
            attackSentBackgroundImage.SetActive(true);
        }
        
        Coroutine flashCoroutine = null;
        if (attackSentNotificationImage != null)
        {
            flashCoroutine = StartCoroutine(FlashImageRoutine(attackSentNotificationImage, thunderFlashCount, 2.0f / (thunderFlashCount * 2)));
        }

        yield return new WaitForSeconds(2.0f);
        
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        
        if (attackSentNotificationImage != null)
        {
            attackSentNotificationImage.SetActive(false);
        }
        if (attackSentBackgroundImage != null)
        {
            attackSentBackgroundImage.SetActive(false);
        }
    }
    
    private IEnumerator FlashImageRoutine(GameObject imageObject, int flashCount, float flashDurationPerToggle)
    {
        for (int i = 0; i < flashCount; i++)
        {
            imageObject.SetActive(true);
            yield return new WaitForSeconds(flashDurationPerToggle);
            imageObject.SetActive(false);
            yield return new WaitForSeconds(flashDurationPerToggle);
        }
    }

    private IEnumerator FlashButtonRoutine(Graphic targetGraphic)
    {
        Color originalColor = targetGraphic.color;
        float minAlpha = 0.3f;
        float flashSpeed = 1.5f;

        while (true)
        {
            float t = (Mathf.Sin(Time.time * flashSpeed * Mathf.PI) + 1.0f) / 2.0f;
            float currentAlpha = Mathf.Lerp(1.0f, minAlpha, t);
            
            targetGraphic.color = new Color(originalColor.r, originalColor.g, originalColor.b, currentAlpha);
            
            yield return null;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestThunderAttackServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong attackerClientId = rpcParams.Receive.SenderClientId;

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != attackerClientId)
            {
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { clientId }
                    }
                };
                ExecuteThunderAttackClientRpc(clientRpcParams);
            }
        }
    }

    [ClientRpc]
    private void ExecuteThunderAttackClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (thunderEffectPrefab != null && opponentFieldTransforms.Count > 0)
        {
            foreach (Transform spawnPoint in opponentFieldTransforms)
            {
                if (spawnPoint != null)
                {
                    GameObject effect = Instantiate(thunderEffectPrefab, spawnPoint.position, spawnPoint.rotation);
                    Destroy(effect, 2.0f);
                }
            }
        }
        else
        {
            Debug.LogWarning("雷エフェクトのPrefabまたは表示位置が設定されていません。");
        }
        
        if (AudioManager.Instance != null && thunderSoundEffect != null)
        {
            AudioManager.Instance.PlaySFX_2D(thunderSoundEffect, thunderSoundVolume);
        }
        
        if (cameraController != null)
        {
            cameraController.TriggerThunderShake();
        }

        if (localPlayerStatus != null)
        {
            localPlayerStatus.HP -= thunderAttackDamage;
            if (localPlayerStatus.HP < 0)
            {
                localPlayerStatus.HP = 0;
            }
            Debug.Log($"雷攻撃により {thunderAttackDamage} のダメージを受けた。残りHP: {localPlayerStatus.HP}");
        }
    }
}