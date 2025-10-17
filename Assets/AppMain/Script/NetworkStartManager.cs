using UnityEngine;
using Unity.Netcode;

public class NetworkStartManager : MonoBehaviour
{
    public void StartHost()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartHost();
            // Debug.Log("ホスト実行");
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton が null です。");
        }
    }

    public void StartClient()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartClient();
            // Debug.Log("クライアント実行");
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton が null です。");
        }
    }
}