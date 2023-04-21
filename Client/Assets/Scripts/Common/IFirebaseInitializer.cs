using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;

namespace Common
{
    public interface IFirebaseInitializer : IInit { }
    
    public class FirebaseInitializerFake : InitBase, IFirebaseInitializer { }
}