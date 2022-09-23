using System.Collections.Generic;
using Common.Managers.Advertising;
using Common.Managers.Notifications;

namespace Common.Helpers
{
    public interface IRemotePropertiesCommon
    {
        bool                    DebugEnabled          { get; set; }
        List<string>            TestDeviceIdsForAdmob { get; set; }
        IList<AdProviderInfo>   AdsProviders          { get; set; }
        IList<NotificationInfo> Notifications         { get; set; }  
    }
    
    public class RemotePropertiesCommon : IRemotePropertiesCommon
    {
        public bool                    DebugEnabled          { get; set; }
        public List<string>            TestDeviceIdsForAdmob { get; set; }
        public IList<AdProviderInfo>   AdsProviders          { get; set; }
        public IList<NotificationInfo> Notifications         { get; set; }  
    }
}