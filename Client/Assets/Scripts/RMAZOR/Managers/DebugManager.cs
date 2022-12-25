using System;
using Common;
using Common.Helpers;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime;
using mazing.common.Runtime.Debugging;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Settings;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;

namespace RMAZOR.Managers
{
    public interface IDebugManager : IInit
    {
        event VisibilityChangedHandler DebugConsoleVisibilityChanged;

        void        ShowDebugConsole();
        void        HideDebugConsole();
        void        Monitor(string _Name, bool _Enable, Func<object> _Value);
        IFpsCounter FpsCounter { get; }
    }

    public class DebugManager : InitBase, IDebugManager
    {
        #region nonpublic members

        private bool m_DebugConsoleInitialized;

        #endregion
        
        #region inject

        private IRemotePropertiesRmazor     RemoteProperties  { get; }
        private IModelGame                  Model             { get; }
        private IViewInputCommandsProceeder CommandsProceeder { get; }
        private IDebugSetting               DebugSetting      { get; }
        private IAdsManager                 AdsManager        { get; }
        private IScoreManager               ScoreManager      { get; }
        private IAudioManager               AudioManager      { get; }
        private IAnalyticsManager           AnalyticsManager  { get; }
        private IDebugConsoleView           DebugConsoleView  { get; }
        public  IFpsCounter                 FpsCounter        { get; }

        private DebugManager(
            IRemotePropertiesRmazor     _RemoteProperties,
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder,
            IDebugSetting               _DebugSetting,
            IAdsManager                 _AdsManager,
            IScoreManager               _ScoreManager,
            IAudioManager               _AudioManager,
            IAnalyticsManager           _AnalyticsManager,
            IDebugConsoleView           _DebugConsoleView,
            IFpsCounter                 _FpsCounter)
        {
            RemoteProperties  = _RemoteProperties;
            Model             = _Model;
            CommandsProceeder = _CommandsProceeder;
            DebugSetting      = _DebugSetting;
            AdsManager        = _AdsManager;
            ScoreManager      = _ScoreManager;
            AudioManager      = _AudioManager;
            AnalyticsManager  = _AnalyticsManager;
            DebugConsoleView  = _DebugConsoleView;
            FpsCounter        = _FpsCounter;
        }

        #endregion

        #region api

        public event VisibilityChangedHandler DebugConsoleVisibilityChanged;

        public void ShowDebugConsole()
        {
            DebugConsoleView.SetVisibility(true);
        }

        public void HideDebugConsole()
        {
            DebugConsoleView.SetVisibility(false);
        }

        public void Monitor(string _Name, bool _Enable, Func<object> _Value)
        {
            DebugConsoleView.Monitor(_Name, _Enable, _Value);
        }

        public override void Init()
        {
            if (Initialized)
                return;
            DebugSetting.ValueSet += EnableDebug;
            InitDebugConsole(false);
            FpsCounter.Init();
            EnableDebug(DebugSetting.Get());
            base.Init();
        }
    
        #endregion

        #region nonpublic methods

        private void InitDebugConsole(bool _Forced)
        {
            if (!Application.isEditor && !RemoteProperties.DebugEnabled && !_Forced)
                return;
            DebugConsoleView.VisibilityChanged += _Value =>
            {
                CommandsProceeder.RaiseCommand(
                    _Value ? EInputCommand.DisableDebug : EInputCommand.EnableDebug,
                    null,
                    true);
                DebugConsoleVisibilityChanged?.Invoke(_Value);
            };
            DebugConsoleView.Init(
                Model, 
                CommandsProceeder,
                AdsManager,
                ScoreManager,
                AudioManager,
                AnalyticsManager,
                FpsCounter);
            m_DebugConsoleInitialized = true;
        }
    
        private void EnableDebug(bool _Enable)
        {
            if (!m_DebugConsoleInitialized && _Enable)
                InitDebugConsole(true);
            DebugConsoleView.EnableDebug(_Enable);
        }

        #endregion
    }
}