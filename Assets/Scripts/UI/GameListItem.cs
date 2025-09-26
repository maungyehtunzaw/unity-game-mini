using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MiniGameHub.Core;

namespace MiniGameHub.UI
{
    /// <summary>
    /// Individual game item in the game list
    /// </summary>
    public class GameListItem : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI gameNameText;
        [SerializeField] private TextMeshProUGUI gameDescriptionText;
        [SerializeField] private Image gameIcon;
        [SerializeField] private Button playButton;
        [SerializeField] private GameObject premiumBadge;
        [SerializeField] private GameObject featuredBadge;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private Button infoButton;
        
        [Header("Visual States")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color premiumColor = new Color(1f, 0.8f, 0f, 1f);
        [SerializeField] private Color disabledColor = Color.gray;
        
        // Private variables
        private string gameId;
        private bool isLocked;
        private Image backgroundImage;
        
        // Events
        public System.Action<string> OnGameSelected;
        public System.Action<string> OnGameInfoRequested;

        private void Awake()
        {
            backgroundImage = GetComponent<Image>();
            
            // Setup button listeners
            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(OnPlayButtonClicked);
            }
            
            if (infoButton != null)
            {
                infoButton.onClick.RemoveAllListeners();
                infoButton.onClick.AddListener(OnInfoButtonClicked);
            }
        }

        public void Setup(string gameId)
        {
            this.gameId = gameId;
            
            // Set game name
            if (gameNameText != null)
            {
                gameNameText.text = GameListConstants.GetGameDisplayName(gameId);
            }
            
            // Set game description
            if (gameDescriptionText != null)
            {
                gameDescriptionText.text = GameListConstants.GetGameDescription(gameId);
            }
            
            // Setup badges and states
            bool isPremium = GameListConstants.IsGamePremium(gameId);
            bool isFeatured = GameListConstants.IsGameFeatured(gameId);
            
            // Show/hide premium badge
            if (premiumBadge != null)
            {
                premiumBadge.SetActive(isPremium);
            }
            
            // Show/hide featured badge
            if (featuredBadge != null)
            {
                featuredBadge.SetActive(isFeatured);
            }
            
            // Check if game is locked
            isLocked = isPremium && !HasPremiumAccess();
            
            // Setup locked state
            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(isLocked);
            }
            
            // Update visual appearance
            UpdateVisualState();
            
            // Load game icon
            LoadGameIcon();
        }

        private void UpdateVisualState()
        {
            Color targetColor = normalColor;
            
            if (isLocked)
            {
                targetColor = disabledColor;
            }
            else if (GameListConstants.IsGamePremium(gameId))
            {
                targetColor = premiumColor;
            }
            
            if (backgroundImage != null)
            {
                backgroundImage.color = targetColor;
            }
            
            // Update button interactability
            if (playButton != null)
            {
                playButton.interactable = !isLocked;
            }
        }

        private void LoadGameIcon()
        {
            if (gameIcon == null) return;
            
            // Load icon from Resources or Addressables
            // For now, we'll use a default icon based on game category
            Sprite iconSprite = LoadIconForGame(gameId);
            if (iconSprite != null)
            {
                gameIcon.sprite = iconSprite;
            }
        }

        private Sprite LoadIconForGame(string gameId)
        {
            // This would typically load from Resources or Addressables
            // For now, return null and use default icon
            string iconPath = $"GameIcons/{gameId}";
            return Resources.Load<Sprite>(iconPath);
        }

        private bool HasPremiumAccess()
        {
            // Check if player has premium access
            // This would integrate with your IAP system
            var economyService = ServiceLocator.Instance?.GetService<EconomyService>();
            return economyService?.HasPremiumAccess() ?? false;
        }

        private void OnPlayButtonClicked()
        {
            if (isLocked)
            {
                // Show unlock prompt
                ShowUnlockPrompt();
                return;
            }
            
            OnGameSelected?.Invoke(gameId);
        }

        private void OnInfoButtonClicked()
        {
            OnGameInfoRequested?.Invoke(gameId);
        }

        private void ShowUnlockPrompt()
        {
            // Show a popup explaining how to unlock the game
            Debug.Log($"Game {gameId} is locked. Premium access required.");
            
            // In a full implementation, you'd show a proper unlock dialog
            // This could offer:
            // - Purchase premium access
            // - Watch an ad to unlock temporarily
            // - Complete achievements to unlock
        }

        public void SetHighlighted(bool highlighted)
        {
            // Add visual highlight effect
            if (backgroundImage != null)
            {
                float alpha = highlighted ? 1.2f : 1.0f;
                Color color = backgroundImage.color;
                color.a = alpha;
                backgroundImage.color = color;
            }
        }

        public string GetGameId()
        {
            return gameId;
        }

        public bool IsLocked()
        {
            return isLocked;
        }
    }
}