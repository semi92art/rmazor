using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using Exceptions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.Utils;
using ModestTree;
using Shapes;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemPath : IViewMazeItem, ICharacterMoveFinished
    {
        bool Collected { get; set; }
    }
    
    public class ViewMazeItemPath : ViewMazeItemBase, IViewMazeItemPath, IUpdateTick
    {
        #region constants

        private const string SoundClipNameCollectPoint = "collect_point"; 

        #endregion
        
        #region shapes

        protected override string ObjectName => "Path Block";
        private Rectangle m_Shape;
        private Line m_LeftBorder, m_RightBorder, m_BottomBorder, m_TopBorder;
        private Disc m_BottomLeftCorner, m_BottomRightCorner, m_TopLeftCorner, m_TopRightCorner;
        private Color m_BottomLeftCornerCol, m_BottomRightCornerCol, m_TopLeftCornerCol, m_TopRightCornerCol;
        
        #endregion
        
        #region nonpublic members

        private bool m_Collect;
        private bool m_LeftBorderInited, m_RightBorderInited, m_BottomBorderInited, m_TopBorderInited;
        private bool m_BottomLeftCornerInited, m_BottomRightCornerInited, m_TopLeftCornerInited, m_TopRightCornerInited;
        
        #endregion
        
        #region inject
        
        public ViewMazeItemPath(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            IMazeCoordinateConverter _CoordinateConverter,
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

        public override object[] Shapes => new object[]
        {
            m_Shape,
            m_LeftBorder, m_RightBorder, m_BottomBorder, m_TopBorder,
            m_BottomLeftCorner, m_BottomRightCorner, m_TopLeftCorner, m_TopRightCorner
        };
        
        public override object Clone() => new ViewMazeItemPath(
            ViewSettings,
            Model,
            CoordinateConverter,
            ContainersGetter,
            GameTicker,
            Transitioner,
            Managers);

        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                m_ActivatedInSpawnPool = value;
                EnableInitializedShapes(value);
            }
        }

        public bool Collected
        {
            get => m_Collect;
            set
            {
                m_Collect = value;
                Collect(value);
            }
        }

        public void UpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            
            float dOffset = Time.deltaTime * 3f;
            
            if (m_LeftBorderInited && m_LeftBorder.Dashed)
                m_LeftBorder.DashOffset -= dOffset;
            if (m_RightBorderInited && m_RightBorder.Dashed)
                m_RightBorder.DashOffset += dOffset;
            if (m_BottomBorderInited && m_BottomBorder.Dashed)
                m_BottomBorder.DashOffset += dOffset;
            if (m_TopBorderInited && m_TopBorder.Dashed)
                m_TopBorder.DashOffset -= dOffset;
        }
        
        public void OnCharacterMoveFinished(CharacterMovingEventArgs _Args)
        {
            if (_Args.Position != Props.Position)
                return;
            var springboardInfos = 
                Model.SpringboardProceeder.ProceedInfos[EMazeItemType.Springboard];
            if (springboardInfos.Any(_Info => _Info.CurrentPosition == Props.Position))
                return;
            Coroutines.Run(OnFinishMoveCoroutine(_Args.Direction));
        }

        #endregion
        
        #region nonpublic methods

        private IEnumerator OnFinishMoveCoroutine(EMazeMoveDirection _Direction)
        {
            const float delta = 0.5f;
            const float duration = 0.1f;
            var dir = RazorMazeUtils.GetDirectionVector(_Direction, Model.Data.Orientation).ToVector2();
            Line border = null;
            if (dir == Vector2.up)
                border = m_TopBorder;
            else if (dir == Vector2.down)
                border = m_BottomBorder;
            else if (dir == Vector2.right)
                border = m_RightBorder;
            else if (dir == Vector2.left)
                border = m_LeftBorder;
            
            if (border.IsNull())
                yield break;
            
            var startPos = border.transform.localPosition;
            yield return Coroutines.Lerp(
                0f,
                delta,
                duration,
                _Progress => border.transform.localPosition = startPos + (Vector3) dir * _Progress,
                GameTicker,
                (_, __) => border.transform.localPosition = startPos,
                _ProgressFormula: _P => _P < 0.5f * delta ? _P : delta - _P);
        }
        
        private void Collect(bool _Collect)
        {
            if (_Collect)
            {
                Managers.Notify(_SM => _SM.PlayClip(
                    SoundClipNameCollectPoint, _Tags: Props.Position.ToString()));
            }
            m_Shape.Color = _Collect ? DrawingUtils.ColorLines.SetA(0f) : DrawingUtils.ColorLines;
        }

        protected override void InitShape()
        {
            var sh = Object.AddComponentOnNewChild<Rectangle>("Path Item", out _);
            sh.Type = Rectangle.RectangleType.RoundedHollow;
            sh.CornerRadiusMode = Rectangle.RectangleCornerRadiusMode.Uniform;
            sh.Color = DrawingUtils.ColorLines;
            sh.SortingOrder = DrawingUtils.GetPathSortingOrder();
            sh.enabled = false;
            m_Shape = sh;
        }

        protected override void UpdateShape()
        {
            m_Shape.Width = m_Shape.Height = CoordinateConverter.Scale * 0.4f;
            m_Shape.CornerRadius = ViewSettings.CornerRadius * CoordinateConverter.Scale * 2f;
            SetBordersAndCorners();
            EnableInitializedShapes(false);
        }

        private void SetBordersAndCorners()
        {
            ClearBordersAndCorners();
            InitBorders();
            InitInnerCorners();
            InitOuterCorners();
            AdjustBorders();
        }

        private void ClearBordersAndCorners()
        {
            if (m_LeftBorderInited)        m_LeftBorder       .enabled = false;
            if (m_RightBorderInited)       m_RightBorder      .enabled = false;
            if (m_BottomBorderInited)      m_BottomBorder     .enabled = false;
            if (m_TopBorderInited)         m_TopBorder        .enabled = false;
            
            if (m_BottomLeftCornerInited)  m_BottomLeftCorner .enabled = false;
            if (m_BottomRightCornerInited) m_BottomRightCorner.enabled = false;
            if (m_TopLeftCornerInited)     m_TopLeftCorner    .enabled = false;
            if (m_TopRightCornerInited)    m_TopRightCorner   .enabled = false;
            
            m_LeftBorderInited = m_RightBorderInited = m_BottomBorderInited = m_TopBorderInited = false;
            m_BottomLeftCornerInited = m_BottomRightCornerInited = m_TopLeftCornerInited = m_TopRightCornerInited = false;
        }

        private void InitBorders()
        {
            Func<V2Int, bool> mustInitBorder = _Pos => !TurretExist(_Pos) && !PathExist(_Pos);
            var pos = Props.Position;
            if (mustInitBorder(pos + V2Int.left))
                InitBorder(EMazeMoveDirection.Left);
            if (mustInitBorder(pos + V2Int.right))
                InitBorder(EMazeMoveDirection.Right);
            if (mustInitBorder(pos + V2Int.up))
                InitBorder(EMazeMoveDirection.Up);
            if (mustInitBorder(pos + V2Int.down))
                InitBorder(EMazeMoveDirection.Down);
        }

        private void InitInnerCorners()
        {
            var pos = Props.Position;
            bool initLeftBottomCorner = !PathExist(pos + V2Int.left)
                                      && !PathExist(pos + V2Int.down)
                                      || SpringboardExist(pos) 
                                      && SpringboardDirection(pos) == V2Int.right + V2Int.up;
            if (initLeftBottomCorner)
                InitCorner(false, false, true);
            bool initLeftTopCorner = !PathExist(pos + V2Int.left)
                                     && !PathExist(pos + V2Int.up)
                                     || SpringboardExist(pos)
                                     && SpringboardDirection(pos) == V2Int.right + V2Int.down;
            if (initLeftTopCorner)
                InitCorner(false, true, true);
            bool initRightBottomCorner = !PathExist(pos + V2Int.right)
                                         && !PathExist(pos + V2Int.down)
                                         || SpringboardExist(pos)
                                         && SpringboardDirection(pos) == V2Int.left + V2Int.up;
            if (initRightBottomCorner)
                InitCorner(true, false, true);
            bool initRightTopCorner = !PathExist(pos + V2Int.right)
                                      && !PathExist(pos + V2Int.up)
                                      || SpringboardExist(pos)
                                      && SpringboardDirection(pos) == V2Int.left + V2Int.down;
            if (initRightTopCorner)
                InitCorner(true, true, true);
        }

        private void InitOuterCorners()
        {
            var pos = Props.Position;
            if (MustInitOuterCorner(pos, V2Int.down, V2Int.left))
                InitCorner(false, false, false);
            if (MustInitOuterCorner(pos, V2Int.down, V2Int.right))
                InitCorner(true, false, false);
            if (MustInitOuterCorner(pos, V2Int.up, V2Int.left))
                InitCorner(false, true, false);
            if (MustInitOuterCorner(pos, V2Int.up, V2Int.right))
                InitCorner(true, true, false);
        }

        private bool MustInitOuterCorner(V2Int _Position, V2Int _Dir1, V2Int _Dir2)
        {
            var pos = _Position;
            bool result = PathExist(pos + _Dir1)
                    && PathExist(pos + _Dir2) 
                    && !PathExist(pos + _Dir1 + _Dir2)
                    && !TurretExist(pos + _Dir1 + _Dir2);
            if (result)
                return true;
            result = TurretExist(pos + _Dir1) 
                    && PathExist(pos + _Dir2) 
                    && !PathExist(pos + _Dir1 + _Dir2)
                    && !TurretExist(pos + _Dir1 + _Dir2);
            if (result)
                return true;
            result = TurretExist(pos + _Dir2) 
                    && PathExist(pos + _Dir1) 
                    && !PathExist(pos + _Dir1 + _Dir2)
                    && !TurretExist(pos + _Dir1 + _Dir2);
            return result;
        }

        private void AdjustBorders()
        {
            var pos = Props.Position;
            AdjustBorder(pos, V2Int.left, V2Int.down, V2Int.up, m_LeftBorderInited, ref m_LeftBorder);
            AdjustBorder(pos, V2Int.right, V2Int.down, V2Int.up, m_RightBorderInited, ref m_RightBorder);
            AdjustBorder(pos, V2Int.down, V2Int.left, V2Int.right, m_BottomBorderInited, ref m_BottomBorder);
            AdjustBorder(pos, V2Int.up, V2Int.left, V2Int.right, m_TopBorderInited, ref m_TopBorder);
        }

        private void AdjustBorder(
            V2Int _Position,
            V2Int _Direction,
            V2Int _Dir1, 
            V2Int _Dir2, 
            bool _BorderInitialized,
            ref Line _Border)
        {
            if (PathExist(_Position + _Direction) || !_BorderInitialized) 
                return;
            if (!TurretExist(_Position + _Direction))
            {
                var bPoints = GetBorderPointsAndDashed(_Direction, true, true);
                if (PathExist(_Position + _Direction + _Dir1) || TurretExist(_Position + _Direction + _Dir1))
                    _Border.Start = bPoints.Item1;
                if (PathExist(_Position + _Direction + _Dir2) || TurretExist(_Position + _Direction + _Dir2))
                    _Border.End = bPoints.Item2;
            }
            else
            {
                if (TurretDirection(_Position + _Direction) == _Dir1)
                    _Border.Start = Average(_Border.Start, _Border.End);
                else if (TurretDirection(_Position + _Direction) == _Dir2)
                    _Border.End = Average(_Border.Start, _Border.End);
            }
        }

        private void EnableInitializedShapes(bool _Enable)
        {
            if (m_Shape.IsNull())         m_Shape.enabled              = _Enable;
            
            if (m_LeftBorderInited)        m_LeftBorder.enabled        = _Enable;
            if (m_RightBorderInited)       m_RightBorder.enabled       = _Enable;
            if (m_BottomBorderInited)      m_BottomBorder.enabled      = _Enable;
            if (m_TopBorderInited)         m_TopBorder.enabled         = _Enable;
            
            if (m_BottomLeftCornerInited)  m_BottomLeftCorner.enabled  = _Enable;
            if (m_BottomRightCornerInited) m_BottomRightCorner.enabled = _Enable;
            if (m_TopLeftCornerInited)     m_TopLeftCorner.enabled     = _Enable;
            if (m_TopRightCornerInited)    m_TopRightCorner.enabled    = _Enable;
        }
        
        private void InitBorder(EMazeMoveDirection _Side)
        {
            Line border;
            switch (_Side)
            {
                case EMazeMoveDirection.Up:
                    border = m_TopBorder; break;
                case EMazeMoveDirection.Right:border = m_RightBorder; break;
                case EMazeMoveDirection.Down: border = m_BottomBorder; break;
                case EMazeMoveDirection.Left: border = m_LeftBorder; break;
                default: throw new SwitchCaseNotImplementedException(_Side);
            }
            if (border.IsNull())
                border = Object.AddComponentOnNewChild<Line>("Border", out _);
            border.Thickness = ViewSettings.LineWidth * CoordinateConverter.Scale;
            border.EndCaps = LineEndCap.None;
            border.Color = DrawingUtils.ColorLines.SetA(0.5f);
            (border.Start, border.End, border.Dashed) = GetBorderPointsAndDashed(_Side, false, false);
            border.transform.position = ContainersGetter.GetContainer(ContainerNames.MazeItems).transform.position;
            border.DashSize = 1f;
            border.enabled = false;
            switch (_Side)
            {
                case EMazeMoveDirection.Up: m_TopBorder = border; m_TopBorderInited = true; break;
                case EMazeMoveDirection.Right: m_RightBorder = border; m_RightBorderInited = true; break;
                case EMazeMoveDirection.Down: m_BottomBorder = border; m_BottomBorderInited = true; break;
                case EMazeMoveDirection.Left: m_LeftBorder = border; m_LeftBorderInited = true; break;
                default: throw new SwitchCaseNotImplementedException(_Side);
            }
        }

        private void InitCorner(
            bool _Right,
            bool _Up,
            bool _Inner)
        {
            Disc corner = null;
            if (!_Right && !_Up && m_BottomLeftCorner.IsNotNull())
                corner = m_BottomLeftCorner;
            else if (_Right && !_Up && m_BottomRightCorner.IsNotNull())
                corner = m_BottomRightCorner;
            else if (!_Right && _Up && m_TopLeftCorner.IsNotNull())
                corner = m_TopLeftCorner;
            else if (_Right && _Up && m_TopRightCorner.IsNotNull())
                corner = m_TopRightCorner;
            if (corner.IsNull())
                corner = Object.AddComponentOnNewChild<Disc>("Corner", out _);
            corner.transform.position = ContainersGetter.GetContainer(ContainerNames.MazeItems).transform.position;
            corner.transform.PlusLocalPosXY(GetCornerCenter(_Right, _Up, _Inner));
            corner.Type = DiscType.Arc;
            corner.ArcEndCaps = ArcEndCap.Round;
            corner.Radius = ViewSettings.CornerRadius * CoordinateConverter.Scale;
            corner.Thickness = ViewSettings.LineWidth * CoordinateConverter.Scale * 1.5f;
            var angles = GetCornerAngles(_Right, _Up, _Inner);
            corner.AngRadiansStart = Mathf.Deg2Rad * angles.x;
            corner.AngRadiansEnd = Mathf.Deg2Rad * angles.y;
            corner.Color = GetCornerColor(_Right, _Up, _Inner);
            corner.enabled = false;
            
            if (!_Right && !_Up)
            {
                m_BottomLeftCorner = corner;
                m_BottomLeftCornerCol = m_BottomLeftCorner.Color;
                m_BottomLeftCornerInited = true;
                if (!_Inner) return;
                if (m_BottomBorder.IsNotNull())
                    m_BottomBorder.Start = GetBorderPointsAndDashed(EMazeMoveDirection.Down, true, true).Item1;
                if (m_LeftBorder.IsNotNull())
                    m_LeftBorder.Start = GetBorderPointsAndDashed(EMazeMoveDirection.Left, true, true).Item1;
            }
            else if (_Right && !_Up)
            {
                m_BottomRightCorner = corner;
                m_BottomRightCornerCol = corner.Color;
                m_BottomRightCornerInited = true;
                if (!_Inner) return;
                if (m_BottomBorder.IsNotNull())
                    m_BottomBorder.End = GetBorderPointsAndDashed(EMazeMoveDirection.Down, true, true).Item2;
                if (m_RightBorder.IsNotNull())
                    m_RightBorder.Start = GetBorderPointsAndDashed(EMazeMoveDirection.Right, true, true).Item1;
            }
            else if (!_Right && _Up)
            {
                m_TopLeftCorner = corner;
                m_TopLeftCornerCol = corner.Color;
                m_TopLeftCornerInited = true;
                if (!_Inner) return;
                if (m_LeftBorder.IsNotNull())
                    m_LeftBorder.End = GetBorderPointsAndDashed(EMazeMoveDirection.Left, true, true).Item2;
                if (m_TopBorder.IsNotNull())
                    m_TopBorder.Start = GetBorderPointsAndDashed(EMazeMoveDirection.Up, true, true).Item1;
            }
            else if (_Right && _Up)
            {
                m_TopRightCorner = corner;
                m_TopRightCornerCol = corner.Color;
                m_TopRightCornerInited = true;
                if (!_Inner) return;
                if (m_TopBorder.IsNotNull())
                    m_TopBorder.End = GetBorderPointsAndDashed(EMazeMoveDirection.Up, true, true).Item2;
                if (m_RightBorder.IsNotNull())
                    m_RightBorder.End = GetBorderPointsAndDashed(EMazeMoveDirection.Right, true, true).Item2;
            }
        }

        private Tuple<Vector2, Vector2, bool> GetBorderPointsAndDashed(V2Int _Direction, bool _StartLimit,
            bool _EndLimit)
        {
            EMazeMoveDirection side = default;
            if (_Direction == V2Int.left)
                side = EMazeMoveDirection.Left;
            else if (_Direction == V2Int.right)
                side = EMazeMoveDirection.Right;
            else if (_Direction == V2Int.up)
                side = EMazeMoveDirection.Up;
            else if (_Direction == V2Int.down)
                side = EMazeMoveDirection.Down;
            return GetBorderPointsAndDashed(side, _StartLimit, _EndLimit);
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
            bool dashed = Model.GetAllProceedInfos().Any(_Item =>
                _Item.CurrentPosition == Props.Position + dir &&
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

        private static Vector2 GetCornerAngles(bool _Right, bool _Up, bool _Inner)
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

        private Color GetCornerColor(bool _Right, bool _Up, bool _Inner)
        {
            var defCol = DrawingUtils.ColorLines;
            var col1 = defCol.SetA(0.5f);
            if (_Inner)
                return DrawingUtils.ColorLines;
            if (!_Right && !_Up)
                return TrapIncreasingExist(Props.Position + V2Int.down + V2Int.left) ? col1 : defCol;
            if (_Right && !_Up)
                return TrapIncreasingExist(Props.Position + V2Int.down + V2Int.right) ? col1 : defCol;
            if (!_Right && _Up)
                return TrapIncreasingExist(Props.Position + V2Int.up + V2Int.left) ? col1 : defCol;
            if (_Right && _Up)
                return TrapIncreasingExist(Props.Position + V2Int.up + V2Int.right) ? col1 : defCol;
            return defCol;
        }

        private bool PathExist(V2Int _Position) => Model.PathItemsProceeder.PathProceeds.Keys.Contains(_Position);
        private bool BlockExist(V2Int _Position) => GetItemsInfo(_Position).Any();

        private bool TurretExist(V2Int _Position)
        {
            return GetItemInfo(_Position, EMazeItemType.Turret) != null;
        }

        private V2Int TurretDirection(V2Int _Position)
        {
            return GetItemInfo(_Position, EMazeItemType.Turret).Direction;
        }

        private bool SpringboardExist(V2Int _Position)
        {
            return GetItemInfo(_Position, EMazeItemType.Springboard) != null;
        }

        private V2Int SpringboardDirection(V2Int _Position)
        {
            return GetItemInfo(_Position, EMazeItemType.Springboard).Direction;
        }

        private bool TrapIncreasingExist(V2Int _Position)
        {
            return GetItemInfo(_Position, EMazeItemType.TrapIncreasing) != null;
        }

        private IEnumerable<IMazeItemProceedInfo> GetItemsInfo(V2Int _Position, EMazeItemType[] _Types = null)
        {
            if (_Types == null)
                _Types = Enum.GetValues(typeof(EMazeItemType)).Cast<EMazeItemType>().Except(EMazeItemType.Block).ToArray();
            return _Types.Select(_T => GetItemInfo(_Position, _T));
        }

        private IMazeItemProceedInfo GetItemInfo(V2Int _Position, EMazeItemType _Type)
        {
            return Model.GetAllProceedInfos()
                .FirstOrDefault(_Item => _Item.CurrentPosition == _Position
                                && _Item.Type == _Type); 
        }

        private static Vector3 Average(Vector3 _A, Vector3 _B) => (_A + _B) * 0.5f;

        protected override void OnAppearStart(bool _Appear)
        {
            base.OnAppearStart(_Appear);
            if (_Appear)
            {
                EnableInitializedShapes(true);
                Collected = false;
            }
        }

        protected override Dictionary<object[], Func<Color>> GetAppearSets(bool _Appear)
        {
            var cornersDictRaw = new Dictionary<Color, List<object>>();
            cornersDictRaw.Add(DrawingUtils.ColorLines, new List<object>());
            cornersDictRaw.Add(DrawingUtils.ColorLines.SetA(0.5f), new List<object>());

            if (m_BottomLeftCornerInited)
                cornersDictRaw[m_BottomLeftCornerCol].Add(m_BottomLeftCorner);
            if (m_BottomRightCornerInited)
                cornersDictRaw[m_BottomRightCornerCol].Add(m_BottomRightCorner);
            if (m_TopLeftCornerInited)
                cornersDictRaw[m_TopLeftCornerCol].Add(m_TopLeftCorner);
            if (m_TopRightCornerInited)
                cornersDictRaw[m_TopRightCornerCol].Add(m_TopRightCorner);

            var borders = new List<object>();
            if (m_BottomBorderInited)
                borders.Add(m_BottomBorder);
            if (m_TopBorderInited)
                borders.Add(m_TopBorder);
            if (m_LeftBorderInited)
                borders.Add(m_LeftBorder);
            if (m_RightBorderInited)
                borders.Add(m_RightBorder);

            var bordersDict = new Dictionary<object[], Func<Color>>
            {
                {
                    borders.ToArray(),
                    () => DrawingUtils.ColorLines.SetA(0.5f)
                }
            };
            var cornersDict = cornersDictRaw.
                ToDictionary(_Kvp => _Kvp.Value.ToArray(),
                    _Kvp => new Func<Color>(() => _Kvp.Key));
            var res = new[] {bordersDict, cornersDict}
                .SelectMany(_Dict => _Dict)
                .ToDictionary(_Kvp => _Kvp.Key, _Kvp => _Kvp.Value);

            if (_Appear && !Props.IsStartNode || !_Appear && !Collected)
                res.Add(new object[] {m_Shape}, () => DrawingUtils.ColorLines);
            else
                m_Shape.enabled = false;

            return res;
        }

        #endregion
    }
}