﻿using Common;
using Common.Helpers;
using Common.Managers.Advertising;
using Common.Managers.Scores;
using Common.Settings;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.Managers
{
    public interface IDebugManager : IInit
    {
        event VisibilityChangedHandler VisibilityChanged;
    }

    public class DebugManager : InitBase, IDebugManager
    {
        #region inject

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private CommonGameSettings          Settings          { get; }
        private IModelGame                  Model             { get; }
        private IViewInputCommandsProceeder CommandsProceeder { get; }
        private IDebugSetting               DebugSetting      { get; }
        private IAdsManager                 AdsManager        { get; }
        private IScoreManager               ScoreManager      { get; }

        public DebugManager(
            CommonGameSettings          _Settings,
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder,
            IDebugSetting               _DebugSetting,
            IAdsManager                 _AdsManager,
            IScoreManager               _ScoreManager)
        {
            Settings          = _Settings;
            Model             = _Model;
            CommandsProceeder = _CommandsProceeder;
            DebugSetting      = _DebugSetting;
            AdsManager        = _AdsManager;
            ScoreManager      = _ScoreManager;
        }

        #endregion

        #region api

        public event VisibilityChangedHandler VisibilityChanged;
    
        public override void Init()
        {
            DebugConsoleView.Instance.VisibilityChanged += _Value => VisibilityChanged?.Invoke(_Value);
            DebugSetting.OnValueSet = EnableDebug;
            InitDebugConsole();
            EnableDebug(DebugSetting.Get());
            base.Init();
        }
    
        #endregion

        #region nonpublic methods

        private void InitDebugConsole()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            DebugConsoleView.Instance.Init(Model, CommandsProceeder, AdsManager, ScoreManager);
#else
        if (Settings.DebugEnabled)
            DebugConsoleView.Instance.Init(Model, CommandsProceeder, AdsManager, ScoreManager);
#endif
        }
    
        private static void EnableDebug(bool _Enable)
        {
            DebugConsoleView.Instance.enabled = _Enable;
        }

        #endregion
    }
}