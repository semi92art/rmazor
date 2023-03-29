using Common;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.CameraProviders.Camera_Effects_Props;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using RMAZOR.Models;
using RMAZOR.Settings;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.Utils;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.Common
{
    public interface IViewCameraEffectsCustomAnimator : IOnLevelStageChanged, IInit
    {
        void AnimateCameraEffectsOnBetweenLevelTransition(bool _Appear);
    }
    
    public class ViewCameraEffectsCustomAnimator : InitBase, IViewCameraEffectsCustomAnimator
    {
        #region inject
        
        private ViewSettings                         ViewSettings                { get; }
        private IRemotePropertiesRmazor              RemoteProperties            { get; }
        private IModelGame                           Model                       { get; }
        private ICameraProvider                      CameraProvider              { get; }
        private IColorProvider                       ColorProvider               { get; }
        private IViewMazeBackgroundTextureController BackgroundTextureController { get; }
        private IRetroModeSetting                    RetroModeSetting            { get; }

        public ViewCameraEffectsCustomAnimator(
            ViewSettings                         _ViewSettings,
            IRemotePropertiesRmazor              _RemoteProperties,
            IModelGame                           _Model,
            ICameraProvider                      _CameraProvider,
            IColorProvider                       _ColorProvider,
            IViewMazeBackgroundTextureController _BackgroundTextureController,
            IRetroModeSetting                    _RetroModeSetting)
        {
            ViewSettings                = _ViewSettings;
            RemoteProperties            = _RemoteProperties;
            Model                       = _Model;
            CameraProvider              = _CameraProvider;
            ColorProvider               = _ColorProvider;
            BackgroundTextureController = _BackgroundTextureController;
            RetroModeSetting            = _RetroModeSetting;
        }

        #endregion

        #region api

        public override void Init()
        {
#if UNITY_EDITOR
            CommonDataRmazor.CameraEffectsCustomAnimator = this;
#endif
            RetroModeSetting.ValueSet += OnRetroModeSettingValueSet;
            base.Init();
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            SetColorGradingProps(_Args);
            SetBloomProps(_Args);
        }

        public void AnimateCameraEffectsOnBetweenLevelTransition(bool _Appear)
        {
            AnimateColorGradingPropsOnBetweenLevel(_Appear);
        }
        
        public void SetBloom(BloomPropsArgs _BloomPropsArgs)
        {
            var props = _BloomPropsArgs.ToBloomProps(out bool enableBloom);
            CameraProvider.EnableEffect(ECameraEffect.Bloom, enableBloom);
            if (!enableBloom)
                return;
            CameraProvider.SetEffectProps(ECameraEffect.Bloom, props);
        }
        
        #endregion

        #region nonpublic methods
        
        private void OnRetroModeSettingValueSet(bool _Value)
        {
            if (Model.LevelStaging.LevelStage == ELevelStage.None)
                return;
            string gameMode = ViewLevelStageSwitcherUtils.GetGameMode(Model.LevelStaging.Arguments);
            EnableRetroModeEffects(_Value || gameMode == ParameterGameModeRandom);
        }

        private void SetColorGradingProps(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.Loaded)
                return;
            CameraProvider.EnableEffect(ECameraEffect.ColorGrading, true);
            var colorGradingProps = RemoteProperties.ColorGradingProps ?? GetDefaultColorGradingProps();
            colorGradingProps.VignetteColor = ColorProvider.GetColor(ColorIds.PathFill);
            CameraProvider.SetEffectProps(ECameraEffect.ColorGrading, colorGradingProps);
            string gameMode = ViewLevelStageSwitcherUtils.GetGameMode(_Args.Arguments);
            bool enableRetro = gameMode == ParameterGameModeRandom || RetroModeSetting.Get();
            EnableRetroModeEffects(enableRetro);
        }

        private void EnableRetroModeEffects(bool _Enable)
        {
            CameraProvider.EnableEffect(ECameraEffect.ChromaticAberration, _Enable);
            CameraProvider.EnableEffect(ECameraEffect.Glitch, _Enable);
            if (!_Enable)
                return;
            var chromaticAbbProps = GetRetroModeChromaticAberrationProps();
            var fastGlitchProps = GetRetroModeFastGlitchProps();
            CameraProvider.SetEffectProps(ECameraEffect.ChromaticAberration, chromaticAbbProps);
            CameraProvider.SetEffectProps(ECameraEffect.Glitch, fastGlitchProps);
        }

        private void AnimateColorGradingPropsOnBetweenLevel(bool _Appear)
        {
            var cgPropsFrom = new ColorGradingProps {VignetteSoftness = 0.001f};
            var cgPropsTo = new ColorGradingProps {VignetteSoftness = 0.5f};
            if (!_Appear)
                (cgPropsFrom, cgPropsTo) = (cgPropsTo, cgPropsFrom);
            CameraProvider.AnimateEffectProps(
                ECameraEffect.ColorGrading, 
                cgPropsFrom, 
                cgPropsTo, 
                ViewSettings.betweenLevelTransitionTime);
        }

        private void SetBloomProps(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.Loaded)
                return;
            var backgroundColorArgs = BackgroundTextureController.GetBackgroundColorArgs();
            var props = backgroundColorArgs.BloomPropsArgs.ToBloomProps(out bool enableBloom);
            // enableBloom &= Application.platform == RuntimePlatform.IPhonePlayer; // на андройде блум тормозит
            CameraProvider.EnableEffect(ECameraEffect.Bloom, enableBloom);
            if (!enableBloom)
                return;
            CameraProvider.SetEffectProps(ECameraEffect.Bloom, props);
        }

        private static ColorGradingProps GetDefaultColorGradingProps()
        {
            return new ColorGradingProps
            {
                Contrast       = 0.35f,
                Blur           = 0.2f,
                Saturation     = 0f,
                VignetteAmount = 0f
            };
        }

        private static ChromaticAberrationProps GetRetroModeChromaticAberrationProps()
        {
            return new ChromaticAberrationProps
            {
                RedX = -3.41f,
                RedY = -3.32f,
                FishEyeDistortion = -0.24f
            };
        }

        private static FastGlitchProps GetRetroModeFastGlitchProps()
        {
            return new FastGlitchProps
            {
                ChromaticGlitch = 0.03f,
                PixelGlitch = 0.035f
            };
        }

        #endregion
    }
}