using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// RankUIの定義は GoGameManager.cs に移動したため削除

public class GameSelectionUIManager : MonoBehaviour
{
    // ランク表示用のリスト変数を削除
    // [Header("Rank UI")]
    // [SerializeField] private List<RankUI> rankUIs; 

    [Header("Game Mode Buttons")]
    [SerializeField] private Button jidoushiTadoushiButton;
    [SerializeField] private Button keigoButton;
    [SerializeField] private Button hiraganaButton;
    [SerializeField] private Button katakanaButton;
    [SerializeField] private Button yohoonButton;
    [SerializeField] private Button katakanaEigoButton;
    [SerializeField] private Button hinshiButton;
    [SerializeField] private Button groupButton;
    [SerializeField] private Button firstKanjiButton;
    [SerializeField] private Button secondKanjiButton;
    [SerializeField] private Button thirdKanjiButton;
    [SerializeField] private Button fourthKanjiButton;
    [SerializeField] private Button fifthKanjiButton;
    [SerializeField] private Button sixthKanjiButton;

    // --- 使用しないボタン（インスペクターに残っていてもコードで制御するため保持、またはnull許容） ---
    [Header("Unused / Hidden Buttons")]
    [SerializeField] private Button kanjiWarmUpButton;
    [SerializeField] private Button kanjiN5Button;
    [SerializeField] private Button kanjiN4Button;
    [SerializeField] private Button kanjiN3Button;
    [SerializeField] private Button kanjiN2Button;
    [SerializeField] private Button kanjiN1Button;

    [Header("Navigation Buttons")]
    [SerializeField] private Button backToTitleButton;

    [Header("Help UI")]
    [SerializeField] private Button helpButton;
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private Button closeHelpButton;

    [Header("UI Display")]
    [Tooltip("現在のゲームモードを表示するテキスト")]
    [SerializeField] private TMP_Text gameModeText;

    [Header("Lock Icons")]
    [Tooltip("購入が必要なゲームモードのボタンに表示するロックアイコン")]
    [SerializeField] private GameObject keigoLock;
    [SerializeField] private GameObject hiraganaLock;
    // [SerializeField] private GameObject katakanaLock;
    [SerializeField] private GameObject yohoonLock;
    [SerializeField] private GameObject katakanaEigoLock;
    [SerializeField] private GameObject hinshiLock;
    [SerializeField] private GameObject groupLock;
    [SerializeField] private GameObject firstKanjiLock;

    // --- 使用しないロックアイコン ---
    [SerializeField] private GameObject kanjiN5Lock;
    [SerializeField] private GameObject kanjiN4Lock;
    [SerializeField] private GameObject kanjiN3Lock;
    [SerializeField] private GameObject kanjiN2Lock;
    [SerializeField] private GameObject kanjiN1Lock;

