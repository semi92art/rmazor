using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.InputConfigurators;
using Shapes;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemSpringboard : IViewMazeItem
    {
        void MakeJump(SpringboardEventArgs _Args);
    }
    
    public class ViewMazeItemSpringboard : ViewMazeItemBase, IViewMazeItemSpringboard
    {
        #region constants
        
        private const float  SpringboardHeight = 0.3f;
        private const float  SpringboardWidth  = 0.4f;
        private const float  JumpCoefficient   = 0.2f;

        #endregion
        
        #region nonpublic members

        private static AudioClipArgs _audioClipArgsSpringboardJump => 
            new AudioClipArgs("springboard_jump", EAudioClipType.GameSound);
        private Vector2 m_Edge1Start, m_Edge2Start;
        
        #endregion

        #region shapes

        protected override string ObjectName => "Springboard Block";
        private Line m_Springboard;
        private Line m_Pillar;
        
        #endregion
        
        #region inject
        
        public ViewMazeItemSpringboard(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            IMazeCoordinateConverter _CoordinateConverter, 
            IContainersGetter _ContainersGetter,
            IViewGameTicker _GameTicker,
            IViewAppearTransitioner _Transitioner,
            IManagersGetter _Managers,
            IColorProvider _ColorProvider,
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
            Coroutines.Run(JumpCoroutine());
        }

        #endregion

        #region nonpublic methods
        
        protected override void InitShape()
        {
            m_Pillar = Object.AddComponentOnNewChild<Line>("Springboard Item", out _);
            m_Springboard = Object.AddComponentOnNewChild<Line>("Springboard", out _);
            m_Pillar.EndCaps = m_Springboard.EndCaps = LineEndCap.Round;
            m_Pillar.Color = m_Springboard.Color = ColorProvider.GetColor(ColorIds.Main);
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
                m_Pillar.Color = _Color;
                m_Springboard.Color = _Color;
            }
        }

        private IEnumerator JumpCoroutine()
        {
            UnityAction<float> doOnProgress = _Progress => (m_Springboard.Start, m_Springboard.End, m_Pillar.End) =
                GetSpringboardEdgesOnJump(_Progress); 

            yield return Coroutines.Lerp(
                0,
                JumpCoefficient,
                0.05f,
                _Progress => doOnProgress(_Progress),
                GameTicker,
                (_, __) =>
                {
                    Coroutines.Run(Coroutines.Lerp(
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
                {Shapes, () => ColorProvider.GetColor(ColorIds.Main)}
            };
        }
        
        #endregion
        
    }
}