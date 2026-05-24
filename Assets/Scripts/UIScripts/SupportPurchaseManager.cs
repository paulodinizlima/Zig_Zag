using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class SupportPurchaseManager : MonoBehaviour, IDetailedStoreListener
{
    public static SupportPurchaseManager instance;

    [Header("Product Id")]
    [SerializeField] private string supportProductId = "support_development_499";

    [Header("Debug")]
    [SerializeField] private bool simulatePurchaseInEditor = true;

    private IStoreController storeController;
    private IExtensionProvider extensionProvider;

    private string localizedPrice = "R$ 4,99";

    public bool HasRealPrice { get; private set; } = false;
    public string LocalizedPrice => localizedPrice;
    public string SupportProductId => supportProductId;

    private void Awake()
    {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        InitializePurchasing();
    }

    private void InitializePurchasing()
    {
        if (storeController != null) {
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(supportProductId, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }

    public void BuySupportDevelopment()
    {
#if UNITY_EDITOR
        if (simulatePurchaseInEditor) {
            if (SupportPromptManager.instance != null) {
                SupportPromptManager.instance.OnPurchaseSuccess();
            }
            return;
        }
#endif

        if (storeController == null) {
            if (SupportPromptManager.instance != null) {
                SupportPromptManager.instance.OnPurchaseFailed("Compra ainda não está disponível.");
            }
            return;
        }

        Product product = storeController.products.WithID(supportProductId);

        if (product == null || !product.availableToPurchase) {
            if (SupportPromptManager.instance != null) {
                SupportPromptManager.instance.OnPurchaseFailed("Produto indisponível no momento.");
            }
            return;
        }

        storeController.InitiatePurchase(product);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionProvider = extensions;

        Product product = storeController.products.WithID(supportProductId);

        if (product != null && product.metadata != null) {
            localizedPrice = product.metadata.localizedPriceString;
            HasRealPrice = !string.IsNullOrEmpty(localizedPrice);
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        if (SupportPromptManager.instance != null) {
            SupportPromptManager.instance.OnPurchaseFailed("Falha ao inicializar compras: " + error);
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        if (SupportPromptManager.instance != null) {
            SupportPromptManager.instance.OnPurchaseFailed("Falha ao inicializar compras: " + message);
        }
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (args.purchasedProduct.definition.id == supportProductId) {
            if (SupportPromptManager.instance != null) {
                SupportPromptManager.instance.OnPurchaseSuccess();
            }

            return PurchaseProcessingResult.Complete;
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        if (SupportPromptManager.instance != null) {
            SupportPromptManager.instance.OnPurchaseFailed("Compra cancelada ou falhou: " + failureReason);
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        if (SupportPromptManager.instance != null) {
            SupportPromptManager.instance.OnPurchaseFailed("Compra falhou: " + failureDescription.message);
        }
    }
}