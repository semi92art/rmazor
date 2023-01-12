using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Ticker;
using UnityEngine;

namespace RMAZOR.Views
{
    public interface IViewGameIdleQuitter : IInit { }
    
    public class ViewGameIdleQuitter : InitBase, IViewGameIdleQuitter, IApplicationPause
    {
        private float m_LastPauseTime;
        
        private ICommonTicker CommonTicker { get; }
        private ISystemTicker SystemTicker { get; }

        public ViewGameIdleQuitter(ICommonTicker _CommonTicker, ISystemTicker _SystemTicker)
        {
            CommonTicker = _CommonTicker;
            SystemTicker = _SystemTicker;
        }
        
        public override void Init()
        {
            m_LastPauseTime = SystemTicker.Time;
            CommonTicker.Register(this);
            base.Init();
        }

        public void OnApplicationPause(bool _Pause)
        {
            if (_Pause)
                m_LastPauseTime = SystemTicker.Time;
            else
            {
                float secondsLeft = SystemTicker.Time - m_LastPauseTime;
                if (secondsLeft / (60f * 60f) > 6f)
                    QuitGame();
            }
        }
        
        private static void QuitGame()
        {
            Application.Quit();
        }
    }
}