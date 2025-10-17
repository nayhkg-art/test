using Unity.Netcode;
using UnityEngine;
using TMPro;

public class StatusConnect : MonoBehaviour
{
    public TMP_Text statusText;

    void Update()
    {
        if (NetworkManager.Singleton == null)
        {
            statusText.text = "null Singleton";
            return;
        }

        if (NetworkManager.Singleton.IsHost)
        {
            statusText.text = "Host";
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            statusText.text = "Client";
        }
        else
        {
            statusText.text = "NotHostClient";
        }
    }
}
