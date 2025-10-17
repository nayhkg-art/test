using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing;
using Unity.Services.Core;
using System.Threading.Tasks;

public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    public static IAPManager Instance { get; private set; }

    private static IStoreController m_StoreController;
    private static IExtensionProvider m_StoreExtensionProvider;

    // 課金対象のアイテムリスト
    public static readonly Dictionary<GameType, string> ProductIds = new Dictionary<GameType, string>
    {
        { GameType.Keigo,       "com.glabo.jidoushi.keigo" },
        { GameType.Hiragana,    "com.glabo.jidoushi.hiragana" },
        { GameType.Katakana,    "com.glabo.jidoushi.katakana" },
        { GameType.Yohoon,      "com.glabo.jidoushi.yohoon" },
        { GameType.KanjiN5,     "com.glabo.jidoushi.kanjin5" },
        { GameType.KanjiN4,     "com.glabo.jidoushi.kanjin4" },
        { GameType.KanjiN3,     "com.glabo.jidoushi.kanjin3" },
        { GameType.KanjiN2,     "com.glabo.jidoushi.kanjin2" },
        { GameType.KanjiN1,     "com.glabo.jidoushi.kanjin1" },
        { GameType.KatakanaEigo,"com.glabo.jidoushi.katakanaeigo" },
        { GameType.Hinshi,      "com.glabo.jidoushi.hinshi" },
        { GameType.Group,       "com.glabo.jidoushi.group" },
        { GameType.FirstKanji,  "com.glabo.jidoushi.firstkanji" }
    };

    // 購入成功/失敗時に呼び出されるイベント
    public event Action<string> OnPurchaseSuccess;
    public event Action<Product, PurchaseFailureReason> OnPurchaseFailedEvent;

    // IAP初期化完了を通知するイベント
    public bool IsInitialized { get; private set; } = false;
    public event Action OnIapInitialized;

    // Awakeではシングルトンの設定のみ行う
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Startで非同期の初期化処理を開始する
    async void Start()
    {
        try
        {
            Debug.Log("--- IAPManager: Unity Gaming Services (UGS) の初期化を開始します... ---");
            await UnityServices.InitializeAsync();
            Debug.Log("--- IAPManager: UGSの初期化に成功しました。 ---");

            // UGSの初期化が成功した後に、IAPの初期化処理を呼び出す
            InitializePurchasing();
        }
        catch (Exception e)
        {
            Debug.LogError($"UGSの初期化中にエラーが発生しました: {e}");
        }
    }

    // IAPの初期化処理
    private void InitializePurchasing()
    {
        Debug.Log("--- IAPManager: IAPの初期化処理を開始します... ---");
        var module = StandardPurchasingModule.Instance();
        module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
        var builder = ConfigurationBuilder.Instance(module);

        foreach (var productId in ProductIds.Values)
        {
            builder.AddProduct(productId, ProductType.NonConsumable);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    public bool IsProductPurchased(string productId)
    {
        if (string.IsNullOrEmpty(productId)) return false;
        return PlayerPrefs.GetInt("purchased_" + productId, 0) == 1;
    }

    public void BuyProduct(string productId)
    {
        if (m_StoreController != null)
        {
            Product product = m_StoreController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                m_StoreController.InitiatePurchase(product);
            }
        }
    }

    public void RestorePurchases()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result, error) =>
            {
                if (result)
                {
                    Debug.Log("RestorePurchases successful.");
                }
                else
                {
                    Debug.LogError("RestorePurchases failed: " + error);
                }
            });
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;
        Debug.Log("IAP Initialized.");
        foreach (var product in controller.products.all)
        {
            if (product.receipt != null && product.definition.type == ProductType.NonConsumable)
            {
                SetPurchased(product.definition.id);
            }
        }

        // 初期化が完了したことを記録し、イベントで通知
        IsInitialized = true;
        OnIapInitialized?.Invoke();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log($"OnInitializeFailed InitializationFailureReason: {error}, message: {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        OnPurchaseCompleted(args.purchasedProduct);
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"OnPurchaseFailed: FAIL. Product: '{product.definition.storeSpecificId}', PurchaseFailureReason: {failureReason}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription description)
    {
        Debug.LogError($"OnPurchaseFailed: FAIL. Product: '{product.definition.storeSpecificId}', " +
                         $"Reason: {description.reason}, Message: {description.message}");

        OnPurchaseFailedEvent?.Invoke(product, description.reason);
    }

    private void SetPurchased(string productId)
    {
        PlayerPrefs.SetInt("purchased_" + productId, 1);
        PlayerPrefs.Save();
        Debug.Log($"Product {productId} purchase status saved.");
    }

    public Product GetProduct(string productId)
    {
        if (m_StoreController != null && !string.IsNullOrEmpty(productId))
        {
            return m_StoreController.products.WithID(productId);
        }
        return null;
    }

    public void HandleSuccessfulPurchase(Product product)
    {
        Debug.Log("IAPListener経由で購入成功をキャッチ！");
        OnPurchaseCompleted(product);
    }

    private void OnPurchaseCompleted(Product purchasedProduct)
    {
        string productId = purchasedProduct.definition.id;
        Debug.Log($"ProcessPurchase: PASS. Product: '{productId}'");

        SetPurchased(productId);
        OnPurchaseSuccess?.Invoke(productId);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public void ClearAllPurchaseData_DEBUG()
    {
        Debug.LogWarning("--- 全ての課金情報をリセットします ---");
        foreach (var productId in ProductIds.Values)
        {
            PlayerPrefs.DeleteKey("purchased_" + productId);
        }
        PlayerPrefs.Save();

        Debug.LogWarning("リセット完了。アプリを再起動するか、タイトルシーンに戻ってください。");
    }
#endif
}