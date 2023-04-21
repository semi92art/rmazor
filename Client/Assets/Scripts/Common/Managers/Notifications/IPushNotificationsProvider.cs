using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;

namespace Common.Managers.Notifications
{
    public interface IPushNotificationsProvider : IInit { }
    
    public class PushNotificationsProviderFake : InitBase, IPushNotificationsProvider { }
}