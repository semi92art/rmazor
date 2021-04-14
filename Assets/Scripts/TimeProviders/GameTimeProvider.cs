using Utils;

namespace TimeProviders
{
    public interface IGameTimeProvider : ITimeProvider { }
    
    public class GameTimeProvider : TimeProviderBase, IGameTimeProvider
    {
        private static GameTimeProvider _instance;
        public static GameTimeProvider Instance  => CommonUtils.MonoBehSingleton(ref _instance, "Game Time Provider");
    
        //private GameTimeProvider() { }
    }
}