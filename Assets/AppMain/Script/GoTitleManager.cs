using UnityEngine;
using System.Threading.Tasks;
using Unity.Netcode;

public class GoTitleManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static GoTitleManager Instance { get; private set; }

    [Header("Component References")]
    [SerializeField] private TimerManager timerManager;
    [SerializeField] private GameOverManager gameOverManager;

    private bool _isTransitioning = false; // 多重実行防止フラグ

    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでも破棄されないようにする
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// ボタンの OnClick イベントから呼び出すためのメソッド。
    /// </summary>
    public void OnGoTitleButtonClicked()
    {
        if (_isTransitioning) return;
        _ = GoTitleAsync();
    }

    /// <summary>
    /// タイトルシーンに戻るための非同期処理。
    /// </summary>
    private async Task GoTitleAsync()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;

        Debug.Log("タイトルシーンへの遷移処理を開始します...");

        Time.timeScale = 1f;

        // LobbyServiceManager, RelayManager, AudioManager がシングルトンであることを想定
        if (LobbyServiceManager.Instance != null)
        {
            if (LobbyServiceManager.Instance.IsHost)
            {
                await LobbyServiceManager.Instance.RemoveLobbyAsync();
            }
            else if (LobbyServiceManager.Instance.JoinedLobby != null)
            {
                await LobbyServiceManager.Instance.LeaveLobbyAsync();
            }
        }
        
        if (RelayManager.Instance != null)
        {
            RelayManager.Instance.Disconnect();
        }
        else if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost))
        {
             NetworkManager.Singleton.Shutdown();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllSounds();
        }

        // ご提示の SceneTransitionManager を使ってタイトルシーンへ遷移
        SceneTransitionManager.LoadSceneWithLoadingScreen("TitleScene");
        
        _isTransitioning = false;
    }
}