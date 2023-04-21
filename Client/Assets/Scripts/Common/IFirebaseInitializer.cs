using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;

namespace Common
{
    public interface IFirebaseInitializer : IInit
    {
        bool   DependenciesAreOk { get; }
        string DependencyStatus  { get; }
    }
    
    public class FirebaseInitializerFake : InitBase, IFirebaseInitializer
    {
        public bool   DependenciesAreOk => true;
        public string DependencyStatus  => default;
    }
}