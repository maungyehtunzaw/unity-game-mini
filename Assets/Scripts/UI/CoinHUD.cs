using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shared coin display that can be used in multiple scenes
/// </summary>
public class CoinHUD : MonoBehaviour 
{
    [Header("Coin Display")]
    public Text coinText;
    public Text levelText;
    public Image coinIcon;
    
    [Header("Animation")]
    public bool animateChanges = true;
    public float animationDuration = 0.5f;
    
    [Header("Effects")]
    public ParticleSystem coinGainEffect;
    public AudioSource coinSound;

    private int _displayedCoins = 0;
    private int _targetCoins = 0;
    private Coroutine _animationCoroutine;

    void OnEnable()
    {
        RefreshDisplay();
        
        if (ServiceLocator.Economy != null)
        {
            ServiceLocator.Economy.OnCoinsChanged += OnCoinsChanged;
            ServiceLocator.Economy.OnLevelUp += OnLevelUp;
        }
    }
    
    void OnDisable()
    {
        if (ServiceLocator.Economy != null)
        {
            ServiceLocator.Economy.OnCoinsChanged -= OnCoinsChanged;
            ServiceLocator.Economy.OnLevelUp -= OnLevelUp;
        }
    }

    void Start()
    {
        RefreshDisplay();
    }

    void RefreshDisplay()
    {
        if (ServiceLocator.Economy == null) return;
        
        _targetCoins = ServiceLocator.Economy.Coins;
        _displayedCoins = _targetCoins;
        
        UpdateCoinText();
        UpdateLevelText();
    }

    void OnCoinsChanged(int newAmount)
    {
        int difference = newAmount - _targetCoins;
        _targetCoins = newAmount;
        
        if (animateChanges && difference != 0)
        {
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);
            
            _animationCoroutine = StartCoroutine(AnimateCoinChange());
            
            // Play effects for coin gain
            if (difference > 0)
            {
                PlayCoinGainEffects();
            }
        }
        else
        {
            _displayedCoins = _targetCoins;
            UpdateCoinText();
        }
    }

    void OnLevelUp(int newLevel)
    {
        UpdateLevelText();
        
        // Could add level up celebration effects here
        Debug.Log($"[CoinHUD] Level up celebration: {newLevel}");
    }

    System.Collections.IEnumerator AnimateCoinChange()
    {
        int startCoins = _displayedCoins;
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
            float progress = elapsedTime / animationDuration;
            
            // Smooth animation curve
            progress = Mathf.SmoothStep(0f, 1f, progress);
            
            _displayedCoins = Mathf.RoundToInt(Mathf.Lerp(startCoins, _targetCoins, progress));
            UpdateCoinText();
            
            yield return null;
        }
        
        _displayedCoins = _targetCoins;
        UpdateCoinText();
        _animationCoroutine = null;
    }

    void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = FormatCoinAmount(_displayedCoins);
        }
    }

    void UpdateLevelText()
    {
        if (levelText != null && ServiceLocator.Economy != null)
        {
            levelText.text = $"LV.{ServiceLocator.Economy.PlayerLevel}";
        }
    }

    string FormatCoinAmount(int amount)
    {
        // Format large numbers nicely
        if (amount >= 1000000)
            return $"{amount / 1000000f:F1}M";
        else if (amount >= 1000)
            return $"{amount / 1000f:F1}K";
        else
            return amount.ToString();
    }

    void PlayCoinGainEffects()
    {
        // Play particle effect
        if (coinGainEffect != null)
        {
            coinGainEffect.Play();
        }
        
        // Play sound effect
        if (coinSound != null)
        {
            coinSound.Play();
        }
        
        // Animate coin icon
        if (coinIcon != null)
        {
            StartCoroutine(AnimateCoinIcon());
        }
    }

    System.Collections.IEnumerator AnimateCoinIcon()
    {
        Vector3 originalScale = coinIcon.transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;
        
        // Scale up
        float elapsedTime = 0f;
        float scaleDuration = 0.1f;
        
        while (elapsedTime < scaleDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / scaleDuration;
            coinIcon.transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            yield return null;
        }
        
        // Scale back down
        elapsedTime = 0f;
        while (elapsedTime < scaleDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / scaleDuration;
            coinIcon.transform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
            yield return null;
        }
        
        coinIcon.transform.localScale = originalScale;
    }

    // Public method to force refresh (useful when scene loads)
    public void ForceRefresh()
    {
        RefreshDisplay();
    }

    // Method to test coin effects (for debugging)
    [ContextMenu("Test Coin Gain")]
    public void TestCoinGain()
    {
        if (ServiceLocator.Economy != null)
        {
            ServiceLocator.Economy.AddCoins(100);
        }
    }
}