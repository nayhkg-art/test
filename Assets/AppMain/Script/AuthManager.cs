using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core; // UnityServices.InitializeAsync() のために必要
using Unity.Services.Authentication; // AuthenticationService のために必要

/// <summary>
/// Unity Gaming Services (UGS) の初期化と認証（サインイン/サインアウト）を管理するスクリプト。
/// シーンをまたいで永続化するシングルトンとして機能します。
/// </summary>
public class AuthManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static AuthManager Instance { get; private set; }

    // 他のスクリプトが購読できるイベント
    public event Action OnInitialized;     // UGSが初期化されたときに発生
    public event Action OnSignedIn;        // ユーザーがサインインしたときに発生
    public event Action OnSignedOut;       // ユーザーがサインアウトしたときに発生
    public event Action OnSignInFailed;    // サインインに失敗したときに発生

    // UGSの初期化状態を示すプロパティ
    public bool IsInitialized { get; private set; } = false;

    // ユーザーがサインインしているかを示すプロパティ
    // UnityServicesが初期化された後でのみ安全にアクセスできます。
    public bool IsSignedIn
    {
        get
        {
            // UGSが初期化されていない場合はfalseを返すか、適切なエラー処理を行う
            if (!IsInitialized)
            {
                Debug.LogWarning("[AuthManager] IsSignedIn: Unity Servicesがまだ初期化されていません。");
                return false;
            }
            return AuthenticationService.Instance.IsSignedIn;
        }
    }

    // 現在のプレイヤーIDを返すプロパティ
    public string PlayerId
    {
        get
        {
            if (!IsInitialized || !AuthenticationService.Instance.IsSignedIn)
            {
                return null; // 未初期化または未サインインの場合はnull
            }
            return AuthenticationService.Instance.PlayerId;
        }
    }

    private bool _isSigningIn = false; // 多重サインイン試行を防ぐフラグ

    /// <summary>
    /// オブジェクトがロードされたときに呼び出されます。
    /// シングルトンパターンの実装とUGSの初期化を開始します。
    /// </summary>
    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 既にインスタンスが存在する場合は自身を破棄
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // シーンをまたいでも破棄されないようにする

        Debug.Log("[AuthManager] Awake: Unity Servicesの初期化を開始します...");
        // 非同期の初期化処理を開始（awaitしないが、処理はバックグラウンドで進行）
        _ = InitializeUnityServicesAsync();
    }

    /// <summary>
    /// Unity Gaming Servicesを初期化し、匿名サインインを試みます。
    /// </summary>
    private async Task InitializeUnityServicesAsync()
    {
        try
        {
            // Unity Servicesの初期化
            // UnityServices.InitializeAsync() は、既に初期化済みであれば再初期化を行わないため、
            // UnityServices.IsInitialized のチェックは不要です。
            await UnityServices.InitializeAsync();
            IsInitialized = true; // 初期化が成功したらフラグを立てる
            Debug.Log("[AuthManager] Unity Servicesの初期化に成功しました。");
            OnInitialized?.Invoke(); // 初期化完了イベントを発火

            // 初期化後、匿名サインインを試みる
            await AttemptSignInAsync();
        }
        catch (Exception e)
        {
            // 初期化に失敗した場合
            Debug.LogError($"[AuthManager] Unity Servicesの初期化に失敗しました: {e.Message}");
            IsInitialized = false; // 初期化失敗フラグ
            OnSignInFailed?.Invoke(); // UIにエラーを伝えるためにサインイン失敗イベントを発火
        }
    }

    /// <summary>
    /// 匿名でサインインを試みます。複数回呼び出されても安全です。
    /// </summary>
    public async Task AttemptSignInAsync()
    {
        // 既にサインイン処理中の場合、またはUGSが未初期化の場合は何もしない
        if (_isSigningIn || !IsInitialized)
        {
            Debug.LogWarning("[AuthManager] AttemptSignInAsync: 既にサインイン処理中か、Unity Servicesが未初期化です。");
            return;
        }

        _isSigningIn = true; // サインイン処理中フラグを立てる
        try
        {
            // 既にサインイン済みか確認
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("[AuthManager] 匿名サインインを試行中...");
                await AuthenticationService.Instance.SignInAnonymouslyAsync(); // 匿名サインインを実行
                Debug.Log($"[AuthManager] サインインに成功しました。プレイヤーID: {AuthenticationService.Instance.PlayerId}");
                OnSignedIn?.Invoke(); // サインイン成功イベントを発火
            }
            else
            {
                Debug.Log($"[AuthManager] 既にサインイン済みです。プレイヤーID: {AuthenticationService.Instance.PlayerId}");
                OnSignedIn?.Invoke(); // 既にサインイン済みの場合でもイベントを発火
            }
        }
        catch (AuthenticationException e)
        {
            // 認証関連のエラー
            Debug.LogError($"[AuthManager] サインインに失敗しました (認証エラー): {e.Message}");
            OnSignInFailed?.Invoke(); // サインイン失敗イベントを発火
        }
        catch (Exception e)
        {
            // その他の予期せぬエラー
            Debug.LogError($"[AuthManager] サインイン中に予期せぬエラーが発生しました: {e.Message}");
            OnSignInFailed?.Invoke(); // サインイン失敗イベントを発火
        }
        finally
        {
            _isSigningIn = false; // サインイン処理中フラグを解除
        }
    }

    /// <summary>
    /// デバッグ目的で強制的にサインアウトします。
    /// </summary>
    public void DebugForceSignOutAsync() // async キーワードを削除
    {
        if (!IsInitialized)
        {
            Debug.LogWarning("[AuthManager] DebugForceSignOutAsync: Unity Servicesが未初期化です。");
            return;
        }
        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut();
                Debug.Log("[AuthManager] サインアウトに成功しました。");
                OnSignedOut?.Invoke(); // サインアウトイベントを発火
            }
            else
            {
                Debug.Log("[AuthManager] サインインしていません。サインアウトの必要はありません。");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[AuthManager] 強制サインアウト中にエラーが発生しました: {e.Message}");
        }
    }

    /// <summary>
    /// GameObjectが破棄されるときに呼び出されます。
    /// シングルトンインスタンスをクリアします。
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        // ここではUGSの明示的なシャットダウンは行いません。
        // 一般的にUGSはアプリケーションの終了時にSDKが自動的に管理するか、
        // より上位のアプリケーションライフサイクルマネージャーが制御します。
        // 未初期化のAuthenticationService.Instanceへのアクセスは避けます。
        Debug.Log("[AuthManager] OnDestroyが呼び出されました。");
    }
}