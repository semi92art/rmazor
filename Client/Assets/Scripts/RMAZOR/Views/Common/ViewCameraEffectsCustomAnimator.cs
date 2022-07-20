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
        
        private ViewSettings            ViewSettings     { get; }
        private IRemotePropertiesRmazor RemoteProperties { get; }
        private ICameraProvider         CameraProvider   { get; }
        private IColorProvider          ColorProvider    { get; }

        public ViewCameraEffectsCustomAnimator(
            ViewSettings            _ViewSettings,
            IRemotePropertiesRmazor _RemoteProperties,
            ICameraProvider         _CameraProvider,
            IColorProvider          _ColorProvider)
        {
            ViewSettings     = _ViewSettings;
            RemoteProperties = _RemoteProperties;
            CameraProvider   = _CameraProvider;
            ColorProvider    = _ColorProvider;
        }

        #endregion

        #region api

        public override void Init()
        {
            ColorProvider.ColorChanged += OnColorChanged;
            base.Init();
            CameraProvider.EnableEffect(ECameraEffect.AntiAliasing, true);
            var fxaaProps = new FxaaProps {Sharpness = 2f, Threshold = 0.3f};
            CameraProvider.SetEffectProps(ECameraEffect.AntiAliasing, fxaaProps);
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            SetColorGradingProps(_Args);
        }

        public void AnimateCameraEffectsOnBetweenLevelTransition(bool _Appear)
        {
            AnimateColorGradingPropsOnBetweenLevel(_Appear);
        }

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
                VignetteAmount = 0.05f
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

        #endregion
    }
}