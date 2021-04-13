using Utils;

namespace TimeProviders
{
    public class UiTimeProvider : TimeProviderBase
    {
        private static UiTimeProvider _instance;
        public static UiTimeProvider Instance => CommonUtils.MonoBehSingleton(ref _instance, "UI Time Provider");
    
        //private UiTimeProvider() { }
    }
}