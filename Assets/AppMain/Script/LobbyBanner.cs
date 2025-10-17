using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro; // TMP_InputField などを使っている場合、これも必要

public class LobbyBanner : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button joinButton;

    public Lobby MyLobby { get; private set; }
    public event Action<Lobby> OnJoinClicked;

    // public void Init(Lobby lobby)
    // {
    //     MyLobby = lobby;
    //     lobbyNameText.text = lobby.Name;
    //     playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";

    //     joinButton.onClick.RemoveAllListeners();
    //     joinButton.onClick.AddListener(() =>
    //     {
    //         OnJoinClicked?.Invoke(MyLobby);
    //     });
    // }
    public void Init(Lobby lobby)
{
    MyLobby = lobby;

    // まず "DisplayName" がデータに含まれているか確認
    if (lobby.Data != null && lobby.Data.TryGetValue(LobbyServiceManager.KEY_DISPLAY_NAME, out DataObject displayNameObject))
    {
        // あればその値を表示名として使う
        lobbyNameText.text = displayNameObject.Value;
    }
    else
    {
        // なければ、念のため従来の lobby.Name を表示（フォールバック）
        lobbyNameText.text = lobby.Name;
    }

    playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";

    joinButton.onClick.RemoveAllListeners();
    joinButton.onClick.AddListener(() =>
    {
        OnJoinClicked?.Invoke(MyLobby);
    });
}

    public void UpdateButtons(Lobby currentJoinedLobby, bool isPlayerHost)
    {
        if (MyLobby == null) return;

        bool canJoin = currentJoinedLobby == null && MyLobby.Players.Count < MyLobby.MaxPlayers;
        joinButton.gameObject.SetActive(canJoin);
        joinButton.interactable = canJoin;
    }

    private void OnDestroy()
    {
        OnJoinClicked = null;
    }
}