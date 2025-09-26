using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using System.Collections;

/// <summary>
/// Manages performance optimization for handling thousands of mini-games on mobile devices
/// Includes memory management, object pooling, and resource monitoring
/// </summary>
public class PerformanceManager
{
    private readonly Dictionary<string, ObjectPool> _objectPools = new Dictionary<string, ObjectPool>();
    private readonly Queue<System.Action> _mainThreadActions = new Queue<System.Action>();
    private float _lastGCTime = 0f;
    private const float GC_INTERVAL = 30f; // Force GC every 30 seconds
    
    // Performance monitoring
    private float _frameTimeThreshold = 1f / 30f; // 30 FPS threshold
    private int _consecutiveSlowFrames = 0;
    private const int MAX_SLOW_FRAMES = 10;
    
    // Memory monitoring
    private long _lastMemoryUsage = 0;
    private float _memoryCheckInterval = 5f;
    private float _lastMemoryCheck = 0f;
    
    public PerformanceManager()
    {
        InitializeObjectPools();
        
        // Start performance monitoring
        if (Application.isPlaying)
        {
            MonoBehaviour coroutineRunner = GameObject.FindObjectOfType<MonoBehaviour>();
            if (coroutineRunner != null)
            {
                coroutineRunner.StartCoroutine(PerformanceMonitorCoroutine());
            }
        }
    }

    private void InitializeObjectPools()
    {
        // Create pools for common UI elements
        _objectPools["GameButton"] = new ObjectPool("GameButton", 100);
        _objectPools["ParticleEffect"] = new ObjectPool("ParticleEffect", 50);
        _objectPools["UINotification"] = new ObjectPool("UINotification", 20);
        _objectPools["AudioSource"] = new ObjectPool("AudioSource", 30);
        
        Debug.Log("[PerformanceManager] Object pools initialized");
    }

    public T GetPooledObject<T>(string poolName) where T : Component
    {
        if (_objectPools.ContainsKey(poolName))
        {
            return _objectPools[poolName].Get<T>();
        }
        
        Debug.LogWarning($"[PerformanceManager] Pool not found: {poolName}");
        return null;
    }

    public void ReturnToPool(string poolName, GameObject obj)
    {
        if (_objectPools.ContainsKey(poolName))
        {
            _objectPools[poolName].Return(obj);
        }
        else
        {
            // If no pool exists, just destroy it
            Object.Destroy(obj);
        }
    }

    public void OptimizeForMobile()
    {
        // Reduce quality settings for mobile
        QualitySettings.SetQualityLevel(0); // Fastest quality
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        
        // Disable shadows on mobile
        QualitySettings.shadows = ShadowQuality.Disable;
        
        // Reduce texture quality
        QualitySettings.masterTextureLimit = 1;
        
        // Disable anti-aliasing
        QualitySettings.antiAliasing = 0;
        
        // Optimize physics
        Physics.defaultSolverIterations = 4;
        Physics.defaultSolverVelocityIterations = 1;
        
        Debug.Log("[PerformanceManager] Mobile optimizations applied");
    }

    public void ForceGarbageCollection()
    {
        if (Time.time - _lastGCTime > GC_INTERVAL)
        {
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
            _lastGCTime = Time.time;
            
            Debug.Log("[PerformanceManager] Forced garbage collection");
        }
    }

    public void UnloadUnusedGameAssets()
    {
        // Unload unused addressable assets
        #if UNITY_ADDRESSABLES
        UnityEngine.AddressableAssets.Addressables.Release(default);
        #endif
        
        // Unload Unity assets
        Resources.UnloadUnusedAssets();
        
        Debug.Log("[PerformanceManager] Unused assets unloaded");
    }

    public void ExecuteOnMainThread(System.Action action)
    {
        lock (_mainThreadActions)
        {
            _mainThreadActions.Enqueue(action);
        }
    }

    public void ProcessMainThreadActions()
    {
        lock (_mainThreadActions)
        {
            while (_mainThreadActions.Count > 0)
            {
                var action = _mainThreadActions.Dequeue();
                try
                {
                    action.Invoke();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[PerformanceManager] Main thread action error: {e.Message}");
                }
            }
        }
    }

    public PerformanceMetrics GetPerformanceMetrics()
    {
        return new PerformanceMetrics
        {
            fps = 1f / Time.deltaTime,
            memoryUsage = Profiler.GetTotalAllocatedMemory(false),
            drawCalls = UnityEngine.Rendering.DebugUI.instance?.panelCount ?? 0,
            frameTime = Time.deltaTime * 1000f, // Convert to milliseconds
            pooledObjectsCount = GetTotalPooledObjects()
        };
    }

    private int GetTotalPooledObjects()
    {
        int total = 0;
        foreach (var pool in _objectPools.Values)
        {
            total += pool.ActiveCount + pool.InactiveCount;
        }
        return total;
    }

