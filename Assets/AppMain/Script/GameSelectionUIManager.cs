using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSelectionUIManager : MonoBehaviour
{
    // 各ゲームタイプ選択ボタンへの参照をInspectorで設定
    [Header("Game Mode Buttons")]
    [SerializeField] private Button jidoushiTadoushiButton;
    [SerializeField] private Button keigoButton;
    [SerializeField] private Button hiraganaButton;
    [SerializeField] private Button katakanaButton;
    [SerializeField] private Button yohoonButton;
    [SerializeField] private Button kanjiWarmUpButton;
    [SerializeField] private Button kanjiN5Button;
    [SerializeField] private Button kanjiN4Button;
    [SerializeField] private Button kanjiN3Button;
    [SerializeField] private Button kanjiN2Button;
    [SerializeField] private Button kanjiN1Button;
    [SerializeField] private Button katakanaEigoButton;
    [SerializeField] private Button hinshiButton;
    [SerializeField] private Button groupButton;
    [SerializeField] private Button firstKanjiButton;

    [Header("Navigation Buttons")]
    [SerializeField] private Button backToTitleButton;

    [Header("UI Display")]
    [Tooltip("現在のゲームモードを表示するテキスト")]
    [SerializeField] private TMP_Text gameModeText;

    // ▼▼▼ 以下を追加 ▼▼▼
    [Header("Lock Icons")]
    [Tooltip("購入が必要なゲームモードのボタンに表示するロックアイコン")]
    [SerializeField] private GameObject keigoLock;
    [SerializeField] private GameObject hiraganaLock;
    [SerializeField] private GameObject katakanaLock;
    [SerializeField] private GameObject yohoonLock;
    [SerializeField] private GameObject kanjiN5Lock;
    [SerializeField] private GameObject kanjiN4Lock;
    [SerializeField] private GameObject kanjiN3Lock;
    [SerializeField] private GameObject kanjiN2Lock;
    [SerializeField] private GameObject kanjiN1Lock;
    [SerializeField] private GameObject katakanaEigoLock;
    [SerializeField] private GameObject hinshiLock;
    [SerializeField] private GameObject groupLock;
    [SerializeField] private GameObject firstKanjiLock;
    // ▲▲▲ ここまで追加 ▲▲▲

    void Start()
    {
        if (GameSelectionManager.Instance == null)
        {
            Debug.LogError("[GameSelectionUIManager] GameSelectionManager.Instance が見つかりません。");
            return;
        }

        // 各ボタンのOnClickイベントにリスナーを追加
        AddListeners();

        // 現在のゲームモードを取得してUIに表示
        UpdateGameModeDisplay();

        // ▼▼▼ 以下を追加 ▼▼▼
        // IAPManagerのインスタンスが存在するか確認
        if (IAPManager.Instance != null)
        {
            // IAPが既に初期化済みかチェック
            if (IAPManager.Instance.IsInitialized)
            {
                // 初期化済みなら、すぐにUIを更新
                UpdateLockIcons();
            }
            else
            {
                // まだなら、初期化完了イベントを購読
                IAPManager.Instance.OnIapInitialized += UpdateLockIcons;
            }
            // 購入成功イベントを購読（購入直後にUIを更新するため）
            IAPManager.Instance.OnPurchaseSuccess += OnPurchaseCompleted;
        }
        else
        {
            Debug.LogError("[GameSelectionUIManager] IAPManager.Instance が見つかりません。");
        }
        // ▲▲▲ ここまで追加 ▲▲▲
    }

    void OnDestroy()
    {
        // ▼▼▼ イベント購読解除処理を修正 ▼▼▼
        RemoveListeners(); // リスナー削除処理をメソッドにまとめる

        if (IAPManager.Instance != null)
        {
            // 購読したイベントを解除
            IAPManager.Instance.OnIapInitialized -= UpdateLockIcons;
            IAPManager.Instance.OnPurchaseSuccess -= OnPurchaseCompleted;
        }
        // ▲▲▲ ここまで修正 ▲▲▲
    }
    
    // ▼▼▼ 以下を全て追加 ▼▼▼

    /// <summary>
    /// 各ボタンにリスナーを登録します。
    /// </summary>
    private void AddListeners()
    {
        AddListener(jidoushiTadoushiButton, GameSelectionManager.Instance.OnJidoushiTadoushiSelected);
        AddListener(keigoButton, GameSelectionManager.Instance.OnKeigoSelected);
        AddListener(hiraganaButton, GameSelectionManager.Instance.OnHiraganaSelected);
        AddListener(katakanaButton, GameSelectionManager.Instance.OnKatakanaSelected);
        AddListener(yohoonButton, GameSelectionManager.Instance.OnYohoonSelected);
        AddListener(kanjiWarmUpButton, GameSelectionManager.Instance.OnKanjiWarmUpSelected);
        AddListener(kanjiN5Button, GameSelectionManager.Instance.OnKanjiN5Selected);
        AddListener(kanjiN4Button, GameSelectionManager.Instance.OnKanjiN4Selected);
        AddListener(kanjiN3Button, GameSelectionManager.Instance.OnKanjiN3Selected);
        AddListener(kanjiN2Button, GameSelectionManager.Instance.OnKanjiN2Selected);
        AddListener(kanjiN1Button, GameSelectionManager.Instance.OnKanjiN1Selected);
        AddListener(katakanaEigoButton, GameSelectionManager.Instance.OnKatakanaEigoSelected);
        AddListener(hinshiButton, GameSelectionManager.Instance.OnHinshiSelected);
        AddListener(groupButton, GameSelectionManager.Instance.OnGroupSelected);
        AddListener(firstKanjiButton, GameSelectionManager.Instance.OnFirstKanjiSelected);
        AddListener(backToTitleButton, GameSelectionManager.Instance.OnBackToTitle);
    }

    /// <summary>
    /// 各ボタンからリスナーを解除します。
    /// </summary>
    private void RemoveListeners()
    {
        if (GameSelectionManager.Instance == null) return;

        RemoveListener(jidoushiTadoushiButton, GameSelectionManager.Instance.OnJidoushiTadoushiSelected);
        RemoveListener(keigoButton, GameSelectionManager.Instance.OnKeigoSelected);
        RemoveListener(hiraganaButton, GameSelectionManager.Instance.OnHiraganaSelected);
        RemoveListener(katakanaButton, GameSelectionManager.Instance.OnKatakanaSelected);
        RemoveListener(yohoonButton, GameSelectionManager.Instance.OnYohoonSelected);
        RemoveListener(kanjiWarmUpButton, GameSelectionManager.Instance.OnKanjiWarmUpSelected);
        RemoveListener(kanjiN5Button, GameSelectionManager.Instance.OnKanjiN5Selected);
        RemoveListener(kanjiN4Button, GameSelectionManager.Instance.OnKanjiN4Selected);
        RemoveListener(kanjiN3Button, GameSelectionManager.Instance.OnKanjiN3Selected);
        RemoveListener(kanjiN2Button, GameSelectionManager.Instance.OnKanjiN2Selected);
        RemoveListener(kanjiN1Button, GameSelectionManager.Instance.OnKanjiN1Selected);
        RemoveListener(katakanaEigoButton, GameSelectionManager.Instance.OnKatakanaEigoSelected);
        RemoveListener(hinshiButton, GameSelectionManager.Instance.OnHinshiSelected);
        RemoveListener(groupButton, GameSelectionManager.Instance.OnGroupSelected);
        RemoveListener(firstKanjiButton, GameSelectionManager.Instance.OnFirstKanjiSelected);
        RemoveListener(backToTitleButton, GameSelectionManager.Instance.OnBackToTitle);
    }
    
    /// <summary>
    /// 購入状態に基づいてロックアイコンの表示/非表示を更新します。
    /// </summary>
    private void UpdateLockIcons()
    {
        Debug.Log("ロックアイコンの表示状態を更新します。");
        SetLockIconState(keigoLock,       IAPManager.ProductIds[GameType.Keigo]);
        SetLockIconState(hiraganaLock,    IAPManager.ProductIds[GameType.Hiragana]);
        SetLockIconState(katakanaLock,    IAPManager.ProductIds[GameType.Katakana]);
        SetLockIconState(yohoonLock,      IAPManager.ProductIds[GameType.Yohoon]);
        SetLockIconState(kanjiN5Lock,     IAPManager.ProductIds[GameType.KanjiN5]);
        SetLockIconState(kanjiN4Lock,     IAPManager.ProductIds[GameType.KanjiN4]);
        SetLockIconState(kanjiN3Lock,     IAPManager.ProductIds[GameType.KanjiN3]);
        SetLockIconState(kanjiN2Lock,     IAPManager.ProductIds[GameType.KanjiN2]);
        SetLockIconState(kanjiN1Lock,     IAPManager.ProductIds[GameType.KanjiN1]);
        SetLockIconState(katakanaEigoLock,IAPManager.ProductIds[GameType.KatakanaEigo]);
        SetLockIconState(hinshiLock,      IAPManager.ProductIds[GameType.Hinshi]);
        SetLockIconState(groupLock,       IAPManager.ProductIds[GameType.Group]);
        SetLockIconState(firstKanjiLock,  IAPManager.ProductIds[GameType.FirstKanji]);
    }

    /// <summary>
    /// 指定されたプロダクトIDの購入状態に応じて、対応するロックアイコンのGameObjectをアクティブ/非アクティブにします。
    /// </summary>
    /// <param name="lockIcon">対象のロックアイコンGameObject</param>
    /// <param name="productId">チェックするプロダクトID</param>
    private void SetLockIconState(GameObject lockIcon, string productId)
    {
        if (lockIcon != null)
        {
            // IAPManagerから購入状態を取得
            bool isPurchased = IAPManager.Instance.IsProductPurchased(productId);
            // 購入済みなら非表示 (false)、未購入なら表示 (true)
            lockIcon.SetActive(!isPurchased);
        }
    }
    
    /// <summary>
    /// IAPManagerのOnPurchaseSuccessイベントから呼び出されるハンドラ。
    /// </summary>
    /// <param name="productId">購入された商品のID（このメソッドでは未使用）</param>
    private void OnPurchaseCompleted(string productId)
    {
        // 購入が成功したらUIを更新する
        Debug.Log($"購入成功({productId})を検知。UIを更新します。");
        UpdateLockIcons();
    }

    /// <summary>
    /// 現在のゲームモードをUIテキストに表示します。
    /// </summary>
    private void UpdateGameModeDisplay()
    {
        if (gameModeText == null)
        {
            Debug.LogWarning("[GameSelectionUIManager] GameModeTextが設定されていません。");
            return;
        }

        if (GameSelectionManager.Instance != null)
        {
            switch (GameSelectionManager.Instance.CurrentGameMode)
            {
                case GameSelectionManager.GameMode.SinglePlayer:
                    gameModeText.text = "Single Play";
                    break;
                case GameSelectionManager.GameMode.Multiplayer:
                    gameModeText.text = "Online Play";
                    break;
                case GameSelectionManager.GameMode.None:
                default:
                    gameModeText.text = "No Mode Selected";
                    Debug.LogWarning("[GameSelectionUIManager] GameModeがNoneまたは未定義です。");
                    break;
            }
        }
        else
        {
            gameModeText.text = "error";
            Debug.LogError("[GameSelectionUIManager] GameSelectionManagerのインスタンスが見つかりません。");
        }
    }
    // ▲▲▲ ここまで全て追加 ▲▲▲

    // リスナーを追加するヘルパーメソッド
    private void AddListener(Button button, UnityEngine.Events.UnityAction call)
    {
        if (button != null)
        {
            button.onClick.AddListener(call);
        }
    }

    // リスナーを削除するヘルパーメソッド
    private void RemoveListener(Button button, UnityEngine.Events.UnityAction call)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(call);
        }
    }
    // UIボタンから呼び出すための仲介役メソッド
    #if UNITY_EDITOR || DEVELOPMENT_BUILD
    // UIボタンから呼び出すための仲介役メソッド
    public void ResetIAPPurchases_Proxy()
    {
        // IAPManagerのインスタンスを見つけて、その中のリセットメソッドを呼び出す
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.ClearAllPurchaseData_DEBUG();
            // リセット後、UIも即時反映させる
            UpdateLockIcons();
        }
        else
        {
            Debug.LogError("IAPManagerのインスタンスが見つかりません！");
        }
    }
#endif
}