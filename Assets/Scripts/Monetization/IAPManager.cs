using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages In-App Purchases for the game hub
/// Handles coin packs, remove ads, and premium features
/// </summary>
public class IAPManager : MonoBehaviour
{
    [Header("IAP Configuration")]
    public bool enableIAP = true;
    public bool testMode = false;
    
    [Header("Product IDs")]
    public string removeAdsProductId = "remove_ads";
    public string coins1000ProductId = "coins_1000";
    public string coins5000ProductId = "coins_5000";
    public string coins10000ProductId = "coins_10000";
    public string premiumPassProductId = "premium_pass";
    
    [Header("Prices (for display)")]
    public float removeAdsPrice = 2.99f;
    public float coins1000Price = 0.99f;
    public float coins5000Price = 3.99f;
    public float coins10000Price = 6.99f;
    public float premiumPassPrice = 9.99f;

    public static IAPManager Instance { get; private set; }

    // Events
    public event Action<string> OnPurchaseCompleted;
    public event Action<string> OnPurchaseFailed;
    public event Action<string> OnPurchaseRestored;

    private Dictionary<string, ProductInfo> _products = new Dictionary<string, ProductInfo>();
    private bool _initialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeIAP();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeIAP()
    {
        if (!enableIAP)
        {
            Debug.Log("[IAPManager] IAP disabled");
            return;
        }

        // Setup product catalog
        _products[removeAdsProductId] = new ProductInfo
        {
            id = removeAdsProductId,
            type = ProductType.NonConsumable,
            price = removeAdsPrice,
            title = "Remove Ads",
            description = "Remove all advertisements from the game"
        };

        _products[coins1000ProductId] = new ProductInfo
        {
            id = coins1000ProductId,
            type = ProductType.Consumable,
            price = coins1000Price,
            title = "1,000 Coins",
            description = "Get 1,000 coins to unlock more games"
        };

        _products[coins5000ProductId] = new ProductInfo
        {
            id = coins5000ProductId,
            type = ProductType.Consumable,
            price = coins5000Price,
            title = "5,000 Coins",
            description = "Get 5,000 coins - Best Value!"
        };

        _products[coins10000ProductId] = new ProductInfo
        {
            id = coins10000ProductId,
            type = ProductType.Consumable,
            price = coins10000Price,
            title = "10,000 Coins",
            description = "Get 10,000 coins - Ultimate Pack!"
        };

        _products[premiumPassProductId] = new ProductInfo
        {
            id = premiumPassProductId,
            type = ProductType.NonConsumable,
            price = premiumPassPrice,
            title = "Premium Pass",
            description = "Unlock all games, remove ads, and get daily bonuses!"
        };

        #if UNITY_PURCHASING
        InitializeUnityIAP();
        #else
        // Mock initialization for testing
        _initialized = true;
        Debug.Log("[IAPManager] Mock IAP initialized (Unity Purchasing not available)");
        #endif
    }

