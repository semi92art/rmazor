#if UNITY_ANDROID
using Common;
using Common.Helpers;
using Common.Utils;
using Google.Android.PerformanceTuner;

namespace RMAZOR
{
    public interface IAndroidPerformanceTunerClient : IInit { }
    
    public class AndroidPerformanceTunerClient : InitBase, IAndroidPerformanceTunerClient
    {
        private readonly AndroidPerformanceTuner<FidelityParams, Annotation> m_Tuner =
            new AndroidPerformanceTuner<FidelityParams, Annotation>();

        public override void Init()
        {
            Cor.Run(Cor.WaitNextFrame(
                InitTuner,
                true));
            InitTuner();
            base.Init();
        }

        private void InitTuner()
        {
            var startErrorCode = m_Tuner.Start();
            Dbg.Log("Android Performance Tuner started with code: " + startErrorCode);
            m_Tuner.onReceiveUploadLog += _Request =>
            {
                Dbg.Log("Telemetry uploaded with request name: " + _Request.name);
            };
        }
    }
}
#endif