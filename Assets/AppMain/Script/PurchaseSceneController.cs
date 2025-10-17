using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Purchasing;

public class PurchaseSceneController : MonoBehaviour
{
    public static string ProductIdToPurchase { get; set; }
    public static GameType CurrentGameMode { get; set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Button restoreButton;
    [SerializeField] private Button backButton;

    // ▼▼▼ ここのヘッダーと変数名を変更 ▼▼▼
    [Header("Game Specific UI (Scrollable)")]
    // Scroll Viewの中にあるTextMeshProオブジェクトを割り当てる
    [SerializeField] private TextMeshProUGUI additionalDescriptionText;
    // Scroll Viewの親オブジェクトそのものを割り当てる
    [SerializeField] private GameObject descriptionScrollViewObject;
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    void Start()
    {
        // (Startメソッドの中身は変更ありません)
        backButton.onClick.AddListener(OnBackClicked);
        restoreButton.onClick.AddListener(OnRestoreClicked);
        purchaseButton.onClick.AddListener(OnPurchaseClicked);

        if (IAPManager.Instance != null && IAPManager.Instance.IsInitialized)
        {
            SetupUI();
        }
        else
        {
            titleText.text = "ストアに接続中...";
            descriptionText.text = "";
            priceText.text = "";
            purchaseButton.interactable = false;

            if (IAPManager.Instance != null)
            {
                IAPManager.Instance.OnIapInitialized += SetupUI;
            }
        }

        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.OnPurchaseSuccess += HandlePurchaseSuccess;
            IAPManager.Instance.OnPurchaseFailedEvent += HandlePurchaseFailed;
        }
    }

    void OnDestroy()
    {
        // (OnDestroyメソッドの中身は変更ありません)
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.OnIapInitialized -= SetupUI;
            IAPManager.Instance.OnPurchaseSuccess -= HandlePurchaseSuccess;
            IAPManager.Instance.OnPurchaseFailedEvent -= HandlePurchaseFailed;
        }
    }

    void SetupUI()
    {
        // (SetupUIメソッドの前半は変更ありません)
        Debug.Log("IAPの準備ができたのでUIをセットアップします。");
        var product = IAPManager.Instance.GetProduct(ProductIdToPurchase);

        if (product != null && product.availableToPurchase)
        {
            titleText.text = product.metadata.localizedTitle;
            descriptionText.text = product.metadata.localizedDescription;
            priceText.text = product.metadata.localizedPriceString;
            purchaseButton.interactable = true;
        }
        else
        {
            titleText.text = "エラー";
            descriptionText.text = "このアイテムは現在購入できません。";
            priceText.text = "-";
            purchaseButton.interactable = false;
        }

        SetAdditionalDescription();
    }

    private void SetAdditionalDescription()
    {
        // GameDescriptionManagerから現在のゲームモードの説明文を取得
        string additionalText = GameDescriptionManager.GetDescription(CurrentGameMode);

        // 説明文が存在するかどうかで表示・非表示を決定
        if (!string.IsNullOrEmpty(additionalText))
        {
            if (descriptionScrollViewObject != null && additionalDescriptionText != null)
            {
                descriptionScrollViewObject.SetActive(true);
                additionalDescriptionText.text = additionalText;
            }
        }
        else
        {
            if (descriptionScrollViewObject != null)
            {
                descriptionScrollViewObject.SetActive(false);
            }
        }
    }
    private void OnPurchaseClicked()
    {
        purchaseButton.interactable = false;
        IAPManager.Instance.BuyProduct(ProductIdToPurchase);
    }

    private void OnRestoreClicked()
    {
        IAPManager.Instance.RestorePurchases();
    }

    private void OnBackClicked()
    {
        SceneManager.LoadScene("GameSelectionScene");
    }

    private void HandlePurchaseSuccess(string successfulProductId)
    {
        if (successfulProductId == ProductIdToPurchase)
        {
            Debug.Log("購入成功！ゲーム選択シーンに戻ります。");
            SceneManager.LoadScene("GameSelectionScene");
        }
    }

    private void HandlePurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        if (product != null && product.definition.id == ProductIdToPurchase)
        {
            Debug.Log($"購入がキャンセルまたは失敗しました。理由: {reason}");
            purchaseButton.interactable = true;
        }
    }
}