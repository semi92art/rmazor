using System.Collections.Generic;
using Common.Managers.Advertising;
using Common.Managers.Notifications;

namespace Common.Helpers
{
    public interface IRemotePropertiesCommon
    {
        bool                    DebugEnabled  { get; set; }
        IList<AdProviderInfo>   AdsProviders  { get; set; }
        IList<NotificationInfo> Nofifications { get; set; }  
    }
    
    public class RemotePropertiesCommon : IRemotePropertiesCommon
    {
        public bool                    DebugEnabled  { get; set; }
        public string                  TestDeviceIds { get; set; }
        public IList<AdProviderInfo>   AdsProviders  { get; set; }
        public IList<NotificationInfo> Nofifications { get; set; }  
    }
}