    private IEnumerator PerformanceMonitorCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            
            // Check frame time
            if (Time.deltaTime > _frameTimeThreshold)
            {
                _consecutiveSlowFrames++;
                if (_consecutiveSlowFrames >= MAX_SLOW_FRAMES)
                {
                    OnPerformanceIssueDetected("Consistent low framerate detected");
                    _consecutiveSlowFrames = 0;
                }
            }
            else
            {
                _consecutiveSlowFrames = 0;
            }
            
            // Check memory usage
            if (Time.time - _lastMemoryCheck > _memoryCheckInterval)
            {
                CheckMemoryUsage();
                _lastMemoryCheck = Time.time;
            }
            
            // Process main thread actions
            ProcessMainThreadActions();
            
            // Auto garbage collection
            ForceGarbageCollection();
        }
    }

    private void CheckMemoryUsage()
    {
        long currentMemory = Profiler.GetTotalAllocatedMemory(false);
        long memoryDelta = currentMemory - _lastMemoryUsage;
        
        // If memory increased by more than 50MB, warn about potential leak
        if (memoryDelta > 50 * 1024 * 1024)
        {
            OnPerformanceIssueDetected($"High memory usage increase: {memoryDelta / (1024 * 1024)}MB");
        }
        
        // If total memory is over 500MB on mobile, force cleanup
        if (Application.isMobilePlatform && currentMemory > 500 * 1024 * 1024)
        {
            OnPerformanceIssueDetected("High memory usage on mobile");
            UnloadUnusedGameAssets();
            ForceGarbageCollection();
        }
        
        _lastMemoryUsage = currentMemory;
    }

    private void OnPerformanceIssueDetected(string issue)
    {
        Debug.LogWarning($"[PerformanceManager] Performance issue: {issue}");
        ServiceLocator.Bus?.PublishPerformanceWarning(issue);
        
        // Auto-optimize when issues are detected
        AutoOptimize();
    }

    private void AutoOptimize()
    {
        // Reduce quality temporarily
        if (QualitySettings.GetQualityLevel() > 0)
        {
            QualitySettings.DecreaseLevel();
            Debug.Log("[PerformanceManager] Quality reduced for performance");
        }
        
        // Force asset cleanup
        UnloadUnusedGameAssets();
        
        // Clear object pools partially
        foreach (var pool in _objectPools.Values)
        {
            pool.ClearInactive();
        }
    }

    public void Cleanup()
    {
        foreach (var pool in _objectPools.Values)
        {
            pool.Dispose();
        }
        _objectPools.Clear();
        
        lock (_mainThreadActions)
        {
            _mainThreadActions.Clear();
        }
        
        Debug.Log("[PerformanceManager] Cleanup completed");
    }
}

/// <summary>
/// Simple object pool implementation for reusing GameObjects
/// </summary>
public class ObjectPool
{
    private readonly string _poolName;
    private readonly Queue<GameObject> _inactive = new Queue<GameObject>();
    private readonly HashSet<GameObject> _active = new HashSet<GameObject>();
    private readonly int _maxSize;

    public int ActiveCount => _active.Count;
    public int InactiveCount => _inactive.Count;

    public ObjectPool(string name, int maxSize = 100)
    {
        _poolName = name;
        _maxSize = maxSize;
    }

    public T Get<T>() where T : Component
    {
        GameObject obj;
        
        if (_inactive.Count > 0)
        {
            obj = _inactive.Dequeue();
            obj.SetActive(true);
        }
        else
        {
            // Create new object if pool is empty
            obj = new GameObject($"Pooled_{_poolName}");
            obj.AddComponent<T>();
        }
        
        _active.Add(obj);
        return obj.GetComponent<T>();
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return;
        
        if (_active.Remove(obj))
        {
            obj.SetActive(false);
            
            if (_inactive.Count < _maxSize)
            {
                _inactive.Enqueue(obj);
            }
            else
            {
                // Pool is full, destroy the object
                Object.Destroy(obj);
            }
        }
    }

    public void ClearInactive()
    {
        while (_inactive.Count > 0)
        {
            var obj = _inactive.Dequeue();
            if (obj != null)
                Object.Destroy(obj);
        }
    }

    public void Dispose()
    {
        ClearInactive();
        
        foreach (var obj in _active)
        {
            if (obj != null)
                Object.Destroy(obj);
        }
        
        _active.Clear();
    }
}

[System.Serializable]
public class PerformanceMetrics
{
    public float fps;
    public long memoryUsage;
    public int drawCalls;
    public float frameTime;
    public int pooledObjectsCount;
    
    public override string ToString()
    {
        return $"FPS: {fps:F1}, Memory: {memoryUsage / (1024 * 1024)}MB, Frame: {frameTime:F1}ms, Pooled: {pooledObjectsCount}";
    }
}