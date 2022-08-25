using System;
using Common;
using Common.Debugging;
using Common.Helpers;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Managers.PlatformGameServices;
using Common.Settings;
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