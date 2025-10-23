using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LobbyListView : MonoBehaviour
{
    [SerializeField] private GameObject lobbyBannerPrefab;
    [SerializeField] private Transform lobbyListContent;
    [SerializeField] private Button refreshButton;

    public event Action OnRefreshRequested;
    public event Action<Lobby> OnJoinRequested;
    private List<LobbyBanner> currentBanners = new List<LobbyBanner>();

    private void Awake()
    {
        foreach (Transform child in lobbyListContent)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnEnable()
    {
        refreshButton.onClick.AddListener(() => OnRefreshRequested?.Invoke());
    }

    private void OnDisable()
    {
        refreshButton.onClick.RemoveAllListeners();
    }

    public void Refresh(List<Lobby> lobbyList, Lobby currentJoinedLobby, bool isPlayerHost)
    {
        foreach (var banner in currentBanners)
        {
            if (banner != null) Destroy(banner.gameObject);
        }
        currentBanners.Clear();

        foreach (var lobby in lobbyList)
        {
            if (lobby == null) continue;

            GameObject bannerObj = Instantiate(lobbyBannerPrefab, lobbyListContent);
            LobbyBanner bannerComponent = bannerObj.GetComponent<LobbyBanner>();

            if (bannerComponent != null)
            {
                bannerComponent.Init(lobby);
                bannerComponent.UpdateButtons(currentJoinedLobby, isPlayerHost);
                bannerComponent.OnJoinClicked += (lobbyToJoin) => OnJoinRequested?.Invoke(lobbyToJoin);
                currentBanners.Add(bannerComponent);
            }
        }
    }

    public void UpdateAllBanners(Lobby currentJoinedLobby, bool isPlayerHost)
    {
        foreach (var banner in currentBanners)
        {
            if (banner != null)
            {
                banner.UpdateButtons(currentJoinedLobby, isPlayerHost);
            }
        }
    }

    public void SetRefreshButtonInteractable(bool interactable)
    {
        refreshButton.interactable = interactable;
    }

    public void SetAllJoinButtonsInteractable(bool interactable)
    {
        foreach (var banner in currentBanners)
        {
            if (banner != null)
            {
                banner.SetJoinButtonInteractable(interactable);
            }
        }
    }
}