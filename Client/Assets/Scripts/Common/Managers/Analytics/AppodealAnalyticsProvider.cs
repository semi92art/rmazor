#if APPODEAL_3
using System.Collections.Generic;
using System.Linq;
using AppodealStack.Monetization.Api;

namespace Common.Managers.Analytics
{
    public interface IAppodealAnalyticsProvider : IAnalyticsProvider { }
    
    public class AppodealAnalyticsProvider : AnalyticsProviderBase, IAppodealAnalyticsProvider
    {
        protected override Dictionary<string, string> ValidIdsAndNamesTranslations => null;

        protected override void SendAnalyticCore(string _AnalyticId, IDictionary<string, object> _EventData = null)
        {
            if (_EventData == null)
            {
                Appodeal.LogEvent(_AnalyticId);
                return;
            }
            var validEventData = _EventData
                .ToDictionary(
                    _Kvp => _Kvp.Key,
                    _Kvp => _Kvp.Value);
            Appodeal.LogEvent(_AnalyticId, validEventData);
        }
    }
}
#endif