using UnityEngine;
using UnityEngine.SceneManagement; // シーン遷移が必要な場合
using UnityEngine.UI; // UIボタンなどの操作が必要な場合

// このスクリプトは、あなたのプロジェクトのタイトル画面を制御するGameObjectにアタッチしてください。
// 例: TitleCanvas、TitleManager など
public class TitleScreenManager : MonoBehaviour
{
    // AudioManagerのシングルトンインスタンスへの参照
    private AudioManager audioManager; // 参照は残しますが、直接オーディオ再生は呼び出しません

    // もしタイトル画面にボタンなどがあるなら、ここに参照を追加します
    // [SerializeField] private Button startButton;
    void Start() // OnEnable の代わりに Start を使用する
{
    // AudioManagerが見つからないというエラーが発生している場合、
    // AudioManager.Instanceがnullでないことを確認してから処理を行う
    if (AudioManager.Instance != null)
    {
        // 以前OnEnableにあったAudioManagerに関連する処理をここに移動
        // 例: AudioManager.Instance.PlayBGM(BGMType.TitleScreen);
    }
    else
    {
        Debug.LogError("TitleScreenManager: AudioManagerが見つかりません。シングルトンが正しく設定されているか、スクリプト実行順序を確認してください。");
    }
}

    // private void OnEnable() // このGameObjectがアクティブになったときに呼ばれます
    // {
    //     Debug.Log("TitleScreenManager: OnEnableが呼び出されました。");

    //     // AudioManagerのインスタンスをOnEnableで取得する。
    //     // これにより、Awakeで取得するよりも安定してシングルトンが初期化されている状態を捉えやすくなります。
    //     if (audioManager == null)
    //     {
    //         audioManager = AudioManager.Instance;
    //         if (audioManager == null)
    //         {
    //             Debug.LogError("TitleScreenManager: AudioManagerが見つかりません。シングルトンが正しく設定されているか、タイトルシーンにAudioManagerのGameObjectが存在するか確認してください。");
    //         }
    //     }

    //     // --- ここが重要な変更点です ---
    //     // AudioManagerのOnSceneLoadedがタイトル画面のオーディオシーケンス全体（タイトルコール音声 -> タイトルBGM）を制御するようになりました。
    //     // そのため、TitleScreenManagerから個別にPlayTitleCall()などを呼び出す必要はありません。
    //     // 以下のコメントアウトされた行は、以前エラーの原因となっていた、または不要になった部分です。
    //     // if (audioManager != null)
    //     // {
    //     //     audioManager.PlayTitleCall(); // この行はもう不要です。
    //     //     Debug.Log("TitleScreenManager: タイトルコール再生を試みましたが、この行はもう不要です。");
    //     // }
    //     // else
    //     // {
    //     //     Debug.LogError("TitleScreenManager: AudioManagerがnullのため、タイトルコールを再生できません。");
    //     // }

    //     // ボタンのリスナー登録など、OnEnableで初期化したいUIの処理があればここに追加
    //     // if (startButton != null)
    //     // {
    //     //     startButton.onClick.AddListener(OnStartButtonClicked);
    //     // }
    // }

    private void OnDisable() // このGameObjectが非アクティブになったときに呼ばれます
    {
        // OnEnableで登録したイベントリスナーを解除します
        // if (startButton != null)
        // {
        //     startButton.onClick.RemoveListener(OnStartButtonClicked);
        // }
    }

    // 例: スタートボタンが押されたときの処理
    // public void OnStartButtonClicked()
    // {
    //     Debug.Log("Start Button Clicked!");
    //     // シーン遷移前にサウンドを停止
    //     // if (audioManager != null)
    //     // {
    //     //     audioManager.StopAllSounds(); // タイトルコールやBGMを停止
    //     // }
    //
    //     // 例: ロビーシーンへ遷移
    //     // SceneManager.LoadScene("LobbyScene");
    // }
}