using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;

namespace RMAZOR.Managers
{
    public interface IRemoteConfigManager : IInit { }
    
    public class RemoteConfigManagerFake : InitBase, IRemoteConfigManager { }
}