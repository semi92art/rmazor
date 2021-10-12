using System;
using System.Collections;
using System.Linq;
using DI.Extensions;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.Utils;
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
        private const string SpringboardJumpSoundClipName = "springboard_jump";
        
        #endregion
        
        #region nonpublic members

        private Vector2 m_Edge1Start, m_Edge2Start;
        
        #endregion

        #region shapes

        protected override object[] DefaultColorShapes => new object[] {m_Springboard, m_Pillar};
        private Line m_Springboard;
        private Line m_Pillar;
        
        #endregion
        
        #region inject
        
        public ViewMazeItemSpringboard(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            ICoordinateConverter _CoordinateConverter, 
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker,
            IViewAppearTransitioner _Transitioner,
            IManagersGetter _Managers) 
            : base(
                _ViewSettings,
                _Model, 
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers) { }
        
        #endregion

        #region api
        
        public override object Clone() => new ViewMazeItemSpringboard(
            ViewSettings, 
            Model, 
            CoordinateConverter, 
            ContainersGetter,
            GameTicker,
            Transitioner,
            Managers);

        public void MakeJump(SpringboardEventArgs _Args)
        {
            Managers.Notify(_SM => _SM.PlayClip(SpringboardJumpSoundClipName));
            Coroutines.Run(JumpCoroutine());
        }

        #endregion

        #region nonpublic methods
        
        protected override void InitShape()
        {
            var go = Object;
            m_Pillar = ContainersGetter.MazeItemsContainer.gameObject
                .GetOrAddComponentOnNewChild<Line>("Springboard Item", ref go);
            go.DestroyChildrenSafe();
            m_Springboard = go.AddComponentOnNewChild<Line>("Springboard", out _, Vector2.zero);
            m_Pillar.EndCaps = m_Springboard.EndCaps = LineEndCap.Round;
            m_Pillar.Color = m_Springboard.Color = DrawingUtils.ColorLines;
        }

        protected override void UpdateShape()
        {
            m_Pillar.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            m_Pillar.Thickness = ViewSettings.LineWidth * CoordinateConverter.GetScale();
            m_Springboard.Thickness = m_Pillar.Thickness * 2f;
            (m_Pillar.Start, m_Pillar.End, m_Springboard.Start, m_Springboard.End) = GetSpringboardAndPillarEdges();
            m_Edge1Start = m_Springboard.Start;
            m_Edge2Start = m_Springboard.End;
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
            var V = Props.Directions.First().ToVector2();
            var Vorth = new Vector2(-V.x, V.y);
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