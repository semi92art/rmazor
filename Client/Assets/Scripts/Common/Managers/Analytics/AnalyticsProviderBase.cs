using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using Common.Helpers;

namespace Common.Managers.Analytics
{
    public interface IAnalyticsProvider : IInit
    {
        void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null);
    }
    
    public abstract class AnalyticsProviderBase : InitBase, IAnalyticsProvider
    {
        #region nonpublic members

        protected abstract Dictionary<string, string> ValidIdsAndNamesTranslations { get; }
        
        #endregion

        #region api
        
        public void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null)
        {
            if (ValidIdsAndNamesTranslations != null && !ValidIdsAndNamesTranslations.Keys.Contains(_AnalyticId))
                return;
            bool containsKey = true;
            string translatedId = ValidIdsAndNamesTranslations == null ? 
                _AnalyticId : ValidIdsAndNamesTranslations.GetSafe(_AnalyticId, out containsKey);
            if (!containsKey)
                return;
            if (_EventData == null)
            {
                SendAnalyticCore(translatedId);
                return;
            }
            var translatedEventData = new Dictionary<string, object>();
            foreach ((string key, var value) in _EventData)
            {
                string translatedParameter = ValidIdsAndNamesTranslations == null ?
                    key : ValidIdsAndNamesTranslations.GetSafe(key, out containsKey);
                if (!containsKey)
                    continue;
                translatedEventData.Add(translatedParameter, value);
            }
            SendAnalyticCore(translatedId, translatedEventData);
        }

        #endregion

        #region nonpublic methods
        protected abstract void SendAnalyticCore(
            string _AnalyticId,
            IDictionary<string, object> _EventData = null);
        
        #endregion
    }
}