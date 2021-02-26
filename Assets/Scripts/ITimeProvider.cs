using UnityEngine;
using Utils;

public interface ITimeProvider
{
    float Time { get; }
    bool Pause { get; set; }
    void Reset();
}

public abstract class TimeProviderBase : MonoBehaviour, ITimeProvider, ISingleton
{
    protected float Delta;
    
    public float Time { get; protected set; }
    public bool Pause { get; set; }

    public void Reset()
    {
        Time = 0;
        Delta = 0;
    }
    
    protected virtual void Update()
    {
        if (Pause)
        {
            Delta += UnityEngine.Time.deltaTime;
            return;
        }
        Time = UnityEngine.Time.time - Delta;
    }
}

public class UiTimeProvider : TimeProviderBase
{
    private static UiTimeProvider _instance;
    public static  UiTimeProvider Instance => CommonUtils.MonoBehSingleton(ref _instance, "UI Time Provider");
    private UiTimeProvider() { }
}

public class GameTimeProvider : TimeProviderBase
{
    private static GameTimeProvider _instance;
    public static  GameTimeProvider Instance  => CommonUtils.MonoBehSingleton(ref _instance, "Game Time Provider");
    private GameTimeProvider() { }
}


