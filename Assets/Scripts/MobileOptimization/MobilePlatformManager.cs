using UnityEngine;

/// <summary>
/// Handles mobile-specific functionality and optimizations
/// Manages screen orientations, input handling, and device-specific features
/// </summary>
public class MobilePlatformManager : MonoBehaviour
{
    [Header("Screen Settings")]
    public bool autoRotation = true;
    public bool landscapeLeft = true;
    public bool landscapeRight = true;
    public bool portrait = true;
    public bool portraitUpsideDown = false;
    
    [Header("Input Settings")]
    public bool multiTouchEnabled = true;
    public int maxTouchCount = 10;
    
    [Header("Performance")]
    public bool preventScreenTimeout = true;
    public int targetFrameRate = 60;
    
    [Header("Safe Area")]
    public bool respectSafeArea = true;
    public RectTransform safeAreaPanel;

    private ScreenOrientation _lastOrientation;
    private Resolution _lastResolution;

    void Start()
    {
        InitializeMobilePlatform();
        SetupScreenSettings();
        SetupInputSettings();
        
        if (respectSafeArea && safeAreaPanel != null)
            ApplySafeArea();
    }

    void InitializeMobilePlatform()
    {
        // Platform-specific initialization
        #if UNITY_ANDROID
        InitializeAndroid();
        #elif UNITY_IOS
        InitializeiOS();
        #endif
        
        // General mobile settings
        if (preventScreenTimeout)
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        Application.targetFrameRate = targetFrameRate;
        
        Debug.Log($"[MobilePlatform] Initialized for {Application.platform}");
    }

    #if UNITY_ANDROID
    void InitializeAndroid()
    {
        // Android-specific settings
        Screen.fullScreen = true;
        
        // Handle Android navigation bar
        if (Screen.fullScreen)
        {
            // Hide system UI for immersive experience
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject window = activity.Call<AndroidJavaObject>("getWindow");
            AndroidJavaObject view = window.Call<AndroidJavaObject>("getDecorView");
            
            int uiOptions = view.Call<int>("getSystemUiVisibility");
            uiOptions |= 4; // SYSTEM_UI_FLAG_HIDE_NAVIGATION
            uiOptions |= 2; // SYSTEM_UI_FLAG_FULLSCREEN
            uiOptions |= 1024; // SYSTEM_UI_FLAG_IMMERSIVE_STICKY
            
            view.Call("setSystemUiVisibility", uiOptions);
        }
        
        Debug.Log("[MobilePlatform] Android platform initialized");
    }
    #endif

    #if UNITY_IOS
    void InitializeiOS()
    {
        // iOS-specific settings
        Screen.fullScreen = false; // iOS handles fullscreen differently
        
        // Enable Game Center if available
        Social.localUser.Authenticate((bool success) => {
            if (success)
                Debug.Log("[MobilePlatform] Game Center authenticated");
            else
                Debug.Log("[MobilePlatform] Game Center authentication failed");
        });
        
        Debug.Log("[MobilePlatform] iOS platform initialized");
    }
    #endif

    void SetupScreenSettings()
    {
        Screen.autorotateToLandscapeLeft = landscapeLeft;
        Screen.autorotateToLandscapeRight = landscapeRight;
        Screen.autorotateToPortrait = portrait;
        Screen.autorotateToPortraitUpsideDown = portraitUpsideDown;
        
        if (autoRotation)
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
        }
        
