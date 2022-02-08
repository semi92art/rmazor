using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using Managers;
using Managers.Audio;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;
using UnityEngine.Events;

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
        private static AudioClipArgs _audioClipArgsSpringboardJump => 
            new AudioClipArgs("springboard_jump", EAudioClipType.GameSound);
        
        private Vector2 m_Edge1Start, m_Edge2Start;
        private Color   m_NonHighlightColor;
        private Color   m_HighlightColor;
        private float   m_HighlightTimer;
        private Line    m_Springboard;
        private Line    m_Pillar;
        private bool    m_IsAnimatedHighlight;
        
        #endregion
        
        #region inject
        
        public ViewMazeItemSpringboard(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            IMazeCoordinateConverter    _CoordinateConverter, 
            IContainersGetter           _ContainersGetter,
            IViewGameTicker             _GameTicker,
            IViewAppearTransitioner     _Transitioner,
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
        
        public override Component[] Shapes => new Component[] {m_Springboard, m_Pillar};
        
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
            Managers.AudioManager.PlayClip(_audioClipArgsSpringboardJump);
            Cor.Run(JumpCoroutine());
        }
        
        public void UpdateTick()
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
            m_Springboard.Color = m_Pillar.Color = color;
        }

        #endregion

        #region nonpublic methods
        
        protected override void InitShape()
        {
            m_Pillar = Object.AddComponentOnNewChild<Line>("Springboard Item", out _);
            m_Pillar.SortingOrder = SortingOrders.PathLine;
            m_Springboard = Object.AddComponentOnNewChild<Line>("Springboard", out _);
            m_Springboard.SortingOrder = SortingOrders.GetBlockSortingOrder(EMazeItemType.Springboard);
            m_Pillar.EndCaps = m_Springboard.EndCaps = LineEndCap.Round;
            m_IsAnimatedHighlight = ViewSettings.SpringboardAnimatedHighlight;
            m_NonHighlightColor  = m_Pillar.Color = m_Springboard.Color =
                ColorProvider.GetColor(m_IsAnimatedHighlight ? ColorIds.Main : ColorIds.MazeItem2);
            m_HighlightColor = ColorProvider.GetColor(ColorIds.MazeItem2);
        }

        protected override void UpdateShape()
        {
            m_Pillar.Thickness = ViewSettings.LineWidth * CoordinateConverter.Scale;
            m_Springboard.Thickness = m_Pillar.Thickness * 2f;
            (m_Pillar.Start, m_Pillar.End, m_Springboard.Start, m_Springboard.End) =
                GetSpringboardAndPillarEdges();
            m_Edge1Start = m_Springboard.Start;
            m_Edge2Start = m_Springboard.End;
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.Main)
            {
                if (m_IsAnimatedHighlight)
                    m_NonHighlightColor = _Color;
                else
                {
                    m_Pillar.Color = _Color;
                    m_Springboard.Color = _Color;
                }
            }
            else if (_ColorId == ColorIds.MazeItem2)
            {
                if (m_IsAnimatedHighlight)
                    m_HighlightColor = _Color;
                else
                {
                    m_Pillar.Color = _Color;
                    m_Springboard.Color = _Color;
                }
            }
        }

        private IEnumerator JumpCoroutine()
        {
            UnityAction<float> doOnProgress = _Progress => (m_Springboard.Start, m_Springboard.End, m_Pillar.End) =
                GetSpringboardEdgesOnJump(_Progress); 

            yield return Cor.Lerp(
                0,
                JumpCoefficient,
                0.05f,
                _Progress => doOnProgress(_Progress),
                GameTicker,
                (_, __) =>
                {
                    Cor.Run(Cor.Lerp(
                        JumpCoefficient,
                        0,
                        0.05f,
                        _Progress => doOnProgress(_Progress),
                        GameTicker));
                });
        }
        
        private Tuple<Vector2, Vector2, Vector2, Vector2> GetSpringboardAndPillarEdges()
        {
            Vector2 v = Props.Directions.First();
            var vOrth = new Vector2(-v.x, v.y);
            var a = -v * 0.4f;
            var d = a + v * SpringboardHeight;
            var b = d - vOrth * SpringboardWidth * 0.5f;
            var c = d + vOrth * SpringboardWidth * 0.5f;
            a *= CoordinateConverter.Scale;
            b *= CoordinateConverter.Scale;
            c *= CoordinateConverter.Scale;
            d *= CoordinateConverter.Scale;
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
            return new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {Shapes, () => m_NonHighlightColor}
            };
        }
        
        #endregion
    }
}