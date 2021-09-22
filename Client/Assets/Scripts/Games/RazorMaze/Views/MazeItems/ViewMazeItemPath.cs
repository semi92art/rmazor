using System;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using Exceptions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeItems.Props;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using TimeProviders;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemPath : IViewMazeItem { }
    
    public class ViewMazeItemPath : ViewMazeItemBase, IViewMazeItemPath, IUpdateTick
    {
        #region nonpublic members

        private Rectangle m_Shape;
        private Line m_LeftBorder, m_RightBorder, m_BottomBorder, m_TopBorder;
        private Disc m_BottomLeftCorner, m_BottomRightCorner, m_TopLeftCorner, m_TopRightCorner;
        private bool 
            m_LeftBorderInitialized,
            m_RightBorderInitialized, 
            m_BottomBorderInitialized,
            m_TopBorderInitialized;
        private bool 
            m_BottomLeftCornerInitialized,
            m_BottomRightCornerInitialized,
            m_TopLeftCornerInitialized,
            m_TopRightCornerInitialized;
        
        #endregion
        
        #region inject
        
        public ViewMazeItemPath(
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTimeProvider _GameTimeProvider,
            IModelMazeData _Data,
            ITicker _Ticker,
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
                if (!value)
                    EnableInitializedShapes(false);
            }
        }
        
        public override bool Proceeding
        {
            get => base.Proceeding;
            set
            {
                base.Proceeding = value;
                if (value) Fill();
                else Unfill();
            }
        }
        
        public void UpdateTick()
        {
            if (!Initialized || !Activated)
                return;
            
            float dOffset = Time.deltaTime * 3f;
            
            if (m_LeftBorderInitialized && m_LeftBorder.Dashed)
                m_LeftBorder.DashOffset -= dOffset;
            if (m_RightBorderInitialized && m_RightBorder.Dashed)
                m_RightBorder.DashOffset += dOffset;
            if (m_BottomBorderInitialized && m_BottomBorder.Dashed)
                m_BottomBorder.DashOffset += dOffset;
            if (m_TopBorderInitialized && m_TopBorder.Dashed)
                m_TopBorder.DashOffset -= dOffset;
        }
        
        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded)
                Appear(true);
            else if (_Args.Stage == ELevelStage.Unloaded)
                Appear(false);
        }

        public override object Clone() => 
            new ViewMazeItemPath(CoordinateConverter, ContainersGetter, GameTimeProvider, Data, Ticker, ViewSettings);

        #endregion
        
        #region nonpublic methods
        
        private void Fill() => m_Shape.Color = DrawingUtils.ColorBack;
        private void Unfill() => m_Shape.Color = DrawingUtils.ColorLines;

        public override void Init(ViewMazeItemProps _Props)
        {
            Props = _Props;
            SetShape();
            Ticker.Register(this);

            Coroutines.Run(Coroutines.WaitWhile(
                () =>
                {
                    bool res = true;
                    if (m_LeftBorderInitialized)
                        res &= !m_LeftBorder.IsNull();
                    if (m_RightBorderInitialized)
                        res &= !m_RightBorder.IsNull();
                    if (m_BottomBorderInitialized)
                        res &= !m_BottomBorder.IsNull();
                    if (m_TopBorderInitialized)
                        res &= !m_TopBorder.IsNull();
        
                    if (m_BottomLeftCornerInitialized)
                        res &= !m_BottomLeftCorner.IsNull();
                    if (m_BottomRightCornerInitialized)
                        res &= !m_BottomRightCorner.IsNull();
                    if (m_TopLeftCornerInitialized)
                        res &= !m_TopLeftCorner.IsNull();
                    if (m_TopRightCornerInitialized)
                        res &= !m_TopRightCorner.IsNull();

                    return !res;
                },
                () => Initialized = true));
        }

        protected override void SetShape()
        {
            var go = Object;
            var sh = ContainersGetter.MazeItemsContainer.gameObject
                .GetOrAddComponentOnNewChild<Rectangle>("Path Item", ref go, 
                    CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            go.DestroyChildrenSafe();
            sh.Width = sh.Height = CoordinateConverter.GetScale() * 0.4f;
            sh.Type = Rectangle.RectangleType.RoundedHollow;
            sh.CornerRadiusMode = Rectangle.RectangleCornerRadiusMode.Uniform;
            float cr = ViewSettings.CornerRadius * CoordinateConverter.GetScale() * 2f;
            sh.CornerRadius = cr;
            sh.Color = DrawingUtils.ColorLines;
            sh.SortingOrder = DrawingUtils.GetPathSortingOrder();
            Object = go;
            m_Shape = sh;
            SetBordersAndCorners();
            EnableInitializedShapes(false);
        }

        private void SetBordersAndCorners()
        {
            InitBorders();
            InitInnerCorners();
            InitOuterCorners();
            AdjustBordersToOtherOuterCorners();
        }

        private void InitBorders()
        {
            var pos = Props.Position;
            if (!PathExist(pos + V2Int.left))
                InitBorder(EMazeMoveDirection.Left);
            if (!PathExist(pos + V2Int.right))
                InitBorder(EMazeMoveDirection.Right);
            if (!PathExist(pos + V2Int.up))
                InitBorder(EMazeMoveDirection.Up);
            if (!PathExist(pos + V2Int.down))
                InitBorder(EMazeMoveDirection.Down);
        }
        
        private void InitInnerCorners()
        {
            var pos = Props.Position;
            if (!PathExist(pos + V2Int.left) && !PathExist(pos + V2Int.down))
                InitCorner(false, false, true);
            if (!PathExist(pos + V2Int.left) && !PathExist(pos + V2Int.up))
                InitCorner(false, true, true);
            if (!PathExist(pos + V2Int.right) && !PathExist(pos + V2Int.down))
                InitCorner(true, false, true);
            if (!PathExist(pos + V2Int.right) && !PathExist(pos + V2Int.up))
                InitCorner(true, true, true);
        }

        private void InitOuterCorners()
        {
            var pos = Props.Position;
            if (PathExist(pos + V2Int.down) && PathExist(pos + V2Int.left) && !PathExist(pos + V2Int.down + V2Int.left))
                InitCorner(false, false, false);
            if (PathExist(pos + V2Int.down) && PathExist(pos + V2Int.right) && !PathExist(pos + V2Int.down + V2Int.right))
                InitCorner(true, false, false);
            if (PathExist(pos + V2Int.up) && PathExist(pos + V2Int.left) && !PathExist(pos + V2Int.up + V2Int.left))
                InitCorner(false, true, false);
            if (PathExist(pos + V2Int.up) && PathExist(pos + V2Int.right) && !PathExist(pos + V2Int.up + V2Int.right))
                InitCorner(true, true, false);
        }

        private void AdjustBordersToOtherOuterCorners()
        {
            var pos = Props.Position;
            if (!PathExist(pos + V2Int.left))
            {
                if (PathExist(pos + V2Int.left + V2Int.down))
                    m_LeftBorder.Start = GetBorderPointsAndDashed(EMazeMoveDirection.Left, true, true).Item1;
                if (PathExist(pos + V2Int.left + V2Int.up))
                    m_LeftBorder.End = GetBorderPointsAndDashed(EMazeMoveDirection.Left, true, true).Item2;
            }
            if (!PathExist(pos + V2Int.right))
            {
                if (PathExist(pos + V2Int.right + V2Int.down))
                    m_RightBorder.Start = GetBorderPointsAndDashed(EMazeMoveDirection.Right, true, true).Item1;
                if (PathExist(pos + V2Int.right + V2Int.up))
                    m_RightBorder.End = GetBorderPointsAndDashed(EMazeMoveDirection.Right, true, true).Item2;
            }
            if (!PathExist(pos + V2Int.down))
            {
                if (PathExist(pos + V2Int.down + V2Int.left))
                    m_BottomBorder.Start = GetBorderPointsAndDashed(EMazeMoveDirection.Down, true, true).Item1;
                if (PathExist(pos + V2Int.down + V2Int.right))
                    m_BottomBorder.End = GetBorderPointsAndDashed(EMazeMoveDirection.Down, true, true).Item2;
            }
            if (!PathExist(pos + V2Int.up))
            {
                if (PathExist(pos + V2Int.up + V2Int.left))
                    m_TopBorder.Start = GetBorderPointsAndDashed(EMazeMoveDirection.Up, true, true).Item1;
                if (PathExist(pos + V2Int.up + V2Int.right))
                    m_TopBorder.End = GetBorderPointsAndDashed(EMazeMoveDirection.Up, true, true).Item2;
            }
        }

        private void EnableInitializedShapes(bool _Enable)
        {
            if (!m_Shape.IsNull()) m_Shape.enabled                         = _Enable;
            
            if (!m_LeftBorder.IsNull()) m_LeftBorder.enabled               = _Enable;
            if (!m_RightBorder.IsNull()) m_RightBorder.enabled             = _Enable;
            if (!m_BottomBorder.IsNull()) m_BottomBorder.enabled           = _Enable;
            if (!m_TopBorder.IsNull()) m_TopBorder.enabled                 = _Enable;
            
            if (!m_BottomLeftCorner.IsNull()) m_BottomLeftCorner.enabled   = _Enable;
            if (!m_BottomRightCorner.IsNull()) m_BottomRightCorner.enabled = _Enable;
            if (!m_TopLeftCorner.IsNull()) m_TopLeftCorner.enabled         = _Enable;
            if (!m_TopRightCorner.IsNull()) m_TopRightCorner.enabled       = _Enable;
        }
        
        private void InitBorder(EMazeMoveDirection _Side)
        {
            var go = new GameObject("Border");
            go.SetParent(ContainersGetter.MazeItemsContainer);
            go.transform.SetLocalPosXY(Vector2.zero);
            var border = go.AddComponent<Line>();
            border.Thickness = ViewSettings.LineWidth * CoordinateConverter.GetScale();
            border.EndCaps = LineEndCap.None;
            border.Color = DrawingUtils.ColorLines.SetA(0.5f);
            (border.Start, border.End, border.Dashed) = GetBorderPointsAndDashed(_Side, false, false);
            border.DashSize = 1f;
            switch (_Side)
            {
                case EMazeMoveDirection.Up: m_TopBorder = border; m_TopBorderInitialized = true; break;
                case EMazeMoveDirection.Right: m_RightBorder = border; m_RightBorderInitialized = true; break;
                case EMazeMoveDirection.Down: m_BottomBorder = border; m_BottomBorderInitialized = true; break;
                case EMazeMoveDirection.Left: m_LeftBorder = border; m_LeftBorderInitialized = true; break;
                default: throw new SwitchCaseNotImplementedException(_Side);
            }
        }

        private void InitCorner(
            bool _Right,
            bool _Up,
            bool _Inner)
        {
            Disc corner = null;
            if (!_Right && !_Up && !m_BottomLeftCorner.IsNull())
                corner = m_BottomLeftCorner;
            else if (_Right && !_Up && !m_BottomRightCorner.IsNull())
                corner = m_BottomRightCorner;
            else if (!_Right && _Up && !m_TopLeftCorner.IsNull())
                corner = m_TopLeftCorner;
            else if (_Right && _Up && !m_TopRightCorner.IsNull())
                corner = m_TopRightCorner;
            if (corner.IsNull())
            {
                var go = new GameObject("Corner");
                go.SetParent(ContainersGetter.MazeItemsContainer);
                go.transform.SetLocalPosXY(Vector2.zero);
                corner = go.AddComponent<Disc>();
            }
            corner.transform.SetLocalPosXY(GetCornerCenter(_Right, _Up, _Inner));
            corner.Type = DiscType.Arc;
            corner.Radius = ViewSettings.CornerRadius * CoordinateConverter.GetScale();
            corner.Thickness = ViewSettings.LineWidth * CoordinateConverter.GetScale() * 1.5f;
            var angles = GetCornerAngles(_Right, _Up, _Inner);
            corner.AngRadiansStart = Mathf.Deg2Rad * angles.x;
            corner.AngRadiansEnd = Mathf.Deg2Rad * angles.y;
            corner.Color = DrawingUtils.ColorLines;
            
            if (!_Right && !_Up)
            {
                m_BottomLeftCorner = corner;
                m_BottomLeftCornerInitialized = true;
                if (!_Inner) return;
                m_BottomBorder.Start = GetBorderPointsAndDashed(EMazeMoveDirection.Down, true, true).Item1;
                m_LeftBorder.Start = GetBorderPointsAndDashed(EMazeMoveDirection.Left, true, true).Item1;
            }
            else if (_Right && !_Up)
            {
                m_BottomRightCorner = corner;
                m_BottomRightCornerInitialized = true;
                if (!_Inner) return;
                m_BottomBorder.End = GetBorderPointsAndDashed(EMazeMoveDirection.Down, true, true).Item2;
                m_RightBorder.Start = GetBorderPointsAndDashed(EMazeMoveDirection.Right, true, true).Item1;
            }
            else if (!_Right && _Up)
            {
                m_TopLeftCorner = corner;
                m_TopLeftCornerInitialized = true;
                if (!_Inner) return;
                m_LeftBorder.End = GetBorderPointsAndDashed(EMazeMoveDirection.Left, true, true).Item2;
                m_TopBorder.Start = GetBorderPointsAndDashed(EMazeMoveDirection.Up, true, true).Item1;
            }
            else if (_Right && _Up)
            {
                m_TopRightCorner = corner;
                m_TopRightCornerInitialized = true;
                if (!_Inner) return;
                m_TopBorder.End = GetBorderPointsAndDashed(EMazeMoveDirection.Up, true, true).Item2;
                m_RightBorder.End = GetBorderPointsAndDashed(EMazeMoveDirection.Right, true, true).Item2;
            }
        }

        private Tuple<Vector2, Vector2, bool> GetBorderPointsAndDashed(EMazeMoveDirection _Side, bool _StartLimit, bool _EndLimit)
        {
            Vector2 start, end;
            var pos = Props.Position.ToVector2();
            var cr = ViewSettings.CornerRadius;
            switch (_Side)
            {
                case EMazeMoveDirection.Up:
                    start = pos + (Vector2.up + Vector2.left) * 0.5f + (_StartLimit ? Vector2.right * cr : Vector2.zero);
                    end = pos + (Vector2.up + Vector2.right) * 0.5f + (_EndLimit ? Vector2.left * cr : Vector2.zero);
                    break;
                case EMazeMoveDirection.Right:
                    start = pos + (Vector2.right + Vector2.down) * 0.5f + (_StartLimit ? Vector2.up * cr : Vector2.zero);
                    end = pos + (Vector2.right + Vector2.up) * 0.5f + (_EndLimit ? Vector2.down * cr : Vector2.zero);
                    break;
                case EMazeMoveDirection.Down:
                    start = pos + (Vector2.down + Vector2.left) * 0.5f + (_StartLimit ? Vector2.right * cr : Vector2.zero);
                    end = pos + (Vector2.down + Vector2.right) * 0.5f + (_EndLimit ? Vector2.left * cr : Vector2.zero);
                    break;
                case EMazeMoveDirection.Left:
                    start = pos + (Vector2.left + Vector2.down) * 0.5f + (_StartLimit ? Vector2.up * cr : Vector2.zero);
                    end = pos + (Vector2.left + Vector2.up) * 0.5f + (_EndLimit ? Vector2.down * cr : Vector2.zero);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Side);
            }
            start = CoordinateConverter.ToLocalMazeItemPosition(start);
            end = CoordinateConverter.ToLocalMazeItemPosition(end);

            var dir = RazorMazeUtils.GetDirectionVector(_Side, MazeOrientation.North);
            bool dashed = Data.Info.MazeItems.Any(_Item =>
                _Item.Position == Props.Position + dir &&
                (_Item.Type == EMazeItemType.TrapReact && _Item.Direction == -dir
                || _Item.Type == EMazeItemType.TrapIncreasing));
            
            return new Tuple<Vector2, Vector2, bool>(start, end, dashed);
        }

        private Vector2 GetCornerCenter(
            bool _Right,
            bool _Up,
            bool _Inner)
        {
            var pos = Props.Position.ToVector2();
            var cr = ViewSettings.CornerRadius;
            float xIndent = (_Right ? 1f : -1f) * (_Inner ? -1f : 1f);
            float yIndent = (_Up ? 1f : -1f) * (_Inner ? -1f : 1f);
            var crVec = cr * new Vector2(xIndent, yIndent);
            var center = pos + ((_Right ? Vector2.right : Vector2.left) + (_Up ? Vector2.up : Vector2.down)) * 0.5f + crVec;
            return CoordinateConverter.ToLocalMazeItemPosition(center);
        }

        private Vector2 GetCornerAngles(bool _Right, bool _Up, bool _Inner)
        {
            if (_Right)
            {
                if (_Up)
                    return _Inner ? new Vector2(0, 90) : new Vector2(180, 270);
                return _Inner ? new Vector2(270, 360) : new Vector2(90, 180);
            }
            if (_Up)
                return _Inner ? new Vector2(90, 180) : new Vector2(270, 360);
            return _Inner ? new Vector2(180, 270) : new Vector2(0, 90);
        }

        private bool PathExist(V2Int _Path) => Data.Info.Path.Contains(_Path);

        protected override void Appear(bool _Appear)
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => !Initialized,
                () =>
                {
                    ShapeRenderer shape = null;
                    if (_Appear && !Props.IsStartNode || !_Appear && !Proceeding)
                        shape = m_Shape;
                    
                    RazorMazeUtils.DoAppearTransitionSimple(
                        _Appear,
                        GameTimeProvider,
                        new Dictionary<IEnumerable<ShapeRenderer>, Color>
                        {
                            {
                                new ShapeRenderer[]
                                {
                                    m_BottomLeftCorner,
                                    m_BottomRightCorner,
                                    m_TopLeftCorner,
                                    m_TopRightCorner
                                },
                                DrawingUtils.ColorLines
                            },
                            {
                                new[] {m_LeftBorder, m_RightBorder, m_BottomBorder, m_TopBorder},
                                DrawingUtils.ColorLines.SetA(0.5f)
                            },
                            {new [] {shape}, DrawingUtils.ColorLines}
                        });
                }));
        }

        #endregion
    }
}