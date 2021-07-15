using Utils;

namespace TimeProviders
{
    public interface IUiTimeProvider : ITimeProvider { }
    
    public class UiTimeProvider : TimeProviderBase, IUiTimeProvider
    {
        private static UiTimeProvider _instance;
        public static UiTimeProvider Instance => CommonUtils.MonoBehSingleton(ref _instance, "UI Time Provider");
    
        //private UiTimeProvider() { }
    }
}