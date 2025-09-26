using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MiniGameHub.Core;

namespace MiniGameHub.UI
{
    /// <summary>
    /// Featured game item displayed prominently on the home page
    /// </summary>
    public class FeaturedGameItem : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI gameNameText;
        [SerializeField] private Image gameIcon;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button playButton;
        [SerializeField] private GameObject newBadge;
        [SerializeField] private GameObject hotBadge;
        [SerializeField] private ParticleSystem sparkleEffect;
        
        [Header("Visual Settings")]
        [SerializeField] private Gradient backgroundGradient;
        [SerializeField] private float animationDuration = 0.3f;
        
        // Private variables
        private string gameId;
        private RectTransform rectTransform;
        private Vector3 originalScale;
        
        // Events
        public System.Action<string> OnGameSelected;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalScale = transform.localScale;
            
            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(OnPlayButtonClicked);
            }
            
            // Add hover effects
            var eventTrigger = gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }
            
            // Add pointer enter event
            var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) => OnPointerEnter());
            eventTrigger.triggers.Add(pointerEnter);
            
            // Add pointer exit event
            var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => OnPointerExit());
            eventTrigger.triggers.Add(pointerExit);
        }

        public void Setup(string gameId)
        {
            this.gameId = gameId;
            
            // Set game name
            if (gameNameText != null)
            {
                gameNameText.text = GameListConstants.GetGameDisplayName(gameId);
            }
            
            // Load game icon
            LoadGameIcon();
            
            // Setup visual styling
            SetupVisualStyling();
            
            // Setup badges
            SetupBadges();
            
            // Start subtle animation
            StartIdleAnimation();
        }

        private void LoadGameIcon()
        {
            if (gameIcon == null) return;
            
            // Load icon from Resources
            string iconPath = $"GameIcons/Featured/{gameId}";
            Sprite iconSprite = Resources.Load<Sprite>(iconPath);
            
            if (iconSprite == null)
            {
                // Fallback to regular icon
                iconPath = $"GameIcons/{gameId}";
                iconSprite = Resources.Load<Sprite>(iconPath);
            }
            
            if (iconSprite != null)
            {
                gameIcon.sprite = iconSprite;
            }
        }

        private void SetupVisualStyling()
        {
            if (backgroundImage == null) return;
            
            // Set gradient background based on game category
            Color primaryColor = GetColorForGame(gameId);
            Color secondaryColor = Color.Lerp(primaryColor, Color.white, 0.3f);
            
            // Apply gradient (simplified version)
            backgroundImage.color = primaryColor;
        }

        private Color GetColorForGame(string gameId)
        {
            // Assign colors based on game type
            return gameId switch
            {
                var id when id.Contains("puzzle") => new Color(0.6f, 0.2f, 0.8f), // Purple
                var id when id.Contains("action") => new Color(0.8f, 0.2f, 0.2f), // Red
                var id when id.Contains("memory") => new Color(0.2f, 0.6f, 0.8f), // Blue
                var id when id.Contains("strategy") => new Color(0.8f, 0.6f, 0.2f), // Orange
                var id when id.Contains("reflex") => new Color(0.2f, 0.8f, 0.2f), // Green
                _ => new Color(0.5f, 0.5f, 0.8f) // Default blue
            };
        }

        private void SetupBadges()
        {
            // Show "NEW" badge for recently added games
            if (newBadge != null)
            {
                bool isNew = IsGameNew(gameId);
                newBadge.SetActive(isNew);
            }
            
            // Show "HOT" badge for popular games
            if (hotBadge != null)
            {
                bool isHot = IsGameHot(gameId);
                hotBadge.SetActive(isHot);
            }
        }

        private bool IsGameNew(string gameId)
        {
            // Check if game was added recently
            // This would typically check against a timestamp
            return gameId == GameListConstants.GAME_TIC_TAC_TOE || 
                   gameId == GameListConstants.GAME_MEMORY_MATCH;
        }

        private bool IsGameHot(string gameId)
        {
            // Check if game is currently popular
            // This would typically check play statistics
            return gameId == GameListConstants.GAME_SNAKE || 
                   gameId == GameListConstants.GAME_MATCH_THREE;
        }

        private void StartIdleAnimation()
        {
            // Subtle floating animation
            if (rectTransform != null)
            {
                LeanTween.moveY(rectTransform, rectTransform.anchoredPosition.y + 5f, 2f)
                    .setEase(LeanTweenType.easeInOutSine)
                    .setLoopPingPong();
            }
        }

        private void OnPointerEnter()
        {
            // Scale up slightly on hover
            LeanTween.cancel(gameObject);
            LeanTween.scale(gameObject, originalScale * 1.05f, animationDuration)
                .setEase(LeanTweenType.easeOutBack);
            
            // Activate sparkle effect
            if (sparkleEffect != null && !sparkleEffect.isPlaying)
            {
                sparkleEffect.Play();
            }
        }

        private void OnPointerExit()
        {
            // Scale back to normal
            LeanTween.cancel(gameObject);
            LeanTween.scale(gameObject, originalScale, animationDuration)
                .setEase(LeanTweenType.easeOutBack);
            
            // Stop sparkle effect
            if (sparkleEffect != null && sparkleEffect.isPlaying)
            {
                sparkleEffect.Stop();
            }
        }

        private void OnPlayButtonClicked()
        {
            // Play button click animation
            LeanTween.cancel(gameObject);
            LeanTween.scale(gameObject, originalScale * 0.95f, 0.1f)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() =>
                {
                    LeanTween.scale(gameObject, originalScale, 0.1f)
                        .setEase(LeanTweenType.easeOutBack);
                });
            
            // Trigger game selection
            OnGameSelected?.Invoke(gameId);
        }

        private void OnDestroy()
        {
            // Clean up animations
            LeanTween.cancel(gameObject);
        }

        public string GetGameId()
        {
            return gameId;
        }
    }
}