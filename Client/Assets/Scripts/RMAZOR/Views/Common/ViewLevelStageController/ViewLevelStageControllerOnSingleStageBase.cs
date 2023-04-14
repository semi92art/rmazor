using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Helpers;
using RMAZOR.Models;
using RMAZOR.Settings;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.UI.Game_Logo;
using RMAZOR.Views.Utils;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public abstract class ViewLevelStageControllerOnSingleStageBase : InitBase
    {
        #region nonpublic members

        protected AudioClipArgs GetAudioClipArgsLevelTheme()
        {
            string gameMode = ViewLevelStageSwitcherUtils.GetGameMode(Model.LevelStaging.Arguments);
            bool isRetro = gameMode == ParameterGameModeRandom || RetroModeSetting.Get();
            string prefabName = isRetro ? "synthwave_theme" : "main_theme";
            float volume = isRetro ? 0.1f : 0.25f;
            return new AudioClipArgs(prefabName,
                EAudioClipType.Music, 
                volume,
                true, 
                _AttenuationSecondsOnPlay: 1f,
                _AttenuationSecondsOnStop: 1f);
        }

        protected static AudioClipArgs AudioClipArgsMainMenuTheme =>
            new AudioClipArgs("main_menu_theme",
                EAudioClipType.Music, 
                0.25f,
                true);
        
        private bool m_StartLogoWasShown;

        #endregion

        #region inject

        protected IModelGame      Model    { get; }
        private   IViewUIGameLogo GameLogo { get; }
        
        [Zenject.Inject] private IRetroModeSetting RetroModeSetting { get; }

        protected ViewLevelStageControllerOnSingleStageBase(
            IModelGame      _Model,
            IViewUIGameLogo _GameLogo)
        {
            Model    = _Model;
            GameLogo = _GameLogo;
        }

        #endregion

        #region nonpublic methods

        protected static int GetGameModeAnalyticParameterValue(string _GameMode)
        {
            return _GameMode switch
            {
                ParameterGameModeMain           => 1,
                ParameterGameModeDailyChallenge => 2,
                ParameterGameModeRandom         => 3,
                ParameterGameModePuzzles        => 4,
                ParameterGameModeBigLevels      => 5,
                _                               => 0
            };
        }

        protected static int GetLevelTypeAnalyticParameterValue(string _LevelType)
        {
            return _LevelType switch
            {
                ParameterLevelTypeDefault => 1,
                ParameterLevelTypeBonus   => 2,
                _                         => 0
            };
        }
        
        protected void ShowGameLogoIfItWasNot()
        {
            if (m_StartLogoWasShown) 
                return;
            GameLogo.Show();
            m_StartLogoWasShown = true;
        }

        #endregion
    }
}