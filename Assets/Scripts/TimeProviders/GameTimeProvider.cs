using Utils;

namespace TimeProviders
{
    public class GameTimeProvider : TimeProviderBase
    {
        private static GameTimeProvider _instance;
        public static GameTimeProvider Instance  => CommonUtils.MonoBehSingleton(ref _instance, "Game Time Provider");
    
        //private GameTimeProvider() { }
    }
}