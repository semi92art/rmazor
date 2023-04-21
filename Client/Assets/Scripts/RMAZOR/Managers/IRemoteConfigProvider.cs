using System.Collections.Generic;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;

namespace RMAZOR.Managers
{
    public interface IRemoteConfigProvider : IInit
    {
        void SetRemoteCachedPropertyInfos(List<RemoteConfigPropertyInfo> _Infos);
        
        IEnumerable<RemoteConfigPropertyInfo> GetFetchedInfos();
    }
    
    public class RemoteConfigProviderFake : InitBase, IRemoteConfigProvider
    {
        public void SetRemoteCachedPropertyInfos(List<RemoteConfigPropertyInfo> _Infos) { }

        public IEnumerable<RemoteConfigPropertyInfo> GetFetchedInfos()
        {
            return new List<RemoteConfigPropertyInfo>();
        }
    }
}