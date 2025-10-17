using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

//Relayで、Unity Netcode for GameObjects が使用できるようにする準備とその通信の開始（ホストまたはクライアントとして）
public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }
    // [SerializeField] private NetworkManager networkManager;
    private NetworkManager networkManager;
    // 現在のインスタンスがクライアントとして接続済みかどうかを返します。
    public bool IsClientConnected => networkManager.IsClient && networkManager.IsConnectedClient;
    // 現在のインスタンスがホストであるかどうかを返します。
    public bool IsHost => networkManager.IsHost;

    private void Awake()
    {
        // ★★★ シーンロード時に毎回 Singleton を取得し直す ★★★
        networkManager = NetworkManager.Singleton;
        if (networkManager == null)
        {
            Debug.LogError("[RelayManager] Awake: NetworkManager.Singleton が見つかりません！");
        }
        else
        {
            Debug.Log($"[RelayManager] Awake: NetworkManager を取得しました (ID: {networkManager.GetInstanceID()})");
        }
        // もし既に他のインスタンスが存在する場合、このオブジェクトは破棄されます。
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // このゲームオブジェクトをシーン間で破棄されないようにします。
        // DontDestroyOnLoad(gameObject);

        // NetworkManagerが設定されているか確認。
        if (networkManager == null)
        {
            if (networkManager == null)
            {
                // NetworkManager が見つからない場合はエラーログを出力します。
                Debug.LogError("RelayManager: NetworkManager が見つかりません！");
            }
        }
    }

    /// <summary>
    /// Unity Relay サービスに新しい Relay セッションの作成を要求し、ホストとして Netcode を開始します。
    /// </summary>
    /// <param name="maxConnections">ホストを除く、最大接続可能なクライアントの数（例: 2人プレイなら1を設定）。</param>
    /// <returns>成功した場合はクライアントが参加するための Relay Code、失敗した場合は null。</returns>
    public async Task<string> CreateRelay(int maxConnections = 1) // 2人プレイの場合、ホスト以外の接続数は「1」になります。
    {
        try
        {
            //（ロビーを作成した後）Relayサービスに頼んで「ゲームの通信専用回線（Allocation）」を確保してもらう。これは、誰もが使える回線ではなく、あなたのゲーム専用の回線。
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            //その確保された「専用回線（Allocation）」にアクセスするための「合言葉（Relay Code）」を生成してもらう。
            // この「合言葉」をみんなに教えれば、その回線につながることができる。
            string relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Relay 作成成功: Relay Code = {relayCode}");

            // NetworkManager が使用するトランスポート層（通信方法）を取得します。
            UnityTransport transport = networkManager.GetComponent<UnityTransport>();
            // ホストとして Relay サーバーに接続するための情報をトランスポートに設定します。
            // これにより、Netcode は Relay を経由して通信できるようになります。
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4, // Relay サーバーの IPv4 アドレス
                (ushort)allocation.RelayServer.Port, // Relay サーバーのポート番号
                allocation.AllocationIdBytes, // 割り当ての一意なID
                allocation.Key,               // 暗号化キー
                allocation.ConnectionData     // ホスト側の接続データ
            );
            Debug.Log($"[RelayManager] StartHost を呼び出します。現在の IsServer: {networkManager.IsServer}, IsHost: {networkManager.IsHost}, IsListening: {networkManager.IsListening}");
            // Netcode のホストとしてゲームを開始します。
            networkManager.StartHost();
            // ★★★ StartHost 直後の状態をログに出力 ★★★
            Debug.Log($"[RelayManager] Netcode ホストを開始しました。IsServer: {NetworkManager.Singleton.IsServer}, IsHost: {NetworkManager.Singleton.IsHost}");
            return relayCode; // 生成された Relay Code を返します。
        }
        catch (RelayServiceException e)
        {
            // Relay サービス固有のエラーが発生した場合の処理。
            Debug.LogError($"Relay 作成エラー: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// ホストから共有された Relay Code を使用して Relay セッションに参加し、クライアントとして Netcode を開始します。
    /// </summary>
    /// <param name="relayCode">参加したい Relay セッションの Relay Code。</param>
    /// <returns>成功した場合は true、失敗した場合は false。</returns>
    public async Task<bool> JoinRelay(string relayCode)
    {
        // 参加試行開始のログ（これはユーザー様のものをほぼそのまま使います）
        Debug.Log($"[RelayManager] Relay 参加試行 (JoinRelay呼び出し): Code = {relayCode}");
        try
        {
            // Relay Code を使用して Relay セッションへの参加情報を取得します。
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);

            // NetworkManager が使用するUnityTransport（データの送受信方法）を取得します。
            // networkManager は Awake で取得済みのはずです。
            if (networkManager == null)
            {
                Debug.LogError("[RelayManager] networkManager が null です。Awakeでの取得に失敗した可能性があります。");
                return false;
            }
            UnityTransport transport = networkManager.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("[RelayManager] NetworkManager に UnityTransport コンポーネントが見つかりません。");
                return false;
            }

            // クライアントとして Relay サーバーに接続するための情報をトランスポートに設定します。
            transport.SetClientRelayData(
                joinAllocation.RelayServer.IpV4,     // Relay サーバーの IPv4 アドレス
                (ushort)joinAllocation.RelayServer.Port, // Relay サーバーのポート番号
                joinAllocation.AllocationIdBytes, // 参加した割り当ての一意なID
                joinAllocation.Key,               // 暗号化キー
                joinAllocation.ConnectionData,    // クライアント側の接続データ
                joinAllocation.HostConnectionData // ホスト側の接続データ（クライアントから見たホストのデータ）
            );

            // ★★★ StartClient 直前の状態をログに出力 (修正・追加) ★★★
            // this.IsClientConnected は RelayManager のプロパティで、内部的に networkManager を参照します。
            Debug.Log($"[RelayManager] networkManager.StartClient() を呼び出します。現在の IsClientConnected: {this.IsClientConnected}, NetworkManager.IsListening: {networkManager.IsListening}");

            // Netcode のクライアントとしてゲームに参加します。
            networkManager.StartClient();

            // ★★★ StartClient 直後の状態をログに出力 (修正・追加) ★★★
            // StartClient() は同期的ですが、Netcodeの実際の接続完了は非同期です。
            // この直後の IsClientConnected はまだ false かもしれませんが、Netcodeが接続処理を開始したことを示します。
            Debug.Log($"[RelayManager] Netcode クライアント処理を開始しました (StartClient呼び出し後)。現在の IsClientConnected (即時): {this.IsClientConnected}, NetworkManager.IsListening: {networkManager.IsListening}");

            return true; // 参加処理の呼び出しが成功したことを示します (実際の接続完了は非同期)。
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"[RelayManager] Relay 参加エラー (RelayServiceException): {e.Message}");
            return false;
        }
        catch (System.Exception e)
        {
            // その他の予期せぬエラーが発生した場合の処理。
            Debug.LogError($"[RelayManager] Relay 参加中に予期せぬエラー: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 現在の Netcode 接続（ホストまたはクライアント）を切断します。
    /// </summary>
    public void Disconnect()
    {
        // ★ networkManager ではなく NetworkManager.Singleton を使う方が確実かもしれません
        //    ただし、Singleton が null になる可能性も考慮します。
        var nm = NetworkManager.Singleton;

        if (nm != null) // Singleton が存在する場合
        {
            Debug.LogWarning($"[RelayManager] NetworkManager をシャットダウンします (強制破棄モード)... IsListening: {nm.IsListening}");
            nm.Shutdown(true); // ★★★ 引数に true を追加して、強制破棄を試みる ★★★
        }
        else
        {
            Debug.LogWarning("[RelayManager] Disconnect 時に NetworkManager.Singleton が null です。");
        }

        // // ホストまたはクライアントとして接続している場合のみ実行します。
        // if (NetworkManager.Singleton != null)
        // {
        //     // NetworkManager をシャットダウンし、現在のネットワークセッションを終了します。
        //     networkManager.Shutdown();
        //     Debug.Log("NetworkManager をシャットダウンしました。");
        // }
    }
}




// 1について親はSchool_ClassroomSceneなのでなにもなく、NetworkManager　objectのcomponent にはNetworkManagerのほかにUnity transportと”using UnityEngine;

// using Unity.Netcode;



// public class NetworkStartManager : MonoBehaviour

// {

//     public void StartHost()

//     {

//         if (NetworkManager.Singleton != null)

//         {

//             NetworkManager.Singleton.StartHost();

//             // Debug.Log("ホスト実行");

//         }

//         else

//         {

//             Debug.LogError("NetworkManager.Singleton が null です。");

//         }

//     }



//     public void StartClient()

//     {

//         if (NetworkManager.Singleton != null)

//         {

//             NetworkManager.Singleton.StartClient();

//             // Debug.Log("クライアント実行");

//         }

//         else

//         {

//             Debug.LogError("NetworkManager.Singleton が null です。");

//         }

//     }

// }”があります。

// ２ばんについて、networkManagerはadd compornentからついかしました。

// ３はんは