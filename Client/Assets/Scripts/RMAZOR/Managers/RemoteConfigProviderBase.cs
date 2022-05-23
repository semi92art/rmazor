using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Helpers;

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
            await FetchConfigs();
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