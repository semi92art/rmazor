using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Entities;
using Common.Helpers;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Utils;

namespace RMAZOR.Managers
{
    public interface IRemoteConfigProvider : IInit
    {
        void                                  SetRemoteCachedPropertyInfos(List<RemoteConfigPropertyInfo> _Infos);
        IEnumerable<RemoteConfigPropertyInfo> GetFetchedInfos();
    }
    
    public abstract class RemoteConfigProviderBase : InitBase, IRemoteConfigProvider
    {
        private List<RemoteConfigPropertyInfo> m_Infos;
        
        public override async void Init()
        {
            if (NetworkUtils.IsInternetConnectionAvailable())
                await FetchConfigs();
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

        protected abstract Task FetchConfigs();
        
        protected void OnFetchConfigsCompletedSuccessfully()
        {
            foreach (var info in m_Infos)
                GetRemoteConfig(info);
            base.Init();
        }

        protected abstract void GetRemoteConfig(RemoteConfigPropertyInfo _Info);
    }
}