using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

namespace MiniGameHub.UI
{
    /// <summary>
    /// Attractive splash screen for app startup with loading progression
    /// </summary>
    public class SplashScreen : MonoBehaviour
    {
        [Header("Logo and Branding")]
        [SerializeField] private Image logoImage;
        [SerializeField] private TextMeshProUGUI gameTitle;
        [SerializeField] private TextMeshProUGUI taglineText;
        [SerializeField] private string appTitle = "Mini Game Hub";
        [SerializeField] private string tagline = "10,000 Games at Your Fingertips";
        
        [Header("Loading Elements")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private TextMeshProUGUI progressPercentage;
        [SerializeField] private GameObject loadingContainer;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem backgroundParticles;
        [SerializeField] private Image backgroundGradient;
        [SerializeField] private Animation logoAnimation;
        [SerializeField] private Animation textAnimation;
        
        [Header("Animations")]
        [SerializeField] private float logoFadeInDuration = 1f;
        [SerializeField] private float textFadeInDelay = 0.5f;
        [SerializeField] private float textFadeInDuration = 1f;
        [SerializeField] private float loadingStartDelay = 1.5f;
        [SerializeField] private float minDisplayTime = 3f;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip startupSound;
        [SerializeField] private AudioClip loadingCompleteSound;
        
        [Header("Scene Management")]
        [SerializeField] private string nextSceneName = "MainMenu";
        [SerializeField] private bool autoLoadNextScene = true;
        
        // Private variables
        private float startTime;
        private bool loadingComplete;
        private float currentProgress;
        
        // Loading steps
        private readonly string[] loadingSteps = new string[]
        {
            "Initializing Game Hub...",
            "Loading Game Catalog...",
            "Setting up Services...",
            "Preparing Game Assets...",
            "Configuring User Data...",
            "Connecting to Services...",
            "Optimizing Performance...",
            "Ready to Play!"
        };

        private void Start()
        {
            startTime = Time.time;
            InitializeSplashScreen();
            StartCoroutine(SplashSequence());
        }

        private void InitializeSplashScreen()
        {
            // Set initial alpha to 0 for fade-in effects
            if (logoImage != null)
            {
                SetImageAlpha(logoImage, 0f);
            }
            
            if (gameTitle != null)
            {
                gameTitle.text = appTitle;
                SetTextAlpha(gameTitle, 0f);
            }
            
            if (taglineText != null)
            {
                taglineText.text = tagline;
                SetTextAlpha(taglineText, 0f);
            }
            
            if (loadingContainer != null)
            {
                loadingContainer.SetActive(false);
            }
            
            if (progressBar != null)
            {
                progressBar.value = 0f;
            }
            
            if (progressPercentage != null)
            {
                progressPercentage.text = "0%";
            }
            
            // Setup background
            SetupBackground();
            
            // Play startup sound
            PlaySound(startupSound);
        }

        private void SetupBackground()
        {
            // Create animated gradient background
            if (backgroundGradient != null)
            {
                StartCoroutine(AnimateBackgroundGradient());
            }
            
            // Start particle effects
            if (backgroundParticles != null)
            {
                backgroundParticles.Play();
            }
        }

        private IEnumerator SplashSequence()
        {
            // Phase 1: Logo fade in
            yield return StartCoroutine(FadeInLogo());
            
            // Phase 2: Text fade in
            yield return new WaitForSeconds(textFadeInDelay);
            yield return StartCoroutine(FadeInTexts());
            
            // Phase 3: Start loading sequence
            yield return new WaitForSeconds(loadingStartDelay);
            yield return StartCoroutine(LoadingSequence());
            
            // Phase 4: Ensure minimum display time
            float elapsedTime = Time.time - startTime;
            if (elapsedTime < minDisplayTime)
            {
                yield return new WaitForSeconds(minDisplayTime - elapsedTime);
            }
            
            // Phase 5: Transition to next scene
            if (autoLoadNextScene)
            {
                yield return StartCoroutine(TransitionToNextScene());
            }
        }

        private IEnumerator FadeInLogo()
        {
            if (logoImage == null) yield break;
            
            float timer = 0f;
            while (timer < logoFadeInDuration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Clamp01(timer / logoFadeInDuration);
                SetImageAlpha(logoImage, alpha);
                
                // Add scale effect
                float scale = Mathf.Lerp(0.8f, 1f, alpha);
                logoImage.transform.localScale = Vector3.one * scale;
                
                yield return null;
            }
            
            SetImageAlpha(logoImage, 1f);
            logoImage.transform.localScale = Vector3.one;
            
            // Play logo animation if available
            if (logoAnimation != null)
            {
                logoAnimation.Play();
            }
        }

        private IEnumerator FadeInTexts()
        {
            Coroutine titleFade = null;
            Coroutine taglineFade = null;
            
            // Fade in title
            if (gameTitle != null)
            {
                titleFade = StartCoroutine(FadeInText(gameTitle, textFadeInDuration));
            }
            
            // Fade in tagline with slight delay
            yield return new WaitForSeconds(0.3f);
            if (taglineText != null)
            {
                taglineFade = StartCoroutine(FadeInText(taglineText, textFadeInDuration));
            }
            
            // Wait for both to complete
            if (titleFade != null) yield return titleFade;
            if (taglineFade != null) yield return taglineFade;
            
            // Play text animation if available
            if (textAnimation != null)
            {
                textAnimation.Play();
            }
        }

        private IEnumerator FadeInText(TextMeshProUGUI text, float duration)
        {
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Clamp01(timer / duration);
                SetTextAlpha(text, alpha);
                yield return null;
            }
            
            SetTextAlpha(text, 1f);
        }

