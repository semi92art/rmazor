using Common;
using Common.Helpers;

namespace RMAZOR.Managers
{
    public interface IRemoteConfigManager : IInit { }
    
    public class RemoteConfigManagerFake : InitBase, IRemoteConfigManager { }
}