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
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Common;
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
        private static AudioClipArgs AudioClipArgsSpringboardJump => 
            new AudioClipArgs("springboard_jump", EAudioClipType.GameSound);
        
        private Vector2 m_Edge1Start, m_Edge2Start;
        
        protected Line Springboard;
        protected Line Pillar;
        
        #endregion
        
        #region inject

        public ViewMazeItemSpringboard(
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

        #endregion

        #region api
        
        public override Component[] Shapes => new Component[] {Springboard, Pillar};
        
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
        
        public virtual void UpdateTick() { }

        #endregion

        #region nonpublic methods
        
        protected override void InitShape()
        {
            Pillar = Object.AddComponentOnNewChild<Line>("Springboard Item", out _);
            Pillar.SortingOrder = SortingOrders.PathLine;
            Springboard = Object.AddComponentOnNewChild<Line>("Springboard", out _);
            Springboard.SortingOrder = SortingOrders.GetBlockSortingOrder(EMazeItemType.Springboard);
            Pillar.EndCaps = Springboard.EndCaps = LineEndCap.Round;
        }

        protected override void UpdateShape()
        {
            Pillar.Thickness = ViewSettings.LineWidth * CoordinateConverter.Scale;
            Springboard.Thickness = Pillar.Thickness * 2f;
            (Pillar.Start, Pillar.End, Springboard.Start, Springboard.End) =
                GetSpringboardAndPillarEdges();
            m_Edge1Start = Springboard.Start;
            m_Edge2Start = Springboard.End;
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.Main)
                return;
            Pillar.Color = _Color;
            Springboard.Color = _Color;
        }

        private IEnumerator JumpCoroutine()
        {
            UnityAction<float> doOnProgress = _Progress => (Springboard.Start, Springboard.End, Pillar.End) =
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
            var col = ColorProvider.GetColor(ColorIds.Main);
            return new Dictionary<IEnumerable<Component>, Func<Color>> {{Shapes, () => col}};
        }
        
        #endregion
    }
}