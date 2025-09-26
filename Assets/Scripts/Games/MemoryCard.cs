using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MiniGameHub.Games
{
    /// <summary>
    /// Individual card component for the Memory Game
    /// </summary>
    public class MemoryCard : MonoBehaviour
    {
        [Header("Card Components")]
        [SerializeField] private Button cardButton;
        [SerializeField] private Image cardImage;
        [SerializeField] private Image cardBack;
        [SerializeField] private Image cardFront;
        [SerializeField] private Animator cardAnimator;
        
        [Header("Card Settings")]
        [SerializeField] private float flipDuration = 0.3f;
        [SerializeField] private bool useAnimations = true;
        
        // Card properties
        private int cardId;
        private int symbolIndex;
        private Sprite symbolSprite;
        private Sprite backSprite;
        private Color cardColor;
        private bool isFlipped;
        private bool isMatched;
        private bool isInteractable = true;
        
        // Events
        public System.Action<MemoryCard> OnCardClicked;
        
        // Properties
        public int CardId => cardId;
        public int SymbolIndex => symbolIndex;
        public bool IsFlipped => isFlipped;
        public bool IsMatched => isMatched;

        private void Awake()
        {
            // Get components if not assigned
            if (cardButton == null)
                cardButton = GetComponent<Button>();
            if (cardImage == null)
                cardImage = GetComponent<Image>();
            if (cardAnimator == null)
                cardAnimator = GetComponent<Animator>();
            
            // Setup button listener
            if (cardButton != null)
            {
                cardButton.onClick.RemoveAllListeners();
                cardButton.onClick.AddListener(OnCardButtonClicked);
            }
            
            // Initialize card state
            isFlipped = false;
            isMatched = false;
        }

        public void Initialize(int id, int symbol, Sprite symbolSprite, Sprite backSprite, Color color)
        {
            cardId = id;
            symbolIndex = symbol;
            this.symbolSprite = symbolSprite;
            this.backSprite = backSprite;
            cardColor = color;
            
            // Setup visual components
            if (cardBack != null)
            {
                cardBack.sprite = backSprite;
                cardBack.gameObject.SetActive(true);
            }
            
            if (cardFront != null)
            {
                cardFront.sprite = symbolSprite;
                cardFront.color = cardColor;
                cardFront.gameObject.SetActive(false);
            }
            
            // Set initial state to back
            isFlipped = false;
            UpdateVisualState();
        }

        private void OnCardButtonClicked()
        {
            if (isInteractable && !isMatched)
            {
                OnCardClicked?.Invoke(this);
            }
        }

        public void FlipToFront(bool animated = true)
        {
            if (isFlipped) return;
            
            isFlipped = true;
            
            if (animated && useAnimations)
            {
                StartCoroutine(AnimateFlip(true));
            }
            else
            {
                UpdateVisualState();
            }
        }

        public void FlipToBack(bool animated = true)
        {
            if (!isFlipped) return;
            
            isFlipped = false;
            
            if (animated && useAnimations)
            {
                StartCoroutine(AnimateFlip(false));
            }
            else
            {
                UpdateVisualState();
            }
        }

        private IEnumerator AnimateFlip(bool toFront)
        {
            // Disable button during animation
            if (cardButton != null)
                cardButton.interactable = false;
            
            // Scale down (flip preparation)
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = new Vector3(0f, originalScale.y, originalScale.z);
            
            float timer = 0f;
            while (timer < flipDuration / 2)
            {
                timer += Time.deltaTime;
                float progress = timer / (flipDuration / 2);
                transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
                yield return null;
            }
            
            // Change the card face at the middle of the flip
            UpdateVisualState();
            
            // Scale back up
            timer = 0f;
            while (timer < flipDuration / 2)
            {
                timer += Time.deltaTime;
                float progress = timer / (flipDuration / 2);
                transform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
                yield return null;
            }
            
            transform.localScale = originalScale;
            
            // Re-enable button
            if (cardButton != null && isInteractable)
                cardButton.interactable = true;
        }

        private void UpdateVisualState()
        {
            if (cardBack != null)
            {
                cardBack.gameObject.SetActive(!isFlipped);
            }
            
            if (cardFront != null)
            {
                cardFront.gameObject.SetActive(isFlipped);
            }
            
            // Update main card image if using single image setup
            if (cardImage != null && cardBack == null && cardFront == null)
            {
                cardImage.sprite = isFlipped ? symbolSprite : backSprite;
                cardImage.color = isFlipped ? cardColor : Color.white;
            }
        }

        public void SetMatched(bool matched)
        {
            isMatched = matched;
            
            if (matched)
            {
                // Visual feedback for matched cards
                StartCoroutine(MatchedEffect());
            }
        }

        private IEnumerator MatchedEffect()
        {
            // Pulse effect for matched cards
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * 1.1f;
            
            // Scale up
            float timer = 0f;
            while (timer < 0.2f)
            {
                timer += Time.deltaTime;
                float progress = timer / 0.2f;
                transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
                yield return null;
            }
            
            // Scale back down
            timer = 0f;
            while (timer < 0.2f)
            {
                timer += Time.deltaTime;
                float progress = timer / 0.2f;
                transform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
                yield return null;
            }
            
            transform.localScale = originalScale;
            
            // Add subtle glow or color change for matched cards
            if (cardFront != null)
            {
                Color originalColor = cardFront.color;
                Color glowColor = Color.Lerp(originalColor, Color.white, 0.3f);
                cardFront.color = glowColor;
            }
        }

        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
            
            if (cardButton != null)
            {
                cardButton.interactable = interactable && !isMatched;
            }
        }

        public void ResetCard()
        {
            isFlipped = false;
            isMatched = false;
            isInteractable = true;
            
            StopAllCoroutines();
            transform.localScale = Vector3.one;
            
            UpdateVisualState();
            
            if (cardButton != null)
            {
                cardButton.interactable = true;
            }
            
            if (cardFront != null)
            {
                cardFront.color = cardColor;
            }
        }

        public void SetHighlighted(bool highlighted)
        {
            // Add visual highlight effect
            if (highlighted)
            {
                // Add outline or glow effect
                if (cardImage != null)
                {
                    cardImage.color = Color.yellow;
                }
            }
            else
            {
                // Remove highlight
                if (cardImage != null)
                {
                    cardImage.color = Color.white;
                }
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}