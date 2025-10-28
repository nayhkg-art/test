using System;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LobbyServiceManager : MonoBehaviour
{
    public static LobbyServiceManager Instance { get; private set; }
    public Lobby JoinedLobby { get; private set; }
    public bool IsHost => JoinedLobby != null && JoinedLobby.HostId == AuthManager.Instance?.PlayerId;
    public event Action<Lobby> OnJoinedLobby;
    public event Action OnLeftLobby;
    public event Action<List<Lobby>> OnLobbyListUpdated;
    public const string KEY_RELAY_CODE = "RelayCode";
    public const string KEY_GAME_TYPE = "GameType";
    public const string KEY_DISPLAY_NAME = "DisplayName";
    private const float HEARTBEAT_INTERVAL = 15f;
    private const float POLLING_INTERVAL = 2.0f;
    // --- ▼▼▼ ここから変更 ▼▼▼ ---
    private const int MAX_RETRY_ATTEMPTS = 3; // 最大リトライ回数
    private const int RETRY_INTERVAL_MS = 2000; // リトライ間隔（ミリ秒）
    // --- ▲▲▲ ここまで変更 ▲▲▲ ---
    private float heartbeatTimer;
    private float pollingTimer;
    private bool isPolling = false;
    private bool isLobbyDeleting = false;
    private bool isCurrentlyCreatingLobby = false;
    private bool isGameInProgress = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        HandleHeartbeat();
        HandlePolling();
    }

    public void NotifyGameStarted()
    {
        isGameInProgress = true;
    }

    #region Lobby Actions

    public async Task LockLobbyAsync()
    {
        if (JoinedLobby == null || !IsHost) return;

        try
        {
            Debug.Log("ロビーをロックします...");
            UpdateLobbyOptions options = new UpdateLobbyOptions
            {
                IsLocked = true
            };
            JoinedLobby = await LobbyService.Instance.UpdateLobbyAsync(JoinedLobby.Id, options);
            Debug.Log("ロビーのロックに成功しました。");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"ロビーのロックに失敗しました: {e}");
        }
    }

    public async Task<Lobby> CreatePublicLobbyWithIdInNameAsync(int maxPlayers, GameType gameType)
    {
        if (AuthManager.Instance == null || !AuthManager.Instance.IsSignedIn || isLobbyDeleting || isCurrentlyCreatingLobby || JoinedLobby != null)
        {
            Debug.LogWarning("ロビー作成不可: サインインしていない、他処理実行中、または既に参加中です。");
            return null;
        }

        isCurrentlyCreatingLobby = true;

        try
        {
            Debug.Log("Relay作成開始...");
            string relayCode = await RelayManager.Instance.CreateRelay(maxPlayers - 1);
            if (string.IsNullOrEmpty(relayCode))
            {
                Debug.LogError("Relayコードの作成に失敗したため、ロビーを作成できません。");
                return null;
            }

            string tempLobbyName = "Creating Lobby...";
            CreateLobbyOptions createOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = new Player(AuthManager.Instance.PlayerId),
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayCode) },
                    { KEY_GAME_TYPE, new DataObject(DataObject.VisibilityOptions.Public, gameType.ToString()) }
                }
            };

            Lobby createdLobby = await LobbyService.Instance.CreateLobbyAsync(tempLobbyName, maxPlayers, createOptions);
            Debug.Log($"一時ロビー作成成功: {createdLobby.Name} (ID: {createdLobby.Id})");

            string lobbyIdPrefix = createdLobby.Id.Substring(0, 4);
            string finalLobbyName = $"[{gameType.ToString()}]- Room [{lobbyIdPrefix}]";
            string displayName = $"Room [ {lobbyIdPrefix} ]";

            Debug.Log($"ロビー名を「{finalLobbyName}」に更新します。");
            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions
            {
                Name = finalLobbyName,
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_DISPLAY_NAME, new DataObject(DataObject.VisibilityOptions.Public, displayName) }
                }
            };

            Lobby updatedLobby = await LobbyService.Instance.UpdateLobbyAsync(createdLobby.Id, updateOptions);
            Debug.Log($"ロビー更新成功: {updatedLobby.Name}");

            JoinedLobby = updatedLobby;
            OnJoinedLobby?.Invoke(JoinedLobby);
            return updatedLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"公開ロビー作成/更新エラー: {e.Message}");
            if (RelayManager.Instance != null) RelayManager.Instance.Disconnect();
            return null;
        }
        finally
        {
            isCurrentlyCreatingLobby = false;
        }
    }

    public async Task<Lobby> CreateLobbyAsync(string lobbyName, int maxPlayers, bool isPrivate, GameType gameType)
    {
        if (AuthManager.Instance == null || !AuthManager.Instance.IsSignedIn || isLobbyDeleting || isCurrentlyCreatingLobby || JoinedLobby != null)
        {
            Debug.LogWarning("ロビー作成不可: サインインしていない、他処理実行中、または既に参加中です。");
            return null;
        }

        isCurrentlyCreatingLobby = true;

        try
        {
            Debug.Log("ロビー作成開始...");
            string relayCode = await RelayManager.Instance.CreateRelay(maxPlayers - 1);
            if (string.IsNullOrEmpty(relayCode))
            {
                Debug.LogError("Relayコードの作成に失敗したため、ロビーを作成できません。");
                return null;
            }

            string finalLobbyName = isPrivate ? $"{gameType.ToString()}-{lobbyName}" : lobbyName;

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = new Player(AuthManager.Instance.PlayerId),
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayCode) },
                    { KEY_GAME_TYPE, new DataObject(DataObject.VisibilityOptions.Public, gameType.ToString()) },
                    { KEY_DISPLAY_NAME, new DataObject(DataObject.VisibilityOptions.Public, lobbyName) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(finalLobbyName, maxPlayers, options);
            Debug.Log($"ロビー作成成功: {lobby.Name} (ID: {lobby.Id}, Code: {lobby.LobbyCode})");

            JoinedLobby = lobby;
            OnJoinedLobby?.Invoke(JoinedLobby);
            return lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"ロビー作成エラー: {e.Message}");
            if (RelayManager.Instance != null) RelayManager.Instance.Disconnect();
            return null;
        }
        finally
        {
            isCurrentlyCreatingLobby = false;
        }
    }

    public async Task<bool> JoinLobbyByCodeAsync(string lobbyCode)
    {
        if (!AuthManager.Instance.IsSignedIn || isLobbyDeleting || JoinedLobby != null)
        {
            Debug.LogWarning("ロビー参加不可: サインインしていない、削除中、または既に参加中です。");
            return false;
        }

        // --- ▼▼▼ ここから変更 ▼▼▼ ---
        for (int i = 0; i < MAX_RETRY_ATTEMPTS; i++)
        {
            try
            {
                Debug.Log($"Lobby Code '{lobbyCode}' での参加を試みます... ({i + 1}/{MAX_RETRY_ATTEMPTS})");
                JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
                {
                    Player = new Player(AuthManager.Instance.PlayerId)
                };

                Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
                JoinedLobby = lobby;
                isPolling = true;
                Debug.Log($"Lobby Code '{lobbyCode}' での参加成功: {lobby.Name} (ID: {lobby.Id})");
                OnJoinedLobby?.Invoke(JoinedLobby);
                return true; // 成功
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"Lobby Codeでの参加に失敗しました (試行回数: {i + 1}): {e.Message}");
                if (i < MAX_RETRY_ATTEMPTS - 1)
                {
                    await Task.Delay(RETRY_INTERVAL_MS);
                }
            }
        }

        Debug.LogError("複数回の試行後もLobby Codeでの参加に失敗しました。");
        return false; // 全てのリトライが失敗
        // --- ▲▲▲ ここまで変更 ▲▲▲ ---
    }

    public async Task<bool> JoinLobbyAsync(Lobby lobbyToJoin)
    {
        if (!AuthManager.Instance.IsSignedIn || isLobbyDeleting || JoinedLobby != null)
        {
            Debug.LogWarning("ロビー参加不可: サインインしていない、削除中、または既に参加中です。");
            return false;
        }

        // --- ▼▼▼ ここから変更 ▼▼▼ ---
        for (int i = 0; i < MAX_RETRY_ATTEMPTS; i++)
        {
            try
            {
                Debug.Log($"ロビーへの参加を試みます... ({i + 1}/{MAX_RETRY_ATTEMPTS}) ID: {lobbyToJoin.Id}");
                JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
                {
                    Player = new Player(AuthManager.Instance.PlayerId)
                };

                Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyToJoin.Id, options);
                JoinedLobby = lobby;
                isPolling = true;
                Debug.Log($"ロビー参加成功: {lobby.Name} (ID: {lobby.Id})");
                OnJoinedLobby?.Invoke(JoinedLobby);
                return true; // 成功
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"参加に失敗しました (試行回数: {i + 1}): {e.Message}");
                if (i < MAX_RETRY_ATTEMPTS - 1)
                {
                    await Task.Delay(RETRY_INTERVAL_MS);
                }
            }
        }
        Debug.LogError("複数回の試行後もロビーへの参加に失敗しました。");
        return false; // 全てのリトライが失敗
        // --- ▲▲▲ ここまで変更 ▲▲▲ ---
    }

    public async Task LeaveLobbyAsync()
    {
        if (JoinedLobby == null || !AuthManager.Instance.IsSignedIn) return;

        if (IsHost)
        {
            await RemoveLobbyAsync();
            return;
        }

        try
        {
            string lobbyId = JoinedLobby.Id;
            string playerId = AuthManager.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, playerId);
            ResetLobbyState("自発的に退出しました。");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"ロビー退出エラー: {e.Message}");
            ResetLobbyState($"退出エラー ({e.Reason}) によりリセット。");
        }
    }

    public async Task RemoveLobbyAsync()
    {
        if (JoinedLobby == null || !AuthManager.Instance.IsSignedIn || !IsHost || isLobbyDeleting) return;

        isLobbyDeleting = true;
        try
        {
            string lobbyId = JoinedLobby.Id;
            await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
            ResetLobbyState("ロビーを削除しました。");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"ロビー削除エラー: {e.Message}");
        }
        finally
        {
            isLobbyDeleting = false;
        }
    }

    public async Task<List<Lobby>> RefreshLobbyListAsync(GameType gameType)
    {
        if (!AuthManager.Instance.IsSignedIn) return new List<Lobby>();

        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Order = new List<QueryOrder>
                {
                    new QueryOrder(asc: true, field: QueryOrder.FieldOptions.Name)
                },
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0"),
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.IsLocked,
                        op: QueryFilter.OpOptions.EQ,
                        value: "0"),
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.Name,
                        op: QueryFilter.OpOptions.CONTAINS,
                        value: $"[{gameType.ToString()}]"
                    )
                }
            };

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
            OnLobbyListUpdated?.Invoke(response.Results);
            return response.Results;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"ロビーリスト更新エラー: {e.Message}");
            OnLobbyListUpdated?.Invoke(new List<Lobby>());
            return new List<Lobby>();
        }
    }

    #endregion

    #region Heartbeat & Polling
    private async void HandleHeartbeat()
    {
        if (JoinedLobby != null && IsHost && AuthManager.Instance.IsSignedIn)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0)
            {
                heartbeatTimer = HEARTBEAT_INTERVAL;
                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(JoinedLobby.Id);
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogWarning($"ハートビート送信失敗: {e.Message}");
                    
                    if (!isGameInProgress)
                    {
                        ResetLobbyState("ロビー待機中のハートビート失敗によりリセット。");
                    }
                    else
                    {
                        Debug.LogWarning("ゲームプレイ中のため、ロビーのハートビート失敗によるリセットはスキップします。");
                    }
                }
            }
        }
    }
    
    public void StopPolling()
    {
        isPolling = false;
        Debug.Log("[LobbyServiceManager] ゲーム開始のため、ロビーのポーリングを停止しました。");
    }

    private async void HandlePolling()
    {
        if (!isPolling || JoinedLobby == null || AuthManager.Instance == null || !AuthManager.Instance.IsSignedIn || IsHost)
        {
            return;
        }

        pollingTimer -= Time.deltaTime;
        if (pollingTimer <= 0)
        {
            pollingTimer = POLLING_INTERVAL;
            try
            {
                string currentLobbyId = JoinedLobby?.Id;
                if (string.IsNullOrEmpty(currentLobbyId))
                {
                    isPolling = false;
                    return;
                }
                Lobby newLobbyState = await LobbyService.Instance.GetLobbyAsync(currentLobbyId);

                if (JoinedLobby == null || JoinedLobby.Id != newLobbyState.Id)
                {
                    isPolling = false;
                    return;
                }
                JoinedLobby = newLobbyState;

                if (JoinedLobby.Data != null && RelayManager.Instance != null)
                {
                    if (JoinedLobby.Data.TryGetValue(KEY_RELAY_CODE, out DataObject relayCodeObject))
                    {
                        string relayCode = relayCodeObject.Value;
                        if (relayCode != "0" && !RelayManager.Instance.IsClientConnected)
                        {
                            bool joinedRelay = await RelayManager.Instance.JoinRelay(relayCode);
                            if (!joinedRelay)
                            {
                                if (JoinedLobby != null) await LeaveLobbyAsync();
                            }
                        }
                    }
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"ポーリングエラー: {e.Message} (Reason: {e.Reason})");
                if (e.Reason == LobbyExceptionReason.LobbyNotFound || e.Reason == LobbyExceptionReason.PlayerNotFound)
                {
                    ResetLobbyState($"ポーリングエラー ({e.Reason}) によりリセット。");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"予期せぬポーリングエラー: {ex.Message}");
                isPolling = false;
            }
        }
    }
    #endregion

    private void ResetLobbyState(string message)
    {
        Debug.Log($"ロビー状態をリセット: {message}");
        JoinedLobby = null;
        isPolling = false;
        isGameInProgress = false;
        heartbeatTimer = 0;
        pollingTimer = 0;
        if (RelayManager.Instance != null) RelayManager.Instance.Disconnect();
        OnLeftLobby?.Invoke();
    }

    private void OnApplicationQuit()
    {
        if (JoinedLobby != null && AuthManager.Instance.IsSignedIn)
        {
            if (IsHost)
            {
                _ = RemoveLobbyAsync();
            }
            else
            {
                _ = LeaveLobbyAsync();
            }
        }
    }
}