using System;
using System.Collections;
using System.Linq;
using Entities;
using Extensions;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using TimeProviders;
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

        #region nonpublic members

        private const float SpringboardHeight = 0.3f;
        private const float SpringboardWidth = 0.4f;
        private const float JumpCoefficient = 0.2f;
        private Line m_Springboard;
        private Line m_Pillar;
        private Vector2 m_Edge1Start, m_Edge2Start;
        
        #endregion
        
        #region inject
        
        private IGameTimeProvider GameTimeProvider { get; }
        private ViewSettings ViewSettings { get; }

        public ViewMazeItemSpringboard(
            ICoordinateConverter _CoordinateConverter, 
            IContainersGetter _ContainersGetter,
            IGameTimeProvider _GameTimeProvider,
            ViewSettings _ViewSettings) 
            : base(_CoordinateConverter, _ContainersGetter)
        {
            GameTimeProvider = _GameTimeProvider;
            ViewSettings = _ViewSettings;
        }
        
        #endregion

        #region api

        public void MakeJump(SpringboardEventArgs _Args)
        {
            Coroutines.Run(JumpCoroutine());
        }
        
        public object Clone() => new ViewMazeItemSpringboard(
            CoordinateConverter, ContainersGetter, GameTimeProvider, ViewSettings);

        #endregion

        #region nonpublic methods
        
        protected override void SetShape()
        {
            var go = Object;
            var pillar = ContainersGetter.MazeItemsContainer.gameObject
                .GetOrAddComponentOnNewChild<Line>("Springboard Item", ref go,
                    CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            go.DestroyChildrenSafe();
            var sprbrd = go.AddComponentOnNewChild<Line>("Springboard", out _, Vector2.zero);
            
            pillar.Thickness = ViewSettings.LineWidth * CoordinateConverter.GetScale();
            sprbrd.Thickness = pillar.Thickness * 2f;
            
            pillar.EndCaps = sprbrd.EndCaps = LineEndCap.Round;
            pillar.Color = sprbrd.Color = ViewUtils.ColorLines;
            (pillar.Start, pillar.End, sprbrd.Start, sprbrd.End) = GetSpringboardAndPillarEdges();
            m_Edge1Start = sprbrd.Start;
            m_Edge2Start = sprbrd.End;

            m_Pillar = pillar;
            m_Springboard = sprbrd;
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
                GameTimeProvider,
                (_, __) =>
                {
                    Coroutines.Run(Coroutines.Lerp(
                        JumpCoefficient,
                        0,
                        0.05f,
                        _Progress => doOnProgress(_Progress),
                        GameTimeProvider));
                });
        }
        
        private Tuple<Vector2, Vector2, Vector2, Vector2> GetSpringboardAndPillarEdges()
        {
            var V = Props.Directions.First().ToVector2();
            var Vorth = new Vector2(-V.x, V.y);
            var Vx = Vector2.right * V.x;
            var Vy = Vector2.up * V.y;
            var A = -V * 0.5f;
            var A1 = -V * 0.4f;
            var D = A1 + V * SpringboardHeight;
            var B = D - Vorth * SpringboardWidth * 0.5f;
            var C = D + Vorth * SpringboardWidth * 0.5f;
            A1 *= CoordinateConverter.GetScale();
            B *= CoordinateConverter.GetScale();
            C *= CoordinateConverter.GetScale();
            D *= CoordinateConverter.GetScale();
            return new Tuple<Vector2, Vector2, Vector2, Vector2>(A1, D, B, C);
        }

        private Tuple<Vector2, Vector2, Vector2> GetSpringboardEdgesOnJump(float _C)
        {
            var V = Props.Directions.First().ToVector2();
            var edge1 = m_Edge1Start + V * _C;
            var edge2 = m_Edge2Start + V * _C;
            var pillarEdge = (edge1 + edge2) * 0.5f;
            return new Tuple<Vector2, Vector2, Vector2>(edge1, edge2, pillarEdge);
        }
        
        #endregion
        
    }
}