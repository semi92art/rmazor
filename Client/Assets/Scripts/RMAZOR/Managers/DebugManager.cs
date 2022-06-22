using System;
using Common;
using Common.Helpers;
using Common.Managers.Advertising;
using Common.Managers.Scores;
using Common.Settings;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;

namespace RMAZOR.Managers
{
    public interface IDebugManager : IInit
    {
        event VisibilityChangedHandler VisibilityChanged;
        void                           Monitor(string _Name, bool _Enable, Func<object> _Value);
    }

    public class DebugManager : InitBase, IDebugManager
    {
        #region inject

        private IRemotePropertiesRmazor     RemoteProperties  { get; }
        private IModelGame                  Model             { get; }
        private IViewInputCommandsProceeder CommandsProceeder { get; }
        private IDebugSetting               DebugSetting      { get; }
        private IAdsManager                 AdsManager        { get; }
        private IScoreManager               ScoreManager      { get; }

        private DebugManager(
            IRemotePropertiesRmazor     _RemoteProperties,
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder,
            IDebugSetting               _DebugSetting,
            IAdsManager                 _AdsManager,
            IScoreManager               _ScoreManager)
        {
            RemoteProperties  = _RemoteProperties;
            Model             = _Model;
            CommandsProceeder = _CommandsProceeder;
            DebugSetting      = _DebugSetting;
            AdsManager        = _AdsManager;
            ScoreManager      = _ScoreManager;
        }

        #endregion

        #region api

        public event VisibilityChangedHandler VisibilityChanged;
        
        public void Monitor(string _Name, bool _Enable, Func<object> _Value)
        {
            DebugConsoleView.Instance.Monitor(_Name, _Enable, _Value);
        }

        public override void Init()
        {
            if (Initialized)
                return;
            DebugSetting.ValueSet += EnableDebug;
            InitDebugConsole();
            EnableDebug(DebugSetting.Get());
            base.Init();
        }
    
        #endregion

        #region nonpublic methods

        private void InitDebugConsole()
        {
            if (!Application.isEditor && !RemoteProperties.DebugEnabled)
                return;
            var instance = DebugConsoleView.Instance;
            instance.VisibilityChanged += _Value =>
            {
                CommandsProceeder.RaiseCommand(
                    _Value ? EInputCommand.DisableDebug : EInputCommand.EnableDebug,
                    null,
                    true);
                VisibilityChanged?.Invoke(_Value);
            };
            instance.Init(Model, CommandsProceeder, AdsManager, ScoreManager);
        }
    
        private static void EnableDebug(bool _Enable)
        {
            DebugConsoleView.Instance.EnableDebug(_Enable);
        }

        #endregion
    }
}