        _lastOrientation = Screen.orientation;
        _lastResolution = Screen.currentResolution;
    }

    void SetupInputSettings()
    {
        Input.multiTouchEnabled = multiTouchEnabled;
        
        if (multiTouchEnabled)
        {
            // Configure multi-touch settings
            Debug.Log($"[MobilePlatform] Multi-touch enabled, max touches: {maxTouchCount}");
        }
    }

    void ApplySafeArea()
    {
        if (safeAreaPanel == null) return;
        
        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        
        safeAreaPanel.anchorMin = anchorMin;
        safeAreaPanel.anchorMax = anchorMax;
        
        Debug.Log($"[MobilePlatform] Safe area applied: {safeArea}");
    }

    void Update()
    {
        // Check for orientation changes
        if (Screen.orientation != _lastOrientation)
        {
            OnOrientationChanged(Screen.orientation);
            _lastOrientation = Screen.orientation;
        }
        
        // Check for resolution changes
        if (Screen.currentResolution.width != _lastResolution.width || 
            Screen.currentResolution.height != _lastResolution.height)
        {
            OnResolutionChanged(Screen.currentResolution);
            _lastResolution = Screen.currentResolution;
        }
        
        // Handle mobile input
        HandleMobileInput();
    }

    void OnOrientationChanged(ScreenOrientation newOrientation)
    {
        Debug.Log($"[MobilePlatform] Orientation changed to: {newOrientation}");
        
        // Reapply safe area after orientation change
        if (respectSafeArea && safeAreaPanel != null)
        {
            Invoke(nameof(ApplySafeArea), 0.1f); // Small delay to ensure UI is updated
        }
        
        // Notify other systems about orientation change
        ServiceLocator.Bus?.Publish(new OrientationChangeEvent { newOrientation = newOrientation });
    }

    void OnResolutionChanged(Resolution newResolution)
    {
        Debug.Log($"[MobilePlatform] Resolution changed to: {newResolution.width}x{newResolution.height}");
        
        // Update UI scaling if needed
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (var canvas in canvases)
        {
            var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (scaler != null)
            {
                // Adjust UI scaling based on new resolution
                float aspectRatio = (float)newResolution.width / newResolution.height;
                if (aspectRatio > 1.7f) // Wide aspect ratio
                {
                    scaler.matchWidthOrHeight = 0f; // Match width
                }
                else
                {
                    scaler.matchWidthOrHeight = 1f; // Match height
                }
            }
        }
    }

    void HandleMobileInput()
    {
        // Handle Android back button
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackButtonPressed();
        }
        
        // Handle multi-touch gestures
        if (Input.touchCount >= 2)
        {
            HandleMultiTouch();
        }
    }

    void OnBackButtonPressed()
    {
        Debug.Log("[MobilePlatform] Back button pressed");
        
        // Send back button event to current scene
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        
        if (activeScene.name == SceneNames.MiniGame)
        {
            // In mini-game, back button should pause or quit
            var loader = FindObjectOfType<MiniGameLoader>();
            if (loader != null)
            {
                // If game is paused, resume. If not paused, pause.
                loader.RequestPause();
            }
        }
        else if (activeScene.name == SceneNames.Home)
        {
            // In home, back button should minimize app (Android) or do nothing (iOS)
            #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                .GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call<bool>("moveTaskToBack", true);
            #endif
        }
    }

    void HandleMultiTouch()
    {
        // Basic pinch-to-zoom detection
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
            Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;
            
            float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
            float touchDeltaMag = (touch1.position - touch2.position).magnitude;
            
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
            
            if (Mathf.Abs(deltaMagnitudeDiff) > 1f)
            {
                // Pinch gesture detected
                OnPinchGesture(deltaMagnitudeDiff > 0 ? -1 : 1);
            }
        }
    }

    void OnPinchGesture(int direction)
    {
        Debug.Log($"[MobilePlatform] Pinch gesture: {(direction > 0 ? "zoom in" : "zoom out")}");
        
        // Could be used for accessibility features or special game controls
        // For now, just log it
    }

    public void SetScreenOrientation(ScreenOrientation orientation)
    {
        Screen.orientation = orientation;
        Debug.Log($"[MobilePlatform] Screen orientation set to: {orientation}");
    }

    public void EnableAutoRotation(bool enable)
    {
        autoRotation = enable;
        if (enable)
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
        }
    }

    public bool IsTablet()
    {
        #if UNITY_ANDROID
        // Check if device is a tablet based on screen size and DPI
        float screenWidth = Screen.width / Screen.dpi;
        float screenHeight = Screen.height / Screen.dpi;
        float screenSize = Mathf.Sqrt(screenWidth * screenWidth + screenHeight * screenHeight);
        return screenSize >= 7.0f; // 7 inches or larger is considered tablet
        #elif UNITY_IOS
        // On iOS, check device model
        return UnityEngine.iOS.Device.generation.ToString().Contains("iPad");
        #else
        return false;
        #endif
    }

    public DeviceType GetDeviceType()
    {
        if (IsTablet())
            return DeviceType.Tablet;
        else if (Application.isMobilePlatform)
            return DeviceType.Phone;
        else
            return DeviceType.Desktop;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        Debug.Log($"[MobilePlatform] Application pause: {pauseStatus}");
        
        if (pauseStatus)
        {
            // App was paused, save game state
            ServiceLocator.Save?.Save();
        }
        else
        {
            // App was resumed, could refresh data or check for updates
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        Debug.Log($"[MobilePlatform] Application focus: {hasFocus}");
        
        if (!hasFocus)
        {
            // App lost focus, pause audio and save
            AudioListener.pause = true;
            ServiceLocator.Save?.Save();
        }
        else
        {
            // App gained focus, resume audio
            AudioListener.pause = false;
        }
    }
}

[System.Serializable]
public class OrientationChangeEvent
{
    public ScreenOrientation newOrientation;
}

public enum DeviceType
{
    Phone,
    Tablet,
    Desktop
}