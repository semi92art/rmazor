using System.Collections.Generic;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Utils;

namespace RMAZOR.Managers
{

    
    public abstract class RemoteConfigProviderBase : InitBase, IRemoteConfigProvider
    {
        private List<RemoteConfigPropertyInfo> m_Infos;
        
        public override void Init()
        {
            if (NetworkUtils.IsInternetConnectionAvailable())
                FetchConfigs();
            else 
                base.Init();
        }
        
        public void SetRemoteCachedPropertyInfos(List<RemoteConfigPropertyInfo> _Infos)
        {
            m_Infos = _Infos;
        }
        
        public IEnumerable<RemoteConfigPropertyInfo> GetFetchedInfos()
        {
            return m_Infos;
        }

        protected abstract void FetchConfigs();
        
        protected void OnFetchConfigsCompletedSuccessfully()
        {
            foreach (var info in m_Infos)
                GetRemoteConfig(info);
            base.Init();
        }

        protected abstract void GetRemoteConfig(RemoteConfigPropertyInfo _Info);
    }
}