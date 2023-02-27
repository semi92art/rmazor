using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemSpringboard : IViewMazeItem
    {
        void MakeJump(SpringboardEventArgs _Args);
    }
    
    public class ViewMazeItemSpringboard : ViewMazeItemBase, IViewMazeItemSpringboard, IUpdateTick
    {
        #region constants
        
        private const float  SpringboardHeight = 0.15f;
        private const float  SpringboardWidth  = 0.3f;
        private const float  JumpCoefficient   = 0.7f;

        #endregion
        
        #region nonpublic members

        protected override string ObjectName => "Springboard Block";
        private static AudioClipArgs AudioClipArgsSpringboardJump => 
            new AudioClipArgs("springboard_jump", EAudioClipType.GameSound);
        
        private Vector2 m_Edge1Start,        m_Edge2Start;
        private Line    m_Springboard,       m_Pillar;
        private Color   m_NonHighlightColor, m_HighlightColor;
        private float   m_HighlightTimer;
        
        #endregion
        
        #region inject

        private ViewMazeItemSpringboard(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            ICoordinateConverter        _CoordinateConverter,
            IContainersGetter           _ContainersGetter,
            IViewGameTicker             _GameTicker,
            IRendererAppearTransitioner _Transitioner,
            IManagersGetter             _Managers,
            IColorProvider              _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder)
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

        #endregion

        #region api
        
        public override Component[] Renderers => new Component[] {m_Springboard, m_Pillar};
        
        public override object Clone() => new ViewMazeItemSpringboard(
            ViewSettings, 
            Model, 
            CoordinateConverter, 
            ContainersGetter,
            GameTicker,
            Transitioner,
            Managers,
            ColorProvider,
            CommandsProceeder);

        public void MakeJump(SpringboardEventArgs _Args)
        {
            Managers.AudioManager.PlayClip(AudioClipArgsSpringboardJump);
            Cor.Run(JumpCoroutine());
        }

        public void UpdateTick()
        {
            const float rate = 3f;
            m_HighlightTimer = GameTicker.Time;
            if (!ActivatedInSpawnPool
            || AppearingState != EAppearingState.Appeared)
            {
                return;
            }
            float coeff = Mathf.Cos(m_HighlightTimer * rate) * 0.5f + 0.5f;
            var color = Color.Lerp(m_NonHighlightColor, m_HighlightColor, coeff);
            m_Springboard.Color = m_Pillar.Color = color;
        }

        #endregion

        #region nonpublic methods
        
        protected override void InitShape()
        {
            m_Pillar = Object.AddComponentOnNewChild<Line>("Springboard Item", out _)
                .SetSortingOrder(SortingOrders.PathLine)
                .SetEndCaps(LineEndCap.Round);
            m_Springboard = Object.AddComponentOnNewChild<Line>("Springboard", out _)
                .SetSortingOrder(GetSortingOrder())
                .SetEndCaps(LineEndCap.Round);
        }

        protected override void UpdateShape()
        {
            m_NonHighlightColor = ColorProvider.GetColor(ColorIds.Main);
            m_HighlightColor    = ColorProvider.GetColor(ColorIds.MazeItem2);
            m_Pillar.SetThickness(ViewSettings.LineThickness * CoordinateConverter.Scale);
            m_Springboard.SetThickness(m_Pillar.Thickness * 2f);
            (m_Pillar.Start, m_Pillar.End, m_Springboard.Start, m_Springboard.End) = GetSpringboardAndPillarEdges();
            (m_Edge1Start, m_Edge2Start) = (m_Springboard.Start, m_Springboard.End);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            switch (_ColorId)
            {
                case ColorIds.Main:      m_NonHighlightColor = _Color; break;
                case ColorIds.MazeItem2: m_HighlightColor    = _Color; break;
            }
        }

        private IEnumerator JumpCoroutine()
        {
            void DoOnProgress(float _Progress)
            {
                (m_Springboard.Start, m_Springboard.End, m_Pillar.End) =
                    GetSpringboardEdgesOnJump(_Progress * JumpCoefficient);
            }
            yield return Cor.Lerp(
                GameTicker,
                0.1f,
                _OnProgress: DoOnProgress,
                _ProgressFormula: _P => _P < 0.5f ? 2f * _P : 2f * (1f - _P));
        }
        
        private Tuple<Vector2, Vector2, Vector2, Vector2> GetSpringboardAndPillarEdges()
        {
            Vector2 v = Props.Directions.First();
            var vOrth = new Vector2(-v.x, v.y);
            var a = -v * 0.4f;
            var d = a + v * SpringboardHeight;
            var b = d - vOrth * SpringboardWidth * 0.5f;
            var c = d + vOrth * SpringboardWidth * 0.5f;
            float scale = CoordinateConverter.Scale;
            a *= scale;
            b *= scale;
            c *= scale;
            d *= scale;
            return new Tuple<Vector2, Vector2, Vector2, Vector2>(a, d, b, c);
        }

        private Tuple<Vector2, Vector2, Vector2> GetSpringboardEdgesOnJump(float _C)
        {
            Vector2 v = Props.Directions.First();
            var edge1 = m_Edge1Start + v * _C;
            var edge2 = m_Edge2Start + v * _C;
            var pillarEdge = (edge1 + edge2) * 0.5f;
            return new Tuple<Vector2, Vector2, Vector2>(edge1, edge2, pillarEdge);
        }
        
        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var col = ColorProvider.GetColor(GetMazeItemBlockColorId());
            return new Dictionary<IEnumerable<Component>, Func<Color>> {{new [] {m_Springboard, m_Pillar}, () => col}};
        }
        
        #endregion
    }
}