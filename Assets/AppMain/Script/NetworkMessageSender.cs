using UnityEngine;
using Unity.Netcode;

public class NetworkMessageSender : NetworkBehaviour
{
    public static event System.Action<Vector3> OnSpawnAttackEnemy;
     [SerializeField] private TimerManager timerManager; // TimerManagerへの参照

    public void SendMessageToClient(Vector3 position)
    {
        // 【追加】自分が（ホストが）敵を送り込んだのでカウントを増やす
        Heartbeat heartbeat = FindFirstObjectByType<Heartbeat>();
        if (heartbeat != null)
        {
            heartbeat.IncrementLocalSentEnemiesCount();
        }

        UpdateClientBoolClientRpc(position);
    }

    public void SendMessageToHost(Vector3 position)
    {
        // 【追加】自分が（クライアントが）敵を送り込んだのでカウントを増やす
        Heartbeat heartbeat = FindFirstObjectByType<Heartbeat>();
        if (heartbeat != null)
        {
            heartbeat.IncrementLocalSentEnemiesCount();
        }
        
        UpdateHostBoolServerRpc(position);
    }

    [ClientRpc]
    void UpdateClientBoolClientRpc(Vector3 position)
    {
        if (!IsHost && timerManager != null && timerManager.IsTimerStart) // timerManagerがアサインされているか、かつIsTimerStartがtrueかを確認
        {
            OnSpawnAttackEnemy?.Invoke(position);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateHostBoolServerRpc(Vector3 position)
    {
        if (IsHost && timerManager != null && timerManager.IsTimerStart) // timerManagerがアサインされているか、かつIsTimerStartがtrueかを確認
        {
            OnSpawnAttackEnemy?.Invoke(position);
        }
    }
}