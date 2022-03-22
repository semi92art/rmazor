using System;
using System.Collections.Generic;
using Common.Enums;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public class ViewMazeItemSpringboardHighlighted : ViewMazeItemSpringboard
    {
        private Color m_NonHighlightColor;
        private Color m_HighlightColor;
        private float m_HighlightTimer;
        private bool  m_IsAnimatedHighlight;
        
        public ViewMazeItemSpringboardHighlighted(
            ViewSettings                  _ViewSettings,
            IModelGame                    _Model,
            IMazeCoordinateConverter      _CoordinateConverter,
            IContainersGetter             _ContainersGetter,
            IViewGameTicker               _GameTicker,
            IViewBetweenLevelTransitioner _Transitioner,
            IManagersGetter               _Managers,
            IColorProvider                _ColorProvider,
            IViewInputCommandsProceeder   _CommandsProceeder)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider,
                _CommandsProceeder) { }
        
        public override void UpdateTick()
        {
            m_HighlightTimer = GameTicker.TimeUnscaled;
            if (!m_IsAnimatedHighlight)
                return;
            if (!ActivatedInSpawnPool)
                return;
            if (AppearingState != EAppearingState.Appeared)
                return;
            if (Model.LevelStaging.LevelStage == ELevelStage.Finished)
                return;
            float coeff = Mathf.Cos(m_HighlightTimer * 10f) * 0.5f + 0.5f;
            var color = Color.Lerp(m_NonHighlightColor, m_HighlightColor, coeff);
            Springboard.Color = Pillar.Color = color;
        }
        
        protected override void InitShape()
        {
            base.InitShape();
            m_IsAnimatedHighlight = ViewSettings.springboardAnimatedHighlight;
            m_NonHighlightColor  = Pillar.Color = Springboard.Color =
                ColorProvider.GetColor(m_IsAnimatedHighlight ? ColorIds.Main : ColorIds.MazeItem2);
            m_HighlightColor = ColorProvider.GetColor(ColorIds.MazeItem2);

        }
        
        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.Main)
            {
                if (m_IsAnimatedHighlight)
                    m_NonHighlightColor = _Color;
                else
                {
                    Pillar.Color = _Color;
                    Springboard.Color = _Color;
                }
            }
            else if (_ColorId == ColorIds.MazeItem2)
            {
                if (m_IsAnimatedHighlight)
                    m_HighlightColor = _Color;
                else
                {
                    Pillar.Color = _Color;
                    Springboard.Color = _Color;
                }
            }
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            return new Dictionary<IEnumerable<Component>, Func<Color>> {{Shapes, () => m_NonHighlightColor}};
        }
    }
}