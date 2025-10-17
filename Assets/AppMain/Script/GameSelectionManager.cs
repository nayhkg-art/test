using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections; // Coroutineのために必要

/// <summary>
/// ゲーム選択画面のUIとロジックを管理するマネージャー。
/// </summary>
public class GameSelectionManager : MonoBehaviour
{
    public static GameSelectionManager Instance { get; private set; }
    public GameMode CurrentGameMode { get; private set; }
    public static GameType SelectedGameType { get; private set; }

    public enum GameMode
    {
        None,
        Multiplayer, // オンライン
        SinglePlayer // シングルプレイ
    }

    private bool isLoading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameSelectionManager] Awake: シングルトンインスタンスを設定しました。");
        }
        else
        {
            Debug.LogWarning("[GameSelectionManager] Awake: 既にインスタンスが存在するため、自身を破棄します。");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        int mode = PlayerPrefs.GetInt("GameMode", (int)GameMode.None);
        CurrentGameMode = (GameMode)mode;
        Debug.Log($"[GameSelectionManager] Start: 現在のゲームモードは {CurrentGameMode} です。");
    }

    public void SetCurrentGameMode(GameMode mode)
    {
        CurrentGameMode = mode;
        PlayerPrefs.SetInt("GameMode", (int)CurrentGameMode);
        PlayerPrefs.Save();
        Debug.Log($"[GameSelectionManager] ゲームモードを {CurrentGameMode} に設定しました。");
    }

    // --- 各ボタンに対応するメソッド ---

    public void OnJidoushiTadoushiSelected() => SelectGameType(GameType.JidoushiTadoushi);
    public void OnKeigoSelected() => SelectGameType(GameType.Keigo);
    public void OnHiraganaSelected() => SelectGameType(GameType.Hiragana);
    public void OnKatakanaSelected() => SelectGameType(GameType.Katakana);
    public void OnYohoonSelected() => SelectGameType(GameType.Yohoon);
    public void OnKanjiWarmUpSelected() => SelectGameType(GameType.KanjiWarmUp);
    public void OnKanjiN5Selected() => SelectGameType(GameType.KanjiN5);
    public void OnKanjiN4Selected() => SelectGameType(GameType.KanjiN4);
    public void OnKanjiN3Selected() => SelectGameType(GameType.KanjiN3);
    public void OnKanjiN2Selected() => SelectGameType(GameType.KanjiN2);
    public void OnKanjiN1Selected() => SelectGameType(GameType.KanjiN1);
    public void OnKatakanaEigoSelected() => SelectGameType(GameType.KatakanaEigo);
    public void OnHinshiSelected() => SelectGameType(GameType.Hinshi);
    public void OnGroupSelected() => SelectGameType(GameType.Group);
    public void OnFirstKanjiSelected() => SelectGameType(GameType.FirstKanji);

    private void SelectGameType(GameType gameType)
    {
        if (isLoading) return;

        SelectedGameType = gameType;
        Debug.Log($"[GameSelectionManager] ゲームタイプ {SelectedGameType} が選択されました。");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSound();
        }

        PlayerPrefs.SetInt("SelectedGameType", (int)SelectedGameType);
        PlayerPrefs.Save();

        // --- ▼ここからが新しいロジック▼ ---

        // 課金対象のゲームタイプかチェック
        if (IAPManager.ProductIds.ContainsKey(gameType))
        {
            string productId = IAPManager.ProductIds[gameType];

            // 購入済みかIAPManagerに問い合わせる
            if (IAPManager.Instance.IsProductPurchased(productId))
            {
                // 【購入済み】ゲームシーンをロード
                Debug.Log($"製品 {productId} は購入済みです。ゲームを開始します。");
                StartCoroutine(LoadSceneWithSound("School_Classroom"));
            }
            else
            {
                // 【未購入】購入シーンに移動
                Debug.Log($"製品 {productId} は未購入です。購入シーンに移動します。");

                // PurchaseSceneにどの商品を買うか情報を渡す
                PurchaseSceneController.ProductIdToPurchase = productId;
                 PurchaseSceneController.CurrentGameMode = gameType;
                SceneTransitionManager.LoadSceneWithLoadingScreen("PurchaseScene"); // 他のシーンと同様の遷移を使ってもOK
            }
        }
        else
        {
            // 課金対象ではない（無料）ゲーム
            Debug.Log($"ゲームタイプ {gameType} は無料です。ゲームを開始します。");
            StartCoroutine(LoadSceneWithSound("School_Classroom"));
        }
    }

    public void OnBackToTitle()
    {
        if (isLoading) return;

        Debug.Log("[GameSelectionManager] TitleSceneに戻ります。");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSound();
        }
        SceneTransitionManager.LoadSceneWithLoadingScreen("TitleScene");
    }

    private IEnumerator LoadSceneWithSound(string sceneName) // 引数を追加
    {
        isLoading = true;

        if (AudioManager.Instance != null && AudioManager.Instance.clickSound != null)
        {
            yield return new WaitForSeconds(AudioManager.Instance.clickSound.length);
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }

        SceneTransitionManager.LoadSceneWithLoadingScreen(sceneName);
        isLoading = false;
    }
}