        private IEnumerator LoadingSequence()
        {
            if (loadingContainer != null)
            {
                loadingContainer.SetActive(true);
            }
            
            float stepDuration = 0.8f;
            
            for (int i = 0; i < loadingSteps.Length; i++)
            {
                // Update loading text
                if (loadingText != null)
                {
                    loadingText.text = loadingSteps[i];
                }
                
                // Calculate target progress
                float targetProgress = (float)(i + 1) / loadingSteps.Length;
                
                // Animate progress bar
                yield return StartCoroutine(AnimateProgress(currentProgress, targetProgress, stepDuration));
                
                currentProgress = targetProgress;
                
                // Add slight random delay for realism
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            }
            
            loadingComplete = true;
            PlaySound(loadingCompleteSound);
        }

        private IEnumerator AnimateProgress(float fromProgress, float toProgress, float duration)
        {
            float timer = 0f;
            
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / duration);
                float currentValue = Mathf.Lerp(fromProgress, toProgress, progress);
                
                if (progressBar != null)
                {
                    progressBar.value = currentValue;
                }
                
                if (progressPercentage != null)
                {
                    progressPercentage.text = $"{Mathf.RoundToInt(currentValue * 100)}%";
                }
                
                yield return null;
            }
            
            // Ensure final values are set
            if (progressBar != null)
            {
                progressBar.value = toProgress;
            }
            
            if (progressPercentage != null)
            {
                progressPercentage.text = $"{Mathf.RoundToInt(toProgress * 100)}%";
            }
        }

        private IEnumerator AnimateBackgroundGradient()
        {
            if (backgroundGradient == null) yield break;
            
            Color[] gradientColors = new Color[]
            {
                new Color(0.1f, 0.1f, 0.3f, 1f), // Dark blue
                new Color(0.2f, 0.1f, 0.4f, 1f), // Purple
                new Color(0.3f, 0.2f, 0.5f, 1f), // Light purple
                new Color(0.1f, 0.2f, 0.4f, 1f)  // Blue-purple
            };
            
            int colorIndex = 0;
            
            while (!loadingComplete)
            {
                Color targetColor = gradientColors[colorIndex];
                Color startColor = backgroundGradient.color;
                
                float timer = 0f;
                float duration = 2f;
                
                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    float progress = timer / duration;
                    backgroundGradient.color = Color.Lerp(startColor, targetColor, progress);
                    yield return null;
                }
                
                colorIndex = (colorIndex + 1) % gradientColors.Length;
                yield return new WaitForSeconds(1f);
            }
        }

        private IEnumerator TransitionToNextScene()
        {
            // Fade out everything
            float fadeOutDuration = 1f;
            float timer = 0f;
            
            while (timer < fadeOutDuration)
            {
                timer += Time.deltaTime;
                float alpha = 1f - (timer / fadeOutDuration);
                
                if (logoImage != null) SetImageAlpha(logoImage, alpha);
                if (gameTitle != null) SetTextAlpha(gameTitle, alpha);
                if (taglineText != null) SetTextAlpha(taglineText, alpha);
                if (loadingContainer != null)
                {
                    CanvasGroup canvasGroup = loadingContainer.GetComponent<CanvasGroup>();
                    if (canvasGroup != null) canvasGroup.alpha = alpha;
                }
                
                yield return null;
            }
            
            // Load next scene
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }

        private void SetImageAlpha(Image image, float alpha)
        {
            if (image != null)
            {
                Color color = image.color;
                color.a = alpha;
                image.color = color;
            }
        }

        private void SetTextAlpha(TextMeshProUGUI text, float alpha)
        {
            if (text != null)
            {
                Color color = text.color;
                color.a = alpha;
                text.color = color;
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        // Public methods for external control
        public void SkipSplash()
        {
            StopAllCoroutines();
            if (autoLoadNextScene && !string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }

        public void SetNextScene(string sceneName)
        {
            nextSceneName = sceneName;
        }

        public bool IsLoadingComplete()
        {
            return loadingComplete;
        }

        // Handle input for skipping (optional)
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                // Allow skipping after minimum time
                if (Time.time - startTime > 2f)
                {
                    SkipSplash();
                }
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}