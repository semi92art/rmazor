using System.Collections;
using mazing.common.Runtime;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Settings;
using RMAZOR.Views.UI.Game_Logo;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnExitLevelStaging : IInit
    {
        void OnExitLevelStaging(LevelStageArgs _Args);
    }
    
    public class ViewLevelStageControllerOnExitLevelStaging :
        ViewLevelStageControllerOnSingleStageBase, 
        IViewLevelStageControllerOnExitLevelStaging
    {
        #region inject

        private ViewSettings                ViewSettings           { get; }
        private IAudioManager               AudioManager           { get; }
        private IViewFullscreenTransitioner FullscreenTransitioner { get; }
        private IUITicker                   UiTicker               { get; }
        private IRetroModeSetting           RetroModeSetting       { get; }

        private ViewLevelStageControllerOnExitLevelStaging(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            IViewUIGameLogo             _GameLogo,
            IAudioManager               _AudioManager,
            IViewFullscreenTransitioner _FullscreenTransitioner,
            IUITicker                   _UIUiTicker,
            IRetroModeSetting           _RetroModeSetting) 
            : base(_Model, _GameLogo)
        {
            ViewSettings           = _ViewSettings;
            AudioManager           = _AudioManager;
            FullscreenTransitioner = _FullscreenTransitioner;
            UiTicker               = _UIUiTicker;
            RetroModeSetting       = _RetroModeSetting;
        }

        #endregion

        #region api

        public void OnExitLevelStaging(LevelStageArgs _Args)
        {
            AudioManager.PlayClip(AudioClipArgsMainMenuTheme);
            if (_Args.PreviousStage != ELevelStage.None)
            {
                AudioManager.StopClip(GetAudioClipArgsLevelTheme());
            }
            if (_Args.PreviousStage == ELevelStage.Unloaded)
                Cor.Run(HideFullscreenTransitionTextureCoroutine());
            bool mainGameModeLoadedAtLeastOnce = SaveUtils.GetValue(SaveKeysRmazor.MainGameModeLoadedAtLeastOnce);
            if (!ViewSettings.loadMainGameModeOnStart || mainGameModeLoadedAtLeastOnce)
                ShowGameLogoIfItWasNot();
            RetroModeSetting.UpdateState();
        }
        
        private IEnumerator HideFullscreenTransitionTextureCoroutine()
        {
            FullscreenTransitioner.DoTextureTransition(false, ViewSettings.betweenLevelTransitionTime);
            yield return Cor.Delay(ViewSettings.betweenLevelTransitionTime, UiTicker);
            FullscreenTransitioner.Enabled = false;
        }

        #endregion
    }
}