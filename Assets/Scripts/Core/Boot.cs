using UnityEngine;
using UnityEngine.SceneManagement;

public class Boot : MonoBehaviour 
{
    [Header("Boot Configuration")]
    public bool enableVSync = true;
    public int targetFrameRate = 60;
    public bool enableMultiThreadedRendering = true;

    void Awake()
    {
        // Configure application settings
        if (enableVSync)
            QualitySettings.vSyncCount = 1;
        else
            Application.targetFrameRate = targetFrameRate;

        // Mobile optimizations
        #if UNITY_ANDROID || UNITY_IOS
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        #endif

        // Initialize all services
        ServiceLocator.Init();
        
        // Subscribe to application events
        Application.focusChanged += OnApplicationFocus;
        
        Debug.Log("[Boot] Application initialized successfully");
        
        // Load main hub
        SceneManager.LoadScene(SceneNames.Home);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            // Save when app loses focus
            ServiceLocator.Save?.Save();
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Save when app is paused
            ServiceLocator.Save?.Save();
        }
    }

    void OnDestroy()
    {
        Application.focusChanged -= OnApplicationFocus;
        ServiceLocator.Cleanup();
    }
}