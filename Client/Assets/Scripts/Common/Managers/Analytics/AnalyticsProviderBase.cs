using System.Collections.Generic;
using System.Linq;
using Common.Helpers;

namespace Common.Managers.Analytics
{
    public interface IAnalyticsProvider : IInit
    {
        void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null);
    }
    
    public abstract class AnalyticsProviderBase : InitBase, IAnalyticsProvider
    {
        #region api
        
        public void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null)
        {
            string realAnalyticId = GetRealAnalyticId(_AnalyticId);
            var translatedEventData = _EventData?.ToDictionary(
                _Kvp =>
                    GetRealParameterId(_Kvp.Key),
                _Kvp => _Kvp.Value);
            SendAnalyticCore(realAnalyticId, translatedEventData);
        }

        #endregion

        #region nonpublic methods
        protected abstract void   SendAnalyticCore(string   _AnalyticId, IDictionary<string, object> _EventData = null);
        protected abstract string GetRealAnalyticId(string  _AnalyticId);
        protected abstract string GetRealParameterId(string _ParameterId);

        #endregion
    }
}