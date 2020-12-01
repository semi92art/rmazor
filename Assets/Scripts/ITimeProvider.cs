public interface ITimeProvider
{
    float Time { get; }
    bool Pause { get; set; }
}

public abstract class TimeProviderBase : DI.DiObject, ITimeProvider, ISingleton
{
    public float Time { get; protected set; }
    public bool Pause { get; set; }
    
    protected float Delta;

    protected TimeProviderBase()
    {
        Time = UnityEngine.Time.deltaTime;
    }
    
    [DI.Update(-1, true)]
    protected virtual void OnUpdate()
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
    private static ITimeProvider _instance;
    public static  ITimeProvider Instance => _instance ?? (_instance = new UiTimeProvider());
}

public class GameTimeProvider : TimeProviderBase
{
    private static ITimeProvider _instance;
    public static  ITimeProvider Instance => _instance ?? (_instance = new GameTimeProvider());
}


