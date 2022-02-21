using System.Collections.Generic;
using Common.CameraProviders;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;

namespace RMAZOR.Views.UI.StartLogo
{
    public class ViewUIStartLogoMazeBlade : ViewUIStartLogoBase
    {
        #region nonpublic members

        protected override float  AnimationSpeed => 1.5f;
        protected override string PrefabName     => "start_logo_maze_blade" + (GraphicUtils.AspectRatio < 0.6f ? string.Empty : "_2");

        protected override Dictionary<string, float> KeysAndDelays => new Dictionary<string, float>
        {
            {"M", 0}, {"A1", 0}, {"Z", 0}, {"E1", 0}, {"B", 0}, {"L", 0}, {"A2", 0}, {"D", 0}, {"E2", 0}
        };
        
        #endregion
        
        #region inject

        public ViewUIStartLogoMazeBlade(
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

        #region nonpublic members

        protected override void InitStartLogo()
        {
            base.InitStartLogo();
            var trigerrer = StartLogoObj.GetCompItem<AnimationTriggerer>("trigerrer");
            trigerrer.Trigger1 += () => LogoShowingAnimationPassed = true;
            var rotator = StartLogoObj.GetCompItem<SimpleRotator>("rotator");
            rotator.Init(GameTicker, -2f);
        }
        
        protected override void SetColors(Color _Color)
        {
            base.SetColors(_Color);
            var rendKeys = new[]
            {
                "saw_1", "saw_2", "saw_3", 
                "spike_1", "spike_2", "spike_3", "spike_4", "spike_5", "spike_6"
            };
            foreach (string key in rendKeys)
                StartLogoObj.GetCompItem<SpriteRenderer>(key).color = _Color;
        }

        #endregion
    }
}