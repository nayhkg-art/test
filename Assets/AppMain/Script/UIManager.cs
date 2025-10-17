using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button startHostButton;
    public Button startClientButton;
    public Button disconnectButton;
    public NetworkMessageSender networkMessageSender;
    private NetworkStartManager networkStartManager;

    void Start()
    {
        networkStartManager = FindAnyObjectByType<NetworkStartManager>();

        startHostButton.onClick.AddListener(OnStartHostButtonClicked);
        startClientButton.onClick.AddListener(OnStartClientButtonClicked);
        disconnectButton.onClick.AddListener(OnDisconnectButtonClicked);
    }

    void OnStartHostButtonClicked()
    {
        networkStartManager.StartHost();
    }

    void OnStartClientButtonClicked()
    {
        networkStartManager.StartClient();
    }

    void OnDisconnectButtonClicked()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("離脱しました");
        }
        else
        {
            Debug.Log("離脱済み");
        }
    }
}

