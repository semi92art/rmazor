// ReSharper disable InconsistentNaming

using Common;
using Common.Helpers;

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
            SRDebug.Instance.IsTriggerEnabled = GlobalGameSettings.apkForAppodeal ||
                                                RemoteProperties.DebugEnabled;
            base.Init();
        }
    }
}