    void Start()
    {
        if (GameSelectionManager.Instance == null)
        {
            Debug.LogError("[GameSelectionUIManager] GameSelectionManager.Instance が見つかりません。");
            return;
        }

        // 不要なボタンとロックアイコンを非表示にする
        HideUnusedButtons();

        AddListeners();
        UpdateGameModeDisplay();
        
        // セレクト画面でのランク表示は廃止のため削除
        // UpdateRankDisplay();

        if (helpPanel != null)
        {
            helpPanel.SetActive(false);
        }

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

    // 現在使用していない漢字系のボタンとロックを非表示にするメソッド
    private void HideUnusedButtons()
    {
        SetObjectActive(kanjiWarmUpButton, false);
        SetObjectActive(kanjiN5Button, false);
        SetObjectActive(kanjiN4Button, false);
        SetObjectActive(kanjiN3Button, false);
        SetObjectActive(kanjiN2Button, false);
        SetObjectActive(kanjiN1Button, false);

        SetObjectActive(kanjiN5Lock, false);
        SetObjectActive(kanjiN4Lock, false);
        SetObjectActive(kanjiN3Lock, false);
        SetObjectActive(kanjiN2Lock, false);
        SetObjectActive(kanjiN1Lock, false);
    }
    
    private void SetObjectActive(Component comp, bool isActive)
    {
        if (comp != null) comp.gameObject.SetActive(isActive);
    }
    
    private void SetObjectActive(GameObject obj, bool isActive)
    {
        if (obj != null) obj.SetActive(isActive);
    }

    private void AddListeners()
    {
        AddListener(jidoushiTadoushiButton, GameSelectionManager.Instance.OnJidoushiTadoushiSelected);
        AddListener(keigoButton, GameSelectionManager.Instance.OnKeigoSelected);
        AddListener(hiraganaButton, GameSelectionManager.Instance.OnHiraganaSelected);
        AddListener(katakanaButton, GameSelectionManager.Instance.OnKatakanaSelected);
        AddListener(yohoonButton, GameSelectionManager.Instance.OnYohoonSelected);
        AddListener(katakanaEigoButton, GameSelectionManager.Instance.OnKatakanaEigoSelected);
        AddListener(hinshiButton, GameSelectionManager.Instance.OnHinshiSelected);
        AddListener(groupButton, GameSelectionManager.Instance.OnGroupSelected);
        AddListener(firstKanjiButton, GameSelectionManager.Instance.OnFirstKanjiSelected);
        AddListener(secondKanjiButton, GameSelectionManager.Instance.OnSecondKanjiSelected);
        AddListener(thirdKanjiButton, GameSelectionManager.Instance.OnThirdKanjiSelected);
        AddListener(fourthKanjiButton, GameSelectionManager.Instance.OnFourthKanjiSelected);
        AddListener(fifthKanjiButton, GameSelectionManager.Instance.OnFifthKanjiSelected);
        AddListener(sixthKanjiButton, GameSelectionManager.Instance.OnSixthKanjiSelected);
        
        AddListener(backToTitleButton, GameSelectionManager.Instance.OnBackToTitle);

        if (helpButton != null)
        {
            helpButton.onClick.AddListener(OnHelpButtonClicked);
        }
        if (closeHelpButton != null)
        {
            closeHelpButton.onClick.AddListener(OnCloseHelpButtonClicked);
        }
    }

    private void RemoveListeners()
    {
        if (GameSelectionManager.Instance == null) return;

        RemoveListener(jidoushiTadoushiButton, GameSelectionManager.Instance.OnJidoushiTadoushiSelected);
        RemoveListener(keigoButton, GameSelectionManager.Instance.OnKeigoSelected);
        RemoveListener(hiraganaButton, GameSelectionManager.Instance.OnHiraganaSelected);
        RemoveListener(katakanaButton, GameSelectionManager.Instance.OnKatakanaSelected);
        RemoveListener(yohoonButton, GameSelectionManager.Instance.OnYohoonSelected);
        RemoveListener(katakanaEigoButton, GameSelectionManager.Instance.OnKatakanaEigoSelected);
        RemoveListener(hinshiButton, GameSelectionManager.Instance.OnHinshiSelected);
        RemoveListener(groupButton, GameSelectionManager.Instance.OnGroupSelected);
        RemoveListener(firstKanjiButton, GameSelectionManager.Instance.OnFirstKanjiSelected);
        RemoveListener(secondKanjiButton, GameSelectionManager.Instance.OnSecondKanjiSelected);
        RemoveListener(thirdKanjiButton, GameSelectionManager.Instance.OnThirdKanjiSelected);
        RemoveListener(fourthKanjiButton, GameSelectionManager.Instance.OnFourthKanjiSelected);
        RemoveListener(fifthKanjiButton, GameSelectionManager.Instance.OnFifthKanjiSelected);
        RemoveListener(sixthKanjiButton, GameSelectionManager.Instance.OnSixthKanjiSelected);
        
        RemoveListener(backToTitleButton, GameSelectionManager.Instance.OnBackToTitle);

        if (helpButton != null)
        {
            helpButton.onClick.RemoveListener(OnHelpButtonClicked);
        }
        if (closeHelpButton != null)
        {
            closeHelpButton.onClick.RemoveListener(OnCloseHelpButtonClicked);
        }
    }

    private void OnHelpButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSound();
        }
        if (helpPanel != null)
        {
            helpPanel.SetActive(true);
        }
    }

    private void OnCloseHelpButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSound();
        }
        if (helpPanel != null)
        {
            helpPanel.SetActive(false);
        }
    }

    private void UpdateLockIcons()
    {
        SetLockIconState(keigoLock, IAPManager.ProductIds[GameType.Keigo]);
        SetLockIconState(hiraganaLock, IAPManager.ProductIds[GameType.Hiragana]);
        // SetLockIconState(katakanaLock, IAPManager.ProductIds[GameType.Katakana]);
        SetLockIconState(yohoonLock, IAPManager.ProductIds[GameType.Yohoon]);
        SetLockIconState(katakanaEigoLock, IAPManager.ProductIds[GameType.KatakanaEigo]);
        SetLockIconState(hinshiLock, IAPManager.ProductIds[GameType.Hinshi]);
        SetLockIconState(groupLock, IAPManager.ProductIds[GameType.Group]);
        SetLockIconState(firstKanjiLock, IAPManager.ProductIds[GameType.FirstKanji]);
    }

    private void SetLockIconState(GameObject lockIcon, string productId)
    {
        if (lockIcon != null && IAPManager.Instance != null)
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
        if (gameModeText != null && GameSelectionManager.Instance != null)
        {
            switch (GameSelectionManager.Instance.CurrentGameMode)
            {
                case GameSelectionManager.GameMode.SinglePlayer:
                    gameModeText.text = "Single Play";
                    break;
                case GameSelectionManager.GameMode.Multiplayer:
                    gameModeText.text = "Online Play";
                    break;
                default:
                    gameModeText.text = "No Mode Selected";
                    break;
            }
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
    }
#endif
}