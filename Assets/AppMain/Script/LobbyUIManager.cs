using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using TMPro;
using System.Threading.Tasks;

public class LobbyUIManager : MonoBehaviour
{
    [Header("Manager references")]
    [SerializeField] private AuthManager authManager;
    [SerializeField] private LobbyServiceManager lobbyServiceManager;
    private AudioManager audioManager;

    [Header("UI Elements")]
    [SerializeField] private Heartbeat heartbeat;
    [SerializeField] private GameObject waitStartView;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI waitPanelMessageText;
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject lobbyCanvas;
    [SerializeField] private Button hostStartGameButton;
    [SerializeField] private Button hostRemoveLobbyButton;
    [SerializeField] private Button clientLeaveLobbyButton;
    [SerializeField] private Button retrySignInButton;
    [SerializeField] private InternetConnectionMonitor internetMonitor;
    [SerializeField] private TextMeshProUGUI selectedGameTypeText;

    [Header("Connection Status UI")]
    [SerializeField] private GameObject connectionWarningPanel;
    [SerializeField] private Button closeWarningButton;

    [Header("Public Lobby Area")]
    [SerializeField] private GameObject publicLobbyArea;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private LobbyListView lobbyListView;

    [Header("Private Lobby Area")]
    [SerializeField] private Button friendsOnlyButton;
    [SerializeField] private GameObject privateLobbyArea;
    [SerializeField] private Button createPrivateLobbyButton;
    [SerializeField] private TMP_InputField lobbyCodeInput;
    [SerializeField] private Button joinByCodeButton;
    [SerializeField] private Button backFromPrivateAreaButton;

    [Header("Host Wait Panel")]
    [SerializeField] private GameObject hostWaitPanel;
    [SerializeField] private TextMeshProUGUI lobbyCodeDisplayText;
    [SerializeField] private Button backFromHostWaitButton;

    [Header("Settings")]
    [SerializeField] private float lobbyListRefreshInterval = 2.0f;
    private float lobbyListRefreshTimer = 0f;
    private bool shouldAutoRefresh = false;

    [Header("Debug UI Elements")]
    [SerializeField] private Button debugSignOutButton;

    private bool isProcessing = false;
    private bool isShowingPrivateArea = false;
    private bool hasConnectionTimedOut = false;
    private bool isWaitingAsPrivateHost = false;

