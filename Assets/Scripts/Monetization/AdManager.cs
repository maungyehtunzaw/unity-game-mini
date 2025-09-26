using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Manages advertisement display and rewarded video functionality
/// </summary>
public class AdManager : MonoBehaviour
{
    [Header("Ad Configuration")]
    public bool enableAds = true;
    public bool testMode = true;
    
    [Header("Ad Unit IDs")]
    public string bannerAdUnitId = "ca-app-pub-test/banner";
    public string interstitialAdUnitId = "ca-app-pub-test/interstitial";
    public string rewardedAdUnitId = "ca-app-pub-test/rewarded";
    
    [Header("Ad Timing")]
    public float interstitialCooldown = 180f; // 3 minutes
    public int gamesBeforeAd = 3;
    
    [Header("Rewards")]
    public int rewardedAdCoins = 50;
    public int dailyAdBonusCoins = 100;

    public static AdManager Instance { get; private set; }

    // Events
    public event Action OnInterstitialShown;
    public event Action OnInterstitialClosed;
    public event Action<bool> OnRewardedAdCompleted;
    public event Action OnBannerShown;
    public event Action OnBannerHidden;

    private float _lastInterstitialTime = 0f;
    private int _gamesPlayedSinceAd = 0;
    private bool _initialized = false;
    private bool _bannerVisible = false;
    private System.Action<bool> _rewardedAdCallback;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Listen to game events
        if (ServiceLocator.Bus != null)
        {
            ServiceLocator.Bus.OnMiniGameFinished += OnGameFinished;
        }
    }

    void InitializeAds()
    {
        if (!enableAds)
        {
            Debug.Log("[AdManager] Ads disabled");
            return;
        }

        if (ServiceLocator.Economy?.HasRemoveAds() == true)
        {
            Debug.Log("[AdManager] Ads removed via purchase");
            enableAds = false;
            return;
        }

        #if UNITY_ADS
        InitializeUnityAds();
        #else
        // Mock initialization
        _initialized = true;
        Debug.Log("[AdManager] Mock ads initialized");
        #endif
    }

    #if UNITY_ADS
    void InitializeUnityAds()
    {
        // Unity Ads initialization
        string gameId = testMode ? "test_game_id" : "your_game_id";
        
        UnityEngine.Advertisements.Advertisement.Initialize(gameId, testMode, this);
    }

    public void OnInitializationComplete()
    {
        _initialized = true;
        Debug.Log("[AdManager] Unity Ads initialized successfully");
    }

    public void OnInitializationFailed(UnityEngine.Advertisements.UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"[AdManager] Unity Ads initialization failed: {error} - {message}");
        _initialized = false;
    }
    #endif

    public void ShowBannerAd()
    {
        if (!CanShowAds()) return;

        Debug.Log("[AdManager] Showing banner ad");

        #if UNITY_ADS && !UNITY_EDITOR
        UnityEngine.Advertisements.Advertisement.Banner.SetPosition(UnityEngine.Advertisements.BannerPosition.BOTTOM_CENTER);
        UnityEngine.Advertisements.Advertisement.Banner.Show(bannerAdUnitId);
        #endif

        _bannerVisible = true;
        OnBannerShown?.Invoke();
    }

    public void HideBannerAd()
    {
        if (!_bannerVisible) return;

        Debug.Log("[AdManager] Hiding banner ad");

        #if UNITY_ADS && !UNITY_EDITOR
        UnityEngine.Advertisements.Advertisement.Banner.Hide();
        #endif

        _bannerVisible = false;
        OnBannerHidden?.Invoke();
    }

    public void ShowInterstitialAd()
    {
        if (!CanShowInterstitial()) return;

        Debug.Log("[AdManager] Showing interstitial ad");

        #if UNITY_ADS && !UNITY_EDITOR
        UnityEngine.Advertisements.Advertisement.Show(interstitialAdUnitId, this);
        #else
        // Mock interstitial for testing
        if (testMode)
        {
            StartCoroutine(MockInterstitialAd());
        }
        #endif

        _lastInterstitialTime = Time.time;
        _gamesPlayedSinceAd = 0;
    }

    public void ShowRewardedAd(System.Action<bool> onComplete)
    {
        if (!CanShowAds())
        {
            onComplete?.Invoke(false);
            return;
        }

        _rewardedAdCallback = onComplete;

        Debug.Log("[AdManager] Showing rewarded ad");

        #if UNITY_ADS && !UNITY_EDITOR
        UnityEngine.Advertisements.Advertisement.Show(rewardedAdUnitId, this);
        #else
        // Mock rewarded ad for testing
        if (testMode)
        {
            StartCoroutine(MockRewardedAd());
        }
        #endif
    }

    bool CanShowAds()
    {
        return _initialized && enableAds && !ServiceLocator.Economy?.HasRemoveAds() == true;
    }

    bool CanShowInterstitial()
    {
        if (!CanShowAds()) return false;

        // Check cooldown
        if (Time.time - _lastInterstitialTime < interstitialCooldown)
            return false;

        // Check games played
        if (_gamesPlayedSinceAd < gamesBeforeAd)
            return false;

        return true;
    }

    void OnGameFinished(GameResult result)
    {
        _gamesPlayedSinceAd++;

        // Show interstitial after certain number of games
        if (CanShowInterstitial())
        {
            // Small delay before showing ad
            StartCoroutine(ShowInterstitialAfterDelay(2f));
        }
    }

    IEnumerator ShowInterstitialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowInterstitialAd();
    }

    IEnumerator MockInterstitialAd()
    {
        OnInterstitialShown?.Invoke();
        yield return new WaitForSeconds(5f); // Simulate 5-second ad
        OnInterstitialClosed?.Invoke();
        Debug.Log("[AdManager] Mock interstitial ad completed");
    }

    IEnumerator MockRewardedAd()
    {
        yield return new WaitForSeconds(30f); // Simulate 30-second rewarded ad
        
        // Grant reward
        ServiceLocator.Economy?.AddCoins(rewardedAdCoins);
        
        _rewardedAdCallback?.Invoke(true);
        OnRewardedAdCompleted?.Invoke(true);
        
        Debug.Log($"[AdManager] Mock rewarded ad completed - Granted {rewardedAdCoins} coins");
    }

    #if UNITY_ADS
    public void OnUnityAdsShowComplete(string adUnitId, UnityEngine.Advertisements.UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId == rewardedAdUnitId)
        {
            bool success = showCompletionState == UnityEngine.Advertisements.UnityAdsShowCompletionState.COMPLETED;
            
            if (success)
            {
                // Grant reward
                ServiceLocator.Economy?.AddCoins(rewardedAdCoins);
                Debug.Log($"[AdManager] Rewarded ad completed - Granted {rewardedAdCoins} coins");
            }
            
            _rewardedAdCallback?.Invoke(success);
            OnRewardedAdCompleted?.Invoke(success);
        }
        else if (adUnitId == interstitialAdUnitId)
        {
            OnInterstitialClosed?.Invoke();
            Debug.Log("[AdManager] Interstitial ad closed");
        }
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityEngine.Advertisements.UnityAdsShowError showError, string message)
    {
        Debug.LogError($"[AdManager] Ad show failed: {adUnitId} - {showError}: {message}");
        
        if (adUnitId == rewardedAdUnitId)
        {
            _rewardedAdCallback?.Invoke(false);
            OnRewardedAdCompleted?.Invoke(false);
        }
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
        if (adUnitId == interstitialAdUnitId)
        {
            OnInterstitialShown?.Invoke();
            Debug.Log("[AdManager] Interstitial ad started");
        }
    }

    public void OnUnityAdsShowClick(string adUnitId)
    {
        Debug.Log($"[AdManager] Ad clicked: {adUnitId}");
    }
    #endif

    // Public methods for UI
    public void RequestRewardedAd(System.Action<bool> onComplete)
    {
        ShowRewardedAd(onComplete);
    }

    public bool IsRewardedAdReady()
    {
        #if UNITY_ADS && !UNITY_EDITOR
        return UnityEngine.Advertisements.Advertisement.IsReady(rewardedAdUnitId);
        #else
        return testMode && CanShowAds();
        #endif
    }

    public void ShowDailyBonusAd(System.Action<bool> onComplete)
    {
        ShowRewardedAd((success) =>
        {
            if (success)
            {
                // Additional daily bonus
                ServiceLocator.Economy?.AddCoins(dailyAdBonusCoins);
                Debug.Log($"[AdManager] Daily bonus granted: {dailyAdBonusCoins} coins");
            }
            onComplete?.Invoke(success);
        });
    }

    public string GetRewardedAdButtonText()
    {
        return $"Watch Ad (+{rewardedAdCoins} coins)";
    }

    public string GetDailyBonusButtonText()
    {
        return $"Daily Bonus (+{dailyAdBonusCoins} coins)";
    }

    void OnDestroy()
    {
        if (ServiceLocator.Bus != null)
        {
            ServiceLocator.Bus.OnMiniGameFinished -= OnGameFinished;
        }
    }
}