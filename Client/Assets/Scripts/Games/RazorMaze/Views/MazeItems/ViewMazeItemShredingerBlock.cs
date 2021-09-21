using System.Collections;
using System.Collections.Generic;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using TimeProviders;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemShredingerBlock : IViewMazeItem { }
    
    public class ViewMazeItemShredingerBlock : ViewMazeItemBase, IViewMazeItemShredingerBlock, IUpdateTick
    {
        #region nonpublic members

        private float m_LineOffset;
        private int m_DeactivationsCount;
        
        private Rectangle m_Block;
        private readonly List<Line> m_Lines = new List<Line>();
        
        #endregion
        
        #region inject
        

        public ViewMazeItemShredingerBlock(
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IModelMazeData _Data,
            ITicker _Ticker,
            IGameTimeProvider _GameTimeProvider,
            ViewSettings _ViewSettings)
            : base(_ViewSettings, _Data, _CoordinateConverter, _ContainersGetter, _GameTimeProvider, _Ticker) { }
        
        #endregion
        
        #region api

        public override bool Activated
        {
            get => m_Activated;
            set
            {
                m_Activated = value;
                m_Block.enabled = value;
                m_Lines.ForEach(_Line => _Line.enabled = value);
            }
        }

        public override bool Proceeding
        {
            get => base.Proceeding;
            set
            {
                base.Proceeding = value;
                if (value)
                    ActivateBlock();
                else DeactivateBlock();
            }
        }

        public override void Init(ViewMazeItemProps _Props)
        {
            base.Init(_Props);
            Proceeding = false;
        }
        
        public void UpdateTick()
        {
            if (!Initialized || !Activated)
                return;
            if (Proceeding)
                return;
            m_LineOffset += Time.deltaTime * ViewSettings.ShredingerLineOffsetSpeed;
            foreach (var line in m_Lines)
            {
                line.DashOffset = m_LineOffset;
            }
        }

        public override object Clone() => new ViewMazeItemShredingerBlock(
            CoordinateConverter, ContainersGetter, Data, Ticker, GameTimeProvider, ViewSettings);

        #endregion
        
        #region nonpublic methods

        protected override void SetShape()
        {
            var go = Object;
            var sh = ContainersGetter.MazeItemsContainer.gameObject
                .GetOrAddComponentOnNewChild<Rectangle>(
                    "Shredinger Block",
                    ref go,
                    CoordinateConverter.ToLocalMazeItemPosition(Props.Position));

            go.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            go.DestroyChildrenSafe();
            Object = go;
            
            sh.Width = sh.Height = CoordinateConverter.GetScale() * 0.9f;
            sh.Type = Rectangle.RectangleType.RoundedHollow;
            sh.CornerRadius = ViewSettings.CornerRadius * CoordinateConverter.GetScale();
            sh.Color = DrawingUtils.ColorLines;
            sh.SortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type);
            sh.Thickness = ViewSettings.LineWidth * CoordinateConverter.GetScale();
            m_Block = sh;
            
            
            var lLeft = go.AddComponentOnNewChild<Line>("Left Line", out _, Vector2.zero);
            var lRight = go.AddComponentOnNewChild<Line>("Right Line", out _, Vector2.zero);
            var lBottom1 = go.AddComponentOnNewChild<Line>("Bottom Line 1", out _, Vector2.zero);
            var lBottom2 = go.AddComponentOnNewChild<Line>("Bottom Line 2", out _, Vector2.zero);
            var lTop1 = go.AddComponentOnNewChild<Line>("Top Line 1", out _, Vector2.zero);
            var lTop2 = go.AddComponentOnNewChild<Line>("Top Line 2", out _, Vector2.zero);
            var lCenter1 = go.AddComponentOnNewChild<Line>("Center Line 1", out _, Vector2.zero);
            var lCenter2 = go.AddComponentOnNewChild<Line>("Center Line 2", out _, Vector2.zero);

            var cornerPositions = GetCornerPositions();
            var centerLinePositions = GetCenterLinePositions();
            lBottom1.End = lLeft.Start = cornerPositions[0];
            lLeft.End = lTop1.Start = cornerPositions[1];
            lTop1.End = lCenter1.Start = centerLinePositions[1];
            lCenter1.End = lBottom1.Start = centerLinePositions[0];
            lBottom2.Start = lCenter2.End = centerLinePositions[3];
            lCenter2.Start = lTop2.End = centerLinePositions[2];
            lTop2.Start = lRight.End = cornerPositions[2];
            lRight.Start = lBottom2.End = cornerPositions[3];
            m_Lines.AddRange(new []{lLeft, lRight, lBottom1, lBottom2, lTop1, lTop2, lCenter1, lCenter2});
            foreach (var line in m_Lines)
            {
                line.Dashed = true;
                line.DashType = DashType.Rounded;
                line.Color = DrawingUtils.ColorLines;
                line.SortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type);
                line.Thickness = ViewSettings.LineWidth * CoordinateConverter.GetScale();
            }
        }

        protected override void Appear(bool _Appear)
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => !Initialized,
                () =>
                {
                    RazorMazeUtils.DoAppearTransitionSimple(
                        _Appear,
                        GameTimeProvider,
                        new Dictionary<IEnumerable<ShapeRenderer>, Color>
                        {
                            {m_Lines, DrawingUtils.ColorLines},
                            {new [] {m_Block}, DrawingUtils.ColorLines }
                        });

                }));
        }

        private List<Vector2> GetCornerPositions()
        {
            float coeff = 0.43f;
            float scale = CoordinateConverter.GetScale();
            var bottomLeft = (Vector2.down + Vector2.left) * coeff * scale;
            var bottomRight = (Vector2.down + Vector2.right) * coeff * scale;
            var topLeft = (Vector2.up + Vector2.left) * coeff * scale;
            var topRight = (Vector2.up + Vector2.right) * coeff * scale;
            return new List<Vector2> {bottomLeft, topLeft, topRight, bottomRight};
        }
        
        private List<Vector2> GetCenterLinePositions()
        {
            float coeff = 0.43f;
            float coeff2 = 0.1f;
            float scale = CoordinateConverter.GetScale();
            var bottomLeft = (Vector2.down + Vector2.left * coeff2) * coeff * scale;
            var bottomRight = (Vector2.down + Vector2.right * coeff2) * coeff * scale;
            var topLeft = (Vector2.up + Vector2.left * coeff2) * coeff * scale;
            var topRight = (Vector2.up + Vector2.right * coeff2) * coeff * scale;
            return new List<Vector2> {bottomLeft, topLeft, topRight, bottomRight};
        }

        private void ActivateBlock() => Coroutines.Run(ActivateBlock(true));

        private void DeactivateBlock() => Coroutines.Run(ActivateBlock(false));

        private IEnumerator ActivateBlock(bool _Activate)
        {
            if (!_Activate && m_DeactivationsCount == 0)
                m_Block.enabled = false;
            float cornerRadius = ViewSettings.CornerRadius * CoordinateConverter.GetScale();
            if (_Activate)
                m_Block.enabled = true;
            else
            {
                foreach (var line in m_Lines)
                    line.enabled = true;
            }
            yield return Coroutines.Lerp(
                0f,
                1f,
                0.2f,
                _Progress =>
                {
                    foreach (var line in m_Lines)
                        line.Color = DrawingUtils.ColorLines.SetA(_Activate ? 1f - _Progress : _Progress);
                    m_Block.Color = DrawingUtils.ColorLines.SetA(_Activate ? _Progress : 1f - _Progress);
                    m_Block.CornerRadius = cornerRadius * (_Activate ? _Progress : 1f - _Progress);
                },
                GameTimeProvider,
                (_Breaked, _Progress) =>
                {
                    if (_Activate)
                    {
                        foreach (var line in m_Lines)
                            line.enabled = false;
                    }
                    else
                        m_Block.enabled = false;
                });
            if (!_Activate)
                m_DeactivationsCount++;
        }

        #endregion
    }
}