using System;
using UnityEngine;

public static class ServiceLocator 
{
    static EconomyService _economy;
    static SaveService _save;
    static EventBus _bus;
    static GameGenerationService _gameGeneration;
    static PerformanceManager _performance;

    public static void Init() 
    {
        _save ??= new SaveService();
        _economy ??= new EconomyService(_save);
        _bus ??= new EventBus();
        _gameGeneration ??= new GameGenerationService();
        _performance ??= new PerformanceManager();
        
        Debug.Log("[ServiceLocator] All services initialized successfully");
    }

    public static EconomyService Economy => _economy;
    public static SaveService Save => _save;
    public static EventBus Bus => _bus;
    public static GameGenerationService GameGeneration => _gameGeneration;
    public static PerformanceManager Performance => _performance;

    public static void Cleanup()
    {
        _performance?.Cleanup();
        _gameGeneration?.Cleanup();
        _economy?.Cleanup();
        _save?.Save();
        _bus?.Cleanup();
    }
}