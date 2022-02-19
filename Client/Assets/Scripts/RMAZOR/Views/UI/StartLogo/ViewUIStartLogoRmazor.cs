using System.Collections.Generic;
using Common.CameraProviders;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;
using Shapes;

namespace RMAZOR.Views.UI.StartLogo
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once UnusedType.Global
    public class ViewUIStartLogoRmazor : ViewUIStartLogoBase
    {
        #region nonpublic members

        protected override float  AnimationSpeed => 1.5f;
        protected override string PrefabName     => "start_logo_rmazor";

        protected override Dictionary<string, float> KeysAndDelays => new Dictionary<string, float>
        {
            {"R1", 0}, {"M", .1f}, {"A", .2f}, {"Z", .3f}, {"O", .5f}, {"R2", .4f}
        };

        #endregion
        
        #region inject

        public ViewUIStartLogoRmazor(
            ICameraProvider             _CameraProvider,
            IContainersGetter           _ContainersGetter,
            IViewInputCommandsProceeder _CommandsProceeder,
            IColorProvider              _ColorProvider,
            IViewGameTicker             _GameTicker,
            IPrefabSetManager           _PrefabSetManager)
            : base(
                _CameraProvider,
                _ContainersGetter,
                _CommandsProceeder,
                _ColorProvider,
                _GameTicker,
                _PrefabSetManager) { }

        #endregion

        #region nonpublic methods
        
        protected override void InitStartLogo()
        {
            base.InitStartLogo();
            var trigerrer = StartLogoObj.GetCompItem<AnimationTriggerer>("trigerrer");
            trigerrer.Trigger1 += () => LogoShowingAnimationPassed = true;
            var eye1 = StartLogoObj.GetCompItem<Rectangle>("eye_1");
            var eye2 = StartLogoObj.GetCompItem<Rectangle>("eye_2");
            eye1.Color = eye2.Color = ColorProvider.GetColor(ColorIds.Background);
        }

        protected override IEnumerable<ShapeRenderer> GetExceptedLogoColorObjects()
        {
            return new[]
            {
                StartLogoObj.GetCompItem<Rectangle>("eye_1"),
                StartLogoObj.GetCompItem<Rectangle>("eye_2")
            };
        }

        #endregion
    }
}