    private void Awake()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
    }

    private void OnEnable()
    {
        isShowingPrivateArea = false;
        isWaitingAsPrivateHost = false;
        hasConnectionTimedOut = false;

        if (createLobbyButton != null) createLobbyButton.onClick.AddListener(OnCreateLobbyClicked);
        if (hostRemoveLobbyButton != null) hostRemoveLobbyButton.onClick.AddListener(OnHostRemoveLobbyClicked);
        if (hostStartGameButton != null) hostStartGameButton.onClick.AddListener(OnHostStartGameClicked);
        if (clientLeaveLobbyButton != null) clientLeaveLobbyButton.onClick.AddListener(OnClientLeaveLobbyClicked);
        if (retrySignInButton != null) retrySignInButton.onClick.AddListener(OnRetrySignInClicked);
        if (debugSignOutButton != null) debugSignOutButton.onClick.AddListener(OnDebugSignOutClicked);
        if (friendsOnlyButton != null) friendsOnlyButton.onClick.AddListener(OnFriendsOnlyClicked);
        if (createPrivateLobbyButton != null) createPrivateLobbyButton.onClick.AddListener(OnCreatePrivateLobbyClicked);
        if (joinByCodeButton != null) joinByCodeButton.onClick.AddListener(OnJoinByCodeClicked);
        if (backFromPrivateAreaButton != null) backFromPrivateAreaButton.onClick.AddListener(OnBackFromPrivateAreaClicked);
        if (backFromHostWaitButton != null) backFromHostWaitButton.onClick.AddListener(OnBackFromHostWaitClicked);
        if (closeWarningButton != null) closeWarningButton.onClick.AddListener(OnCloseWarningClicked);

        if (lobbyListView != null)
        {
            lobbyListView.OnRefreshRequested += OnRefreshRequestedWrapper;
            lobbyListView.OnJoinRequested += OnJoinRequestedWrapper;
        }

        if (internetMonitor != null) internetMonitor.OnConnectionTimeout += HandleConnectionTimeout;
        if (authManager != null)
        {
            authManager.OnSignInFailed += HandleSignInFailed;
            authManager.OnInitialized += UpdateUI;
            authManager.OnSignedIn += UpdateUI;
            authManager.OnSignedOut += UpdateUI;
        }
        if (lobbyServiceManager != null)
        {
            lobbyServiceManager.OnJoinedLobby += HandleJoinedLobby;
            lobbyServiceManager.OnLeftLobby += HandleLeftLobby;
            lobbyServiceManager.OnLobbyListUpdated += HandleLobbyListUpdated;
        }

        UpdateUI();
    }

    private void OnDisable()
    {
        if (createLobbyButton != null) createLobbyButton.onClick.RemoveListener(OnCreateLobbyClicked);
        if (hostRemoveLobbyButton != null) hostRemoveLobbyButton.onClick.RemoveListener(OnHostRemoveLobbyClicked);
        if (hostStartGameButton != null) hostStartGameButton.onClick.RemoveListener(OnHostStartGameClicked);
        if (clientLeaveLobbyButton != null) clientLeaveLobbyButton.onClick.RemoveListener(OnClientLeaveLobbyClicked);
        if (retrySignInButton != null) retrySignInButton.onClick.RemoveListener(OnRetrySignInClicked);
        if (debugSignOutButton != null) debugSignOutButton.onClick.RemoveListener(OnDebugSignOutClicked);
        if (friendsOnlyButton != null) friendsOnlyButton.onClick.RemoveListener(OnFriendsOnlyClicked);
        if (createPrivateLobbyButton != null) createPrivateLobbyButton.onClick.RemoveListener(OnCreatePrivateLobbyClicked);
        if (joinByCodeButton != null) joinByCodeButton.onClick.RemoveListener(OnJoinByCodeClicked);
        if (backFromPrivateAreaButton != null) backFromPrivateAreaButton.onClick.RemoveListener(OnBackFromPrivateAreaClicked);
        if (backFromHostWaitButton != null) backFromHostWaitButton.onClick.RemoveListener(OnBackFromHostWaitClicked);
        if (closeWarningButton != null) closeWarningButton.onClick.RemoveListener(OnCloseWarningClicked);

        if (lobbyListView != null)
        {
            lobbyListView.OnRefreshRequested -= OnRefreshRequestedWrapper;
            lobbyListView.OnJoinRequested -= OnJoinRequestedWrapper;
        }

        if (internetMonitor != null) internetMonitor.OnConnectionTimeout -= HandleConnectionTimeout;
        if (authManager != null)
        {
            authManager.OnSignInFailed -= HandleSignInFailed;
            authManager.OnInitialized -= UpdateUI;
            authManager.OnSignedIn -= UpdateUI;
            authManager.OnSignedOut -= UpdateUI;
        }
        if (lobbyServiceManager != null)
        {
            lobbyServiceManager.OnJoinedLobby -= HandleJoinedLobby;
            lobbyServiceManager.OnLeftLobby -= HandleLeftLobby;
            lobbyServiceManager.OnLobbyListUpdated -= HandleLobbyListUpdated;
        }
    }

    private void PlayClickSound()
    {
        if (audioManager != null)
        {
            audioManager.PlayClickSound();
        }
    }

    // Wrapper methods for adding click sound
    private void OnCreateLobbyClicked() { PlayClickSound(); HandleCreatePublicLobbyClick(); }
    private void OnHostRemoveLobbyClicked() { PlayClickSound(); HandleRemoveClick(); }
    private void OnHostStartGameClicked() { PlayClickSound(); HandleHostStartGameClick(); }
    private void OnClientLeaveLobbyClicked() { PlayClickSound(); HandleClientLeaveClick(); }
    private void OnRetrySignInClicked() { PlayClickSound(); HandleRetrySignInClick(); }
    private void OnDebugSignOutClicked() { PlayClickSound(); HandleDebugSignOutClick(); }
    private void OnFriendsOnlyClicked() { PlayClickSound(); ShowPrivateLobbyArea(); }
    private void OnCreatePrivateLobbyClicked() { PlayClickSound(); HandleCreatePrivateLobbyClick(); }
    private void OnJoinByCodeClicked() { PlayClickSound(); HandleJoinByCodeClick(); }
    private void OnBackFromPrivateAreaClicked() { PlayClickSound(); ShowPublicLobbyArea(); }
    private void OnBackFromHostWaitClicked() { PlayClickSound(); HandleRemoveClick(); }
    private void OnCloseWarningClicked() { PlayClickSound(); HideConnectionWarning(); }
    private void OnRefreshRequestedWrapper() { PlayClickSound(); HandleRefreshClick(); }
    private void OnJoinRequestedWrapper(Lobby lobby) { PlayClickSound(); HandleJoinClick(lobby); }

    private void HideConnectionWarning()
    {
        if (connectionWarningPanel != null)
        {
            connectionWarningPanel.SetActive(false);
        }
    }

    private void ShowPublicLobbyArea() { isShowingPrivateArea = false; UpdateUI(); }
    private void ShowPrivateLobbyArea() { isShowingPrivateArea = true; UpdateUI(); }

    private async void HandleCreatePublicLobbyClick()
    {
        isProcessing = true;
        UpdateUI();
        SetStatus("公開ロビーを作成中...");
        try { await lobbyServiceManager.CreatePublicLobbyWithIdInNameAsync(2, GameSelectionManager.SelectedGameType); }
        catch (Exception e) { Debug.LogError($"公開ロビー操作中にエラーが発生しました: {e.Message}"); SetStatus("ロビー作成に失敗しました。"); }
        finally { isProcessing = false; UpdateUI(); }
    }

    private async void HandleCreatePrivateLobbyClick()
    {
        isProcessing = true;
        UpdateUI();
        SetStatus("非公開ロビーを作成中...");
        Lobby createdLobby = null;
        try
        {
            if (GameSelectionManager.SelectedGameType == GameType.None)
            {
                Debug.LogError("ゲームタイプが選択されていません。プライベートロビーを作成できません。");
                SetStatus("ロビー作成に失敗しました: ゲームタイプが未選択です。");
                isProcessing = false;
                UpdateUI();
                return;
            }
            createdLobby = await lobbyServiceManager.CreateLobbyAsync("Room Name", 2, true, GameSelectionManager.SelectedGameType);
        }
        catch (Exception e) { Debug.LogError($"非公開ロビー作成中にエラーが発生しました: {e.Message}"); SetStatus("ロビー作成に失敗しました。"); }
        finally { if (createdLobby == null) { isProcessing = false; UpdateUI(); } }

        if (createdLobby != null)
        {
            isWaitingAsPrivateHost = true;
            if (lobbyCodeDisplayText != null)
            {
                lobbyCodeDisplayText.text = $"Tell your friends about this code!:\n<size=120%><color=#FFC107>{createdLobby.LobbyCode}</color></size>";
            }
            SetStatus($"非公開ロビーを作成しました。コード: {createdLobby.LobbyCode}");
        }
        UpdateUI();
    }

    private async void HandleJoinByCodeClick()
    {
        if (lobbyCodeInput == null || string.IsNullOrWhiteSpace(lobbyCodeInput.text))
        {
            SetStatus("Lobby Code を入力してください。");
            return;
        }

        isProcessing = true;
        SetStatus("ロビーに接続中...");
        UpdateUI();

        bool success = await lobbyServiceManager.JoinLobbyByCodeAsync(lobbyCodeInput.text.Trim().ToUpper());
        
        isProcessing = false;
        if (!success)
        {
            if (connectionWarningPanel != null) connectionWarningPanel.SetActive(true);
        }
        
        UpdateUI();
    }

    private async Task ProcessLobbyTask(Task task)
    {
        isProcessing = true;
        UpdateUI();
        try { await task; }
        catch (Exception e) { Debug.LogError($"ロビー操作中にエラーが発生しました: {e.Message}"); }
        finally { isProcessing = false; UpdateUI(); }
    }

    private void Update()
    {
        if (shouldAutoRefresh && !isProcessing)
        {
            lobbyListRefreshTimer -= Time.deltaTime;
            if (lobbyListRefreshTimer <= 0)
            {
                lobbyListRefreshTimer = lobbyListRefreshInterval;
                _ = lobbyServiceManager.RefreshLobbyListAsync(GameSelectionManager.SelectedGameType);
            }
        }
    }

    public void UpdateUI()
    {
        if (authManager == null || !authManager.IsInitialized)
        {
            publicLobbyArea?.SetActive(false);
            privateLobbyArea?.SetActive(false);
            hostWaitPanel?.SetActive(false);
            waitStartView?.SetActive(false);
            retrySignInButton?.gameObject.SetActive(false);
            SetStatus("認証サービスを初期化中...");
            return;
        }

        bool isSignedIn = authManager.IsSignedIn;
        bool inLobby = lobbyServiceManager != null && lobbyServiceManager.JoinedLobby != null;
        int clientCount = (heartbeat != null && inLobby) ? heartbeat.connectedClientsCount.Value : 0;

        if (clientCount >= 2) isWaitingAsPrivateHost = false;

        if (isWaitingAsPrivateHost)
        {
            publicLobbyArea?.SetActive(false);
            privateLobbyArea?.SetActive(false);
            waitStartView?.SetActive(false);
            hostWaitPanel?.SetActive(true);
        }
        else if (inLobby)
        {
            publicLobbyArea?.SetActive(false);
            privateLobbyArea?.SetActive(false);
            hostWaitPanel?.SetActive(false);
            waitStartView?.SetActive(true);

            bool isHost = lobbyServiceManager.IsHost;
            hostRemoveLobbyButton?.gameObject.SetActive(isHost);
            clientLeaveLobbyButton?.gameObject.SetActive(!isHost);

            if (hostStartGameButton != null)
            {
                if (isHost)
                {
                    bool showStartButton = (clientCount == 2);
                    hostStartGameButton.gameObject.SetActive(showStartButton);
                    hostStartGameButton.interactable = showStartButton;
                }
                else
                {
                    hostStartGameButton.gameObject.SetActive(false);
                }
            }

            if (waitPanelMessageText != null)
            {
                waitPanelMessageText.gameObject.SetActive(true);
                waitPanelMessageText.text = isHost
                    ? (hostStartGameButton.gameObject.activeSelf && hostStartGameButton.interactable ? "Press the Start button" : "Waiting for player to join...")
                    : "Waiting for host to press\n the start button...";
            }
        }
        else
        {
            publicLobbyArea?.SetActive(isSignedIn && !isShowingPrivateArea);
            privateLobbyArea?.SetActive(isSignedIn && isShowingPrivateArea);

            if (!isSignedIn)
            {
                publicLobbyArea?.SetActive(false);
                privateLobbyArea?.SetActive(false);
                if (retrySignInButton != null && hasConnectionTimedOut)
                {
                    retrySignInButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (!isShowingPrivateArea)
                {
                    publicLobbyArea?.SetActive(true);
                    privateLobbyArea?.SetActive(false);
                }
                else
                {
                    publicLobbyArea?.SetActive(false);
                    privateLobbyArea?.SetActive(true);
                }
            }
            hostWaitPanel?.SetActive(false);
            waitStartView?.SetActive(false);
        }

        bool canInteract = !isProcessing && !inLobby;
        if (createLobbyButton != null) createLobbyButton.interactable = canInteract;
        if (friendsOnlyButton != null) friendsOnlyButton.interactable = canInteract;
        if (createPrivateLobbyButton != null) createPrivateLobbyButton.interactable = canInteract;
        if (joinByCodeButton != null) joinByCodeButton.interactable = canInteract;
        lobbyListView?.SetRefreshButtonInteractable(canInteract && !isShowingPrivateArea);

        if (selectedGameTypeText != null)
        {
            if (GameSelectionManager.SelectedGameType != GameType.None)
            {
                selectedGameTypeText.text = GetGameTypeNameInJapanese(GameSelectionManager.SelectedGameType);
                selectedGameTypeText.gameObject.SetActive(true);
            }
            else
            {
                selectedGameTypeText.gameObject.SetActive(false);
            }
        }

        shouldAutoRefresh = publicLobbyArea != null && publicLobbyArea.activeSelf;
        UpdateStatusText(isSignedIn, inLobby);
    }

    private string GetGameTypeNameInJapanese(GameType gameType)
    {
        switch (gameType)
        {
            case GameType.None: return "ゲーム未選択";
            case GameType.JidoushiTadoushi: return "じどうし・たどうし";
            case GameType.Keigo: return "けいご";
            case GameType.Hiragana: return "ひらがな";
            case GameType.Katakana: return "カタカナ";
            case GameType.Yohoon: return "バビブ・キャキュキョ";
            case GameType.KatakanaEigo: return "カタカナえいご";
            case GameType.Hinshi: return "ひんし";
            case GameType.Group: return "なんグループ";
            case GameType.FirstKanji: return "かんじ１ねんせい";
            default: return gameType.ToString();
        }
    }

    private void UpdateStatusText(bool isSignedIn, bool inLobby)
    {
        if (isProcessing) return;
        if (inLobby) { SetStatus($"ロビー: {lobbyServiceManager.JoinedLobby.Name}"); }
        else if (isSignedIn)
        {
            if (isShowingPrivateArea) SetStatus("非公開ロビーを作成するかコードで参加してください。");
            else SetStatus("ロビーを選択または作成してください。");
        }
        else
        {
            if (retrySignInButton != null && retrySignInButton.gameObject.activeSelf)
                SetStatus("サインインに失敗しました。\n接続を確認して再試行してください。");
            else SetStatus("サインイン中...");
        }
    }

    private void HandleJoinedLobby(Lobby lobby)
    {
        if (heartbeat != null)
        {
            heartbeat.enabled = true;
        }
        UpdateUI();
    }

    private void HandleLeftLobby()
    {
        Debug.Log("[LobbyUIManager] ロビーから退出しました。Heartbeatに通知し、ネットワークをリセットします。");
        if (heartbeat != null)
        {
            heartbeat.NotifyIntentionalDisconnect();
            heartbeat.enabled = false;
            heartbeat.ResetHeartBeat();
        }

        isShowingPrivateArea = false;
        isWaitingAsPrivateHost = false;
        UpdateUI();
    }

    private void HandleLobbyListUpdated(List<Lobby> lobbies)
    {
        if (lobbyServiceManager.JoinedLobby == null)
        {
            List<Lobby> filteredLobbies = new List<Lobby>();
            GameType selectedGameType = GameSelectionManager.SelectedGameType;
            string filterString = $"[{selectedGameType.ToString()}]";
            foreach (Lobby lobby in lobbies)
            {
                if (lobby.Name.Contains(filterString))
                {
                    filteredLobbies.Add(lobby);
                }
            }
            lobbyListView.Refresh(filteredLobbies, null, false);
        }
    }
    private void HandleSignInFailed() { UpdateUI(); }
    private void HandleConnectionTimeout() { hasConnectionTimedOut = true; UpdateUI(); }

    private async void HandleRemoveClick() { await ProcessLobbyTask(lobbyServiceManager.RemoveLobbyAsync()); }

    private async void HandleHostStartGameClick()
    {
        if (heartbeat == null || lobbyServiceManager == null) return;
        isProcessing = true;
        UpdateUI();
        await lobbyServiceManager.LockLobbyAsync();
        heartbeat.HandleGameStartRequested();
        isProcessing = false;
    }

    private async void HandleClientLeaveClick()
    {
        Debug.Log("[LobbyUIManager] クライアントが退出ボタンを押しました。");
        if (heartbeat != null)
        {
            heartbeat.NotifyIntentionalDisconnect();
            heartbeat.enabled = false;
            heartbeat.ResetHeartBeat();
        }
        
        await ProcessLobbyTask(lobbyServiceManager.LeaveLobbyAsync());
    }

    private async void HandleRefreshClick() { await ProcessLobbyTask(lobbyServiceManager.RefreshLobbyListAsync(GameSelectionManager.SelectedGameType)); }
    private async void HandleJoinClick(Lobby lobby)
    {
        if (isProcessing) return;

        isProcessing = true;
        lobbyListView.SetAllJoinButtonsInteractable(false);
        SetStatus("ロビーに接続中...");
        UpdateUI();

        bool success = false;
        try
        {
            success = await lobbyServiceManager.JoinLobbyAsync(lobby);
        }
        catch (Exception e)
        {
            Debug.LogError($"ロビー参加処理中に予期せぬエラーが発生しました: {e.Message}");
            success = false;
        }
        finally
        {
            isProcessing = false;
            if (!success)
            {
                if (connectionWarningPanel != null) connectionWarningPanel.SetActive(true);
                lobbyListView.SetAllJoinButtonsInteractable(true);
            }
            UpdateUI();
        }
    }
    private async void HandleRetrySignInClick()
    {
        hasConnectionTimedOut = false;
        await ProcessLobbyTask(authManager.AttemptSignInAsync());
    }
    private void HandleDebugSignOutClick() { authManager.DebugForceSignOutAsync(); }
    private void SetStatus(string message) { if (statusText != null) statusText.text = message; }

    public void TransitionToGameView()
    {
        isWaitingAsPrivateHost = false;
        if (audioManager != null)
        {
            audioManager.StopAllSounds();
        }
        lobbyCanvas?.SetActive(false);
        mainCanvas?.SetActive(true);
        shouldAutoRefresh = false;
    }
}