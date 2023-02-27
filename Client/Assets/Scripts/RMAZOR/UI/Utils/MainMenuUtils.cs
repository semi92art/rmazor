using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.CameraProviders.Camera_Effects_Props;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using UnityEngine;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.UI.Utils
{
    public static class MainMenuUtils
    {
        public static IEnumerator SubPanelsAdditionalCameraEffectsActionCoroutine(
            bool            _Appear,
            float           _Time,
            ICameraProvider _CameraProvider,
            IUnityTicker    _Ticker)
        {
            if (!_Appear)
            {
                // var colorGradingProps = new ColorGradingProps
                // {
                //     Contrast       = 0.35f,
                //     Blur           = 0.2f,
                //     Saturation     = 0f,
                //     VignetteAmount = 0f
                // };
                // _CameraProvider.EnableEffect(ECameraEffect.ColorGrading, true);
                // _CameraProvider.SetEffectProps(ECameraEffect.ColorGrading, colorGradingProps);
                yield break;
            }
            var effectsToDisable = Enum
                .GetValues(typeof(ECameraEffect))
                .Cast<ECameraEffect>()
                .Except(new [] {ECameraEffect.ColorGrading});
            foreach (var effect in effectsToDisable)
                _CameraProvider.EnableEffect(effect, false);
            float currTime = _Ticker.Time;
            while (_Ticker.Time < currTime + _Time)
            {
                float timeCoeff = (currTime + _Time - _Ticker.Time) / _Time;
                var depthOfFieldProps = new FastDofProps
                {
                    BlurAmount = (1 - timeCoeff) * 0.2f
                };
                _CameraProvider.SetEffectProps(ECameraEffect.DepthOfField, depthOfFieldProps);
                yield return new WaitForEndOfFrame();
            }
        }
        
        public static int GetTotalXpGot(IScoreManager _ScoreManager)
        {
            var savedGame = _ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName);
            object xpValue = savedGame.Arguments.GetSafe(KeyCharacterXp, out bool keyExist);
            if (!keyExist)
            {
                xpValue = 0;
                savedGame.Arguments.SetSafe(KeyCharacterXp, xpValue);
                _ScoreManager.SaveGame(savedGame);
            }
            int totalXpGot = Convert.ToInt32(xpValue);
            return totalXpGot;
        }
        
        public static IEnumerator LoadLevelCoroutine(
            Dictionary<string, object>  _Args,
            ViewSettings                _ViewSettings,
            IViewFullscreenTransitioner _FullscreenTransitioner,
            IUnityTicker                _Ticker,
            IViewLevelStageSwitcher     _LevelStageSwitcher)
        {
            _FullscreenTransitioner.Enabled = true;
            _FullscreenTransitioner.DoTextureTransition(true, _ViewSettings.betweenLevelTransitionTime);
            yield return Cor.Delay(_ViewSettings.betweenLevelTransitionTime, _Ticker);
            _LevelStageSwitcher.SwitchLevelStage(EInputCommand.LoadLevel, _Args);
        }
    }
}