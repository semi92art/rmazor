using Common.Managers;
using Common.Managers.Advertising;
using Common.Managers.Analytics;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime.Debugging;
using mazing.common.Runtime.Managers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;

namespace RMAZOR
{
    public class DebugConsoleViewFake : MonoBehaviour, IDebugConsoleView
    {
        public event VisibilityChangedHandler VisibilityChanged;
        public bool                           Visible => false;

        public void Init(
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder,
            IAdsManager                 _AdsManager,
            IScoreManager               _ScoreManager,
            IAudioManager               _AudioManager,
            IAnalyticsManager           _AnalyticsManager,
            IFpsCounter                 _FpsCounter)
        { }

        public void EnableDebug(bool   _Enable)                                         { }
        public void SetVisibility(bool _Visible)                                        { }
        public void Monitor(string     _Name, bool _Enable, System.Func<object> _Value) { }
    }
}