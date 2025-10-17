using UnityEngine;
using UnityEngine.UI;

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

    void Start()
    {
        if (GameSelectionManager.Instance == null)
        {
            Debug.LogError("[GameSelectionUIManager] GameSelectionManager.Instance が見つかりません。");
            return;
        }

        // 各ボタンのOnClickイベントにリスナーを追加
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

    void OnDestroy()
    {
        if (GameSelectionManager.Instance == null) return; // Instanceがない場合は何もしない

        // リスナーを削除
        RemoveListener(jidoushiTadoushiButton, GameSelectionManager.Instance.OnJidoushiTadoushiSelected);
        RemoveListener(keigoButton, GameSelectionManager.Instance.OnKeigoSelected);
        RemoveListener(hiraganaButton, GameSelectionManager.Instance.OnHiraganaSelected);
        RemoveListener(katakanaButton, GameSelectionManager.Instance.OnKatakanaSelected);
        RemoveListener(yohoonButton, GameSelectionManager.Instance.OnYohoonSelected); // 修正: Katakana -> Yohoon
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
        }
        else
        {
            Debug.LogError("IAPManagerのインスタンスが見つかりません！");
        }
    }
    // ▼▼▼ この行を追加 ▼▼▼
#endif
}