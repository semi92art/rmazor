// ReSharper disable InconsistentNaming

using Common;
using Common.Helpers;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;

namespace RMAZOR
{
    public interface ISRDebuggerInitializer : IInit { }
    
    public class SRDebuggerInitializer : InitBase, ISRDebuggerInitializer
    {
        private GlobalGameSettings      GlobalGameSettings { get; }
        private IRemotePropertiesCommon RemoteProperties   { get; }

        public SRDebuggerInitializer(
            GlobalGameSettings      _GlobalGameSettings,
            IRemotePropertiesCommon _RemoteProperties)
        {
            GlobalGameSettings = _GlobalGameSettings;
            RemoteProperties   = _RemoteProperties;
        }

        public override void Init()
        {
            SRDebug.Instance.IsTriggerEnabled = RemoteProperties.DebugEnabled;
            base.Init();
        }
    }
}