    #if UNITY_PURCHASING
    void InitializeUnityIAP()
    {
        // Unity IAP initialization would go here
        // This requires the Unity IAP package to be installed
        
        var builder = UnityEngine.Purchasing.ConfigurationBuilder.Instance(
            UnityEngine.Purchasing.StandardPurchasingModule.Instance());

        foreach (var product in _products.Values)
        {
            builder.AddProduct(product.id, 
                product.type == ProductType.Consumable ? 
                UnityEngine.Purchasing.ProductType.Consumable : 
                UnityEngine.Purchasing.ProductType.NonConsumable);
        }

        UnityEngine.Purchasing.UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(UnityEngine.Purchasing.IStoreController controller, 
                             UnityEngine.Purchasing.IExtensionProvider extensions)
    {
        _initialized = true;
        Debug.Log("[IAPManager] Unity IAP initialized successfully");
        
        // Check for restored purchases
        RestorePurchases();
    }

    public void OnInitializeFailed(UnityEngine.Purchasing.InitializationFailureReason error)
    {
        Debug.LogError($"[IAPManager] IAP initialization failed: {error}");
        _initialized = false;
    }

    public UnityEngine.Purchasing.PurchaseProcessingResult ProcessPurchase(
        UnityEngine.Purchasing.PurchaseEventArgs args)
    {
        string productId = args.purchasedProduct.definition.id;
        Debug.Log($"[IAPManager] Purchase completed: {productId}");
        
        ProcessPurchaseInternal(productId);
        OnPurchaseCompleted?.Invoke(productId);
        
        return UnityEngine.Purchasing.PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, 
                                UnityEngine.Purchasing.PurchaseFailureReason failureReason)
    {
        Debug.LogError($"[IAPManager] Purchase failed: {product.definition.id}, Reason: {failureReason}");
        OnPurchaseFailed?.Invoke(product.definition.id);
    }
    #endif

    public void PurchaseProduct(string productId)
    {
        if (!_initialized)
        {
            Debug.LogWarning("[IAPManager] IAP not initialized");
            return;
        }

        if (!_products.ContainsKey(productId))
        {
            Debug.LogError($"[IAPManager] Unknown product: {productId}");
            return;
        }

        Debug.Log($"[IAPManager] Purchasing: {productId}");

        #if UNITY_PURCHASING && !UNITY_EDITOR
        // Real purchase on device
        var storeController = /* get from initialization */;
        storeController.InitiatePurchase(productId);
        #else
        // Mock purchase for testing
        if (testMode)
        {
            Debug.Log($"[IAPManager] Mock purchase: {productId}");
            ProcessPurchaseInternal(productId);
            OnPurchaseCompleted?.Invoke(productId);
        }
        #endif
    }

    void ProcessPurchaseInternal(string productId)
    {
        switch (productId)
        {
            case var id when id == removeAdsProductId:
                ServiceLocator.Economy.SetRemoveAds(true);
                Debug.Log("[IAPManager] Ads removed");
                break;

            case var id when id == coins1000ProductId:
                ServiceLocator.Economy.AddCoins(1000);
                Debug.Log("[IAPManager] Added 1,000 coins");
                break;

            case var id when id == coins5000ProductId:
                ServiceLocator.Economy.AddCoins(5000);
                Debug.Log("[IAPManager] Added 5,000 coins");
                break;

            case var id when id == coins10000ProductId:
                ServiceLocator.Economy.AddCoins(10000);
                Debug.Log("[IAPManager] Added 10,000 coins");
                break;

            case var id when id == premiumPassProductId:
                ServiceLocator.Economy.SetRemoveAds(true);
                ServiceLocator.Save.Data.removeAds = true;
                // Could unlock premium features here
                Debug.Log("[IAPManager] Premium Pass activated");
                break;

            default:
                Debug.LogWarning($"[IAPManager] Unhandled purchase: {productId}");
                break;
        }

        ServiceLocator.Save.Save();
    }

    public void RestorePurchases()
    {
        #if UNITY_PURCHASING && !UNITY_EDITOR
        // Restore purchases on device
        #else
        // Mock restore for testing
        if (testMode)
        {
            Debug.Log("[IAPManager] Mock restore purchases");
            
            // Check if remove ads was previously purchased
            if (ServiceLocator.Economy.HasRemoveAds())
            {
                OnPurchaseRestored?.Invoke(removeAdsProductId);
            }
        }
        #endif
    }

    public ProductInfo GetProductInfo(string productId)
    {
        return _products.ContainsKey(productId) ? _products[productId] : null;
    }

    public List<ProductInfo> GetAllProducts()
    {
        return new List<ProductInfo>(_products.Values);
    }

    public bool IsProductPurchased(string productId)
    {
        switch (productId)
        {
            case var id when id == removeAdsProductId:
                return ServiceLocator.Economy.HasRemoveAds();
            case var id when id == premiumPassProductId:
                return ServiceLocator.Economy.HasRemoveAds(); // Premium includes ad removal
            default:
                return false; // Consumables are not "owned"
        }
    }

    // Public methods for UI
    public void BuyRemoveAds() => PurchaseProduct(removeAdsProductId);
    public void BuyCoins1000() => PurchaseProduct(coins1000ProductId);
    public void BuyCoins5000() => PurchaseProduct(coins5000ProductId);
    public void BuyCoins10000() => PurchaseProduct(coins10000ProductId);
    public void BuyPremiumPass() => PurchaseProduct(premiumPassProductId);
}

[System.Serializable]
public class ProductInfo
{
    public string id;
    public ProductType type;
    public float price;
    public string title;
    public string description;
    public string localizedPrice;
}

public enum ProductType
{
    Consumable,
    NonConsumable
}