using Common;
using Common.CameraProviders;
using Common.CameraProviders.Camera_Effects_Props;
using Common.Helpers;
using Common.Providers;
using RMAZOR.Models;
using UnityEngine;

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
        private ICameraProvider                      CameraProvider              { get; }
        private IColorProvider                       ColorProvider               { get; }
        private IViewMazeBackgroundTextureController BackgroundTextureController { get; }

        public ViewCameraEffectsCustomAnimator(
            ViewSettings                         _ViewSettings,
            IRemotePropertiesRmazor              _RemoteProperties,
            ICameraProvider                      _CameraProvider,
            IColorProvider                       _ColorProvider,
            IViewMazeBackgroundTextureController _BackgroundTextureController)
        {
            ViewSettings                = _ViewSettings;
            RemoteProperties            = _RemoteProperties;
            CameraProvider              = _CameraProvider;
            ColorProvider               = _ColorProvider;
            BackgroundTextureController = _BackgroundTextureController;
        }

        #endregion

        #region api

        public override void Init()
        {
#if UNITY_EDITOR
            CommonDataRmazor.CameraEffectsCustomAnimator = this;
#endif
            ColorProvider.ColorChanged += OnColorChanged;
            
            CameraProvider.EnableEffect(ECameraEffect.Bloom, true);
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
        
        #if UNITY_EDITOR

        public void SetBloom(BloomPropsAlt _BloomPropsAlt)
        {
            var props = _BloomPropsAlt.ToBloomProps(out bool enableBloom);
            CameraProvider.EnableEffect(ECameraEffect.Bloom, enableBloom);
            if (!enableBloom)
                return;
            CameraProvider.SetEffectProps(ECameraEffect.Bloom, props);
        }
        
        #endif

        #endregion

        #region nonpublic methods
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.PathFill)
            {
                var colorGradingProps = new ColorGradingProps {VignetteColor = _Color};
                CameraProvider.SetEffectProps(ECameraEffect.ColorGrading, colorGradingProps);
            }
        }

        private void SetColorGradingProps(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.Loaded)
                return;
            CameraProvider.EnableEffect(ECameraEffect.ColorGrading, true);
            var colorGradingProps = RemoteProperties.ColorGradingProps ?? new ColorGradingProps
            {
                Contrast       = 0.35f,
                Blur           = 0.2f,
                Saturation     = 0f,
                VignetteAmount = 0f
            };
            colorGradingProps.VignetteColor = ColorProvider.GetColor(ColorIds.PathFill);
            CameraProvider.SetEffectProps(ECameraEffect.ColorGrading, colorGradingProps);
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
            BackgroundTextureController.GetBackgroundColors(
                out _,
                out _,
                out _, 
                out _, 
                out _, 
                out _ ,
                out BloomPropsAlt bloomPropsAlt);
            var props = bloomPropsAlt.ToBloomProps(out bool enableBloom);
            CameraProvider.EnableEffect(ECameraEffect.Bloom, enableBloom);
            if (!enableBloom)
                return;
            CameraProvider.SetEffectProps(ECameraEffect.Bloom, props);
        }

        #endregion
    }
}