using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class RankUI
{
    public GameType gameType;
    public GameObject rank_S_UI;
    public GameObject rank_A_UI;
    public GameObject rank_B_UI;
    public GameObject rank_C_UI;
    public GameObject rank_D_UI;
    public GameObject rank_E_UI;
    public GameObject rank_F_UI;
}

public class GameSelectionUIManager : MonoBehaviour
{
    [Header("Rank UI")]
    [SerializeField] private List<RankUI> rankUIs;
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

    void Start()
    {
        if (GameSelectionManager.Instance == null)
        {
            Debug.LogError("[GameSelectionUIManager] GameSelectionManager.Instance が見つかりません。");
            return;
        }

        AddListeners();
        UpdateGameModeDisplay();
        UpdateRankDisplay();

        if (IAPManager.Instance != null)
        {
            if (IAPManager.Instance.IsInitialized)
            {
                UpdateLockIcons();
            }
            else
            {
                IAPManager.Instance.OnIapInitialized += UpdateLockIcons;
            }
            IAPManager.Instance.OnPurchaseSuccess += OnPurchaseCompleted;
        }
        else
        {
            Debug.LogError("[GameSelectionUIManager] IAPManager.Instance が見つかりません。");
        }
    }

    void OnDestroy()
    {
        RemoveListeners();

        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.OnIapInitialized -= UpdateLockIcons;
            IAPManager.Instance.OnPurchaseSuccess -= OnPurchaseCompleted;
        }
    }
    
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
    
    private void UpdateLockIcons()
    {
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

    private void SetLockIconState(GameObject lockIcon, string productId)
    {
        if (lockIcon != null)
        {
            bool isPurchased = IAPManager.Instance.IsProductPurchased(productId);
            lockIcon.SetActive(!isPurchased);
        }
    }
    
    private void OnPurchaseCompleted(string productId)
    {
        UpdateLockIcons();
    }

    private void UpdateGameModeDisplay()
    {
        if (gameModeText == null)
        {
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
                    gameModeText.text = "モード未選択";
                    Debug.LogWarning("[GameSelectionUIManager] GameModeがNoneまたは未定義です。");
                    break;
            }
        }
        else
        {
            gameModeText.text = "エラー";
            Debug.LogError("[GameSelectionUIManager] GameSelectionManagerのインスタンスが見つかりません。");
        }
    }

    private void UpdateRankDisplay()
    {
        foreach (var rankUI in rankUIs)
        {
            RankManager.Rank bestRank = RankManager.LoadBestRank(rankUI.gameType);

            SetRankUIActive(rankUI, RankManager.Rank.S, bestRank == RankManager.Rank.S);
            SetRankUIActive(rankUI, RankManager.Rank.A, bestRank == RankManager.Rank.A);
            SetRankUIActive(rankUI, RankManager.Rank.B, bestRank == RankManager.Rank.B);
            SetRankUIActive(rankUI, RankManager.Rank.C, bestRank == RankManager.Rank.C);
            SetRankUIActive(rankUI, RankManager.Rank.D, bestRank == RankManager.Rank.D);
            SetRankUIActive(rankUI, RankManager.Rank.E, bestRank == RankManager.Rank.E);
            SetRankUIActive(rankUI, RankManager.Rank.F, bestRank == RankManager.Rank.F);

            if (bestRank == RankManager.Rank.None)
            {
                SetAllRankUIInactive(rankUI);
            }
        }
    }

    private void SetAllRankUIInactive(RankUI rankUI)
    {
        if (rankUI == null || rankUI.rank_S_UI == null) return;
        rankUI.rank_S_UI.SetActive(false);
        rankUI.rank_A_UI.SetActive(false);
        rankUI.rank_B_UI.SetActive(false);
        rankUI.rank_C_UI.SetActive(false);
        rankUI.rank_D_UI.SetActive(false);
        rankUI.rank_E_UI.SetActive(false);
        rankUI.rank_F_UI.SetActive(false);
    }

    private void SetRankUIActive(RankUI rankUI, RankManager.Rank rank, bool isActive)
    {
        GameObject uiObject = null;
        switch (rank)
        {
            case RankManager.Rank.S: uiObject = rankUI.rank_S_UI; break;
            case RankManager.Rank.A: uiObject = rankUI.rank_A_UI; break;
            case RankManager.Rank.B: uiObject = rankUI.rank_B_UI; break;
            case RankManager.Rank.C: uiObject = rankUI.rank_C_UI; break;
            case RankManager.Rank.D: uiObject = rankUI.rank_D_UI; break;
            case RankManager.Rank.E: uiObject = rankUI.rank_E_UI; break;
            case RankManager.Rank.F: uiObject = rankUI.rank_F_UI; break;
        }

        if (uiObject != null)
        {
            uiObject.SetActive(isActive);
        }
    }

    private void AddListener(Button button, UnityEngine.Events.UnityAction call)
    {
        if (button != null)
        {
            button.onClick.AddListener(call);
        }
    }

    private void RemoveListener(Button button, UnityEngine.Events.UnityAction call)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(call);
        }
    }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public void ResetIAPPurchases_Proxy()
    {
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.ClearAllPurchaseData_DEBUG();
            UpdateLockIcons();
        }
        else
        {
            Debug.LogError("IAPManagerのインスタンスが見つかりません！");
        }
    }
#endif
}