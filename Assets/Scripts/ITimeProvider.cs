public interface ITimeProvider
{
    float Time { get; }
    bool Pause { get; set; }
    void Reset();
}

public abstract class TimeProviderBase : DI.DiObject, ITimeProvider, ISingleton
{
    protected float Delta;
    
    public float Time { get; protected set; }
    public bool Pause { get; set; }

    public void Reset()
    {
        Time = 0;
        Delta = 0;
    }
    

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
    private UiTimeProvider() { }
}

public class GameTimeProvider : TimeProviderBase
{
    private static ITimeProvider _instance;
    public static  ITimeProvider Instance => _instance ?? (_instance = new GameTimeProvider());
    private GameTimeProvider() { }
}


