using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Utils;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItems.Props;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public class ViewMazeItemPath : ViewMazeItemPathBase, IUpdateTick
    {
        #region constants

        protected const float AdditionalScale = 0.01f;

        #endregion
        
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsCollectMoneyItem => 
            new AudioClipArgs("collect_point", EAudioClipType.GameSound);
        protected override string ObjectName => "Path Block";
        
        private Line      
            m_LeftBorder,      
            m_RightBorder,    
            m_BottomBorder,
            m_TopBorder;
        private Disc   
            m_BottomLeftCorner,
            m_BottomRightCorner, 
            m_TopLeftCorner, 
            m_TopRightCorner;
        private Line   
            m_BlankHatch1,  
            m_BlankHatch2;
        
        private bool 
            m_LeftBorderInited, 
            m_RightBorderInited,
            m_BottomBorderInited,
            m_TopBorderInited;
        protected bool 
            BottomLeftCornerInited,
            BottomRightCornerInited,
            TopLeftCornerInited, 
            TopRightCornerInited;
        protected bool
            IsBottomLeftCornerInner,
            IsBottomRightCornerInner,
            IsTopLeftCornerInner,
            IsTopRightCornerInner;
        private bool 
            m_IsLeftBorderInverseOffset,
            m_IsRightBorderInverseOffset,
            m_IsBottomBorderInverseOffset,
            m_IsTopBorderInverseOffset;
        
        private   float m_DashedOffset;
        
        #endregion
        
        #region inject
        
        
        protected ViewMazeItemPath(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            ICoordinateConverter        _CoordinateConverter,
            IContainersGetter           _ContainersGetter,
            IViewGameTicker             _GameTicker,
            IRendererAppearTransitioner _Transitioner,
            IManagersGetter             _Managers,
            IColorProvider              _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewMazeItemPathItem       _PathItem,
            IViewMazeItemsPathInformer  _Informer)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider,
                _CommandsProceeder,
                _PathItem,
                _Informer) { }
        
        #endregion

        #region api
        
        public override Component[] Renderers => new Component[]
        {
            m_LeftBorder, m_RightBorder, m_BottomBorder, m_TopBorder,
            m_BottomLeftCorner, m_BottomRightCorner, m_TopLeftCorner, m_TopRightCorner
        }.Concat(PathItem.Renderers)
            .ToArray();
        
        public override object Clone() => new ViewMazeItemPath(
            ViewSettings,
            Model,
            CoordinateConverter,
            ContainersGetter,
            GameTicker,
            Transitioner,
            Managers,
            ColorProvider,
            CommandsProceeder,
            PathItem.Clone() as IViewMazeItemPathItem,
            Informer);
        
        public override void UpdateState(ViewMazeItemProps _Props)
        {
            if (!Initialized)
                Managers.AudioManager.InitClip(AudioClipArgsCollectMoneyItem);
            PathItem.ItemPathItemMoney.Collected -= RaiseMoneyItemCollected;
            PathItem.ItemPathItemMoney.Collected += RaiseMoneyItemCollected;
            base.UpdateState(_Props);
        }
        
        public override void Collect(bool _Collect, bool _OnStart)
        {
            if (Props.Blank)
                return;
            PathItem.Collect(_Collect);
        }

        public override void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args) { }

        public override void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args) { }

        public override void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args) { }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            PathItem.OnLevelStageChanged(_Args);
        }

        public virtual void UpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            ShiftOffsetsOfDashedBorders();
            HighlightBordersAndCorners();
        }

        public override void Appear(bool _Appear)
        {
            PathItem.Appear(_Appear);
            base.Appear(_Appear);
        }

        #endregion
        
        #region nonpublic methods

        private void ShiftOffsetsOfDashedBorders()
        {
            const float maxOffset = 10f;
            float dOffset = GameTicker.DeltaTime * 3f;
            m_DashedOffset += dOffset;
            m_DashedOffset = MathUtils.ClampInverse(m_DashedOffset, 0, maxOffset);
            void SetBorderDashOffset<T>(T _Border, bool _Inited, bool _Inversed)
                where T : ShapeRenderer, IDashable
            {
                if (!_Inited || !_Border.Dashed) 
                    return;
                float offset = !_Inversed ? -m_DashedOffset : m_DashedOffset + 0.5f;
                _Border.DashOffset = offset;
            }
            SetBorderDashOffset(m_LeftBorder,   m_LeftBorderInited, m_IsLeftBorderInverseOffset);
            SetBorderDashOffset(m_RightBorder,  m_RightBorderInited, m_IsRightBorderInverseOffset);
            SetBorderDashOffset(m_BottomBorder, m_BottomBorderInited, m_IsBottomBorderInverseOffset);
            SetBorderDashOffset(m_TopBorder,    m_TopBorderInited, m_IsTopBorderInverseOffset);
        }

        protected virtual void HighlightBordersAndCorners()
        {
            // if (OverrideHighlightingOnCharacterMoveFinished)
            //     return;
            var col = Informer.GetHighlightColor();
            if (m_LeftBorderInited)      m_LeftBorder.Color        = col;
            if (m_RightBorderInited)     m_RightBorder.Color       = col;
            if (m_BottomBorderInited)    m_BottomBorder.Color      = col;
            if (m_TopBorderInited)       m_TopBorder.Color         = col;
            if (BottomLeftCornerInited)  m_BottomLeftCorner.Color  = col;
            if (TopLeftCornerInited)     m_TopLeftCorner.Color     = col;
            if (TopRightCornerInited)    m_TopRightCorner.Color    = col;
            if (BottomRightCornerInited) m_BottomRightCorner.Color = col;
        }

        protected override void InitShape()
        {
            PathItem.InitShape(() => Props, Object.transform);
            m_BlankHatch1 = Object.AddComponentOnNewChild<Line>("Blank Hatch 1", out _)
                .SetEndCaps(LineEndCap.None)
                .SetDashed(true)
                .SetDashSnap(DashSnapping.Off)
                .SetMatchDashSpacingToDashSize(true)
                .SetDashType(DashType.Angled)
                .SetDashSpace(DashSpace.FixedCount)
                .SetDashSize(6f)
                .SetDashShapeModifier(-1f)
                .SetSortingOrder(SortingOrders.Path);
            m_BlankHatch2 = Object.AddComponentOnNewChild<Line>("Blank Hatch 2", out _)
                .SetEndCaps(LineEndCap.None)
                .SetDashed(true)
                .SetDashSnap(DashSnapping.Off)
                .SetMatchDashSpacingToDashSize(true)
                .SetDashType(DashType.Angled)
                .SetDashSpace(DashSpace.FixedCount)
                .SetDashSize(6f)
                .SetDashShapeModifier(1f)
                .SetSortingOrder(SortingOrders.Path);
        }

        protected override void UpdateShape()
        {
            PathItem.UpdateShape();
            float scale = CoordinateConverter.Scale;
            m_BlankHatch1.enabled = m_BlankHatch2.enabled = Props.Blank;
            if (Props.Blank)
            {
                Collect(true, true);
                m_BlankHatch1
                    .SetStart(Vector2.left * 0.5f * scale)
                    .SetEnd(Vector2.right * 0.5f * scale)
                    .SetThickness(scale);
                m_BlankHatch2
                    .SetStart(Vector2.left * 0.5f * scale)
                    .SetEnd(Vector2.right * 0.5f * scale)
                    .SetThickness(scale);
            }
            if (ViewSettings.collectStartPathItemOnLevelLoaded 
                && Model.PathItemsProceeder.PathProceeds[Props.Position])
                Collect(true, true);
            if (Informer.IsAnyBlockOfConcreteTypeWithSamePosition(EMazeItemType.ShredingerBlock)
                || Informer.IsAnyBlockOfConcreteTypeWithSamePosition(EMazeItemType.Portal))
            {
                Collect(true, true);
            }
            DrawBordersAndCorners();
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            switch (_ColorId)
            {
                case ColorIds.Main:
                    m_BlankHatch1.Color = m_BlankHatch2.Color = _Color.SetA(0.25f);
                    if (BottomLeftCornerInited)  m_BottomLeftCorner.Color  = _Color;
                    if (BottomRightCornerInited) m_BottomRightCorner.Color = _Color;
                    if (TopLeftCornerInited)     m_TopLeftCorner.Color     = _Color;
                    if (TopRightCornerInited)    m_TopRightCorner.Color    = _Color;
                    var borderCol = GetBorderColor();
                    if (m_LeftBorderInited)      m_LeftBorder.Color        = borderCol;
                    if (m_RightBorderInited)     m_RightBorder.Color       = borderCol;
                    if (m_BottomBorderInited)    m_BottomBorder.Color      = borderCol;
                    if (m_TopBorderInited)       m_TopBorder.Color         = borderCol;
                    break;
            }
        }

        private void DrawBordersAndCorners()
        {
            DrawBorders();
            DrawCorners();
            // AdjustBorders();
            EnableInitializedShapes(false);
        }

        protected virtual void DrawBorders()
        {
            var vals = (false, 0f);
            if (m_LeftBorderInited)   (m_LeftBorder.enabled,   m_LeftBorder.DashOffset)   = vals;
            if (m_RightBorderInited)  (m_RightBorder.enabled,  m_RightBorder.DashOffset)  = vals;
            if (m_BottomBorderInited) (m_BottomBorder.enabled, m_BottomBorder.DashOffset) = vals;
            if (m_TopBorderInited)    (m_TopBorder.enabled,    m_TopBorder.DashOffset)    = vals;
            m_LeftBorderInited = m_RightBorderInited = m_BottomBorderInited = m_TopBorderInited = false;
            m_IsLeftBorderInverseOffset = m_IsRightBorderInverseOffset =
                m_IsBottomBorderInverseOffset = m_IsTopBorderInverseOffset = false;
            var sides = Enum
                .GetValues(typeof(EDirection))
                .Cast<EDirection>();
            foreach (var side in sides)
            {
                if (!Informer.MustInitBorder(side))
                    continue;
                DrawBorder(side);
            }
        }

        private void DrawCorners()
        {
            if (BottomLeftCornerInited)  m_BottomLeftCorner .enabled = false;
            if (BottomRightCornerInited) m_BottomRightCorner.enabled = false;
            if (TopLeftCornerInited)     m_TopLeftCorner    .enabled = false;
            if (TopRightCornerInited)    m_TopRightCorner   .enabled = false;
            BottomLeftCornerInited = BottomRightCornerInited = TopLeftCornerInited = TopRightCornerInited = false;
            DrawInnerCorners();
        }

        private void DrawInnerCorners()
        {
            var pos = Props.Position;
            bool initLeftBottomCorner = !Informer.PathExist(pos + V2Int.Left)
                                      && !Informer.PathExist(pos + V2Int.Down)
                                      && !Informer.PathExist(pos + V2Int.Left + V2Int.Down)
                                      || Informer.SpringboardExist(pos) 
                                      && Informer.GetSpringboardDirection(pos) == V2Int.Right + V2Int.Up;
            if (initLeftBottomCorner)
                DrawCorner(false, false, true);
            bool initLeftTopCorner = !Informer.PathExist(pos + V2Int.Left)
                                     && !Informer.PathExist(pos + V2Int.Up)
                                     && !Informer.PathExist(pos + V2Int.Left + V2Int.Up)
                                     || Informer.SpringboardExist(pos)
                                     && Informer.GetSpringboardDirection(pos) == V2Int.Right + V2Int.Down;
            if (initLeftTopCorner)
                DrawCorner(false, true, true);
            bool initRightBottomCorner = !Informer.PathExist(pos + V2Int.Right)
                                         && !Informer.PathExist(pos + V2Int.Down)
                                         && !Informer.PathExist(pos + V2Int.Right + V2Int.Down)
                                         || Informer.SpringboardExist(pos)
                                         && Informer.GetSpringboardDirection(pos) == V2Int.Left + V2Int.Up;
            if (initRightBottomCorner)
                DrawCorner(true, false, true);
            bool initRightTopCorner = !Informer.PathExist(pos + V2Int.Right)
                                      && !Informer.PathExist(pos + V2Int.Up)
                                      && !Informer.PathExist(pos + V2Int.Right + V2Int.Up)
                                      || Informer.SpringboardExist(pos)
                                      && Informer.GetSpringboardDirection(pos) == V2Int.Left + V2Int.Down;
            if (initRightTopCorner)
                DrawCorner(true, true, true);
        }

        private void DrawBorder(EDirection _Side)
        {
            Line border = _Side switch
            {
                EDirection.Up    => m_TopBorder,
                EDirection.Right => m_RightBorder,
                EDirection.Down  => m_BottomBorder,
                EDirection.Left  => m_LeftBorder,
                _                        => throw new SwitchCaseNotImplementedException(_Side)
            };
            if (border.IsNull())
                border = Object.AddComponentOnNewChild<Line>("Border", out _);
            border.SetThickness(ViewSettings.PathItemBorderThickness * CoordinateConverter.Scale)
                .SetEndCaps(LineEndCap.None)
                .SetSortingOrder(SortingOrders.PathLine)
                .SetDashed(false)
                .SetDashType(DashType.Rounded)
                .SetDashSnap(DashSnapping.Off)
                .SetDashSpace(DashSpace.FixedCount);
            (border.Start, border.End, _) = 
                GetBorderPointsAndDashed(_Side, false, false);
            border.transform.position = ContainersGetter.GetContainer(ContainerNamesMazor.MazeItems).transform.position;
            border.enabled = false;
            switch (_Side)
            {
                case EDirection.Up:    m_TopBorder    = border; m_TopBorderInited    = true; break;
                case EDirection.Right: m_RightBorder  = border; m_RightBorderInited  = true; break;
                case EDirection.Down:  m_BottomBorder = border; m_BottomBorderInited = true; break;
                case EDirection.Left:  m_LeftBorder   = border; m_LeftBorderInited   = true; break;
                default:                       throw new SwitchCaseNotImplementedException(_Side);
            }
        }

        private void DrawCorner(
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
            var angles = Informer.GetCornerAngles(_Right, _Up, _Inner);
            var positions = new []
            {
                Props.Position + (_Right ? V2Int.Right : V2Int.Left),
                Props.Position + (_Up ? V2Int.Up : V2Int.Down)
            };
            bool isConerNearTurret =  positions.Any(
                _P => Informer.GetItemInfoByPositionAndType(
                    _P, EMazeItemType.Turret) != null);
            positions = new []
            {
                Props.Position + (_Right ? V2Int.Right : V2Int.Left) + (_Up ? V2Int.Up : V2Int.Down)
            };
            bool isCornerNearTrapIncreasing = positions.Any(
                _P => Informer.GetItemInfoByPositionAndType(
                    _P, EMazeItemType.TrapIncreasing) != null);
            var pos = ContainersGetter.GetContainer(ContainerNamesMazor.MazeItems).transform.position;
            var scale = CoordinateConverter.Scale;
            corner.SetType(DiscType.Arc)
                .SetArcEndCaps(isConerNearTurret || isCornerNearTrapIncreasing ? ArcEndCap.Round : ArcEndCap.None)
                .SetRadius((ViewSettings.PathItemBorderThickness * (_Inner ? .5f : -.5f)) * scale)
                .SetThickness(ViewSettings.PathItemBorderThickness * scale)
                .SetAngRadiansStart(Mathf.Deg2Rad * angles.x)
                .SetAngRadiansEnd(Mathf.Deg2Rad * angles.y)
                .SetSortingOrder(SortingOrders.PathJoint)
                .transform.SetPosXY(pos)
                .PlusLocalPosXY(GetCornerCenter(_Right, _Up, _Inner));
            corner!.enabled = false;
            if (!_Right && !_Up)
            {
                m_BottomLeftCorner = corner;
                BottomLeftCornerInited = true;
                IsBottomLeftCornerInner = _Inner;
            }
            else if (_Right && !_Up)
            {
                m_BottomRightCorner = corner;
                BottomRightCornerInited = true;
                IsBottomRightCornerInner = _Inner;
            }
            else if (!_Right)
            {
                m_TopLeftCorner = corner;
                TopLeftCornerInited = true;
                IsTopLeftCornerInner = _Inner;
            }
            else
            {
                m_TopRightCorner = corner;
                TopRightCornerInited = true;
                IsTopRightCornerInner = _Inner;
            }
        }
        
        private Tuple<Vector2, Vector2, bool> GetBorderPointsAndDashed(
            EDirection _Side,
            bool       _StartLimit,
            bool       _EndLimit)
        {
            Vector2 start, end;
            (start, end) = Informer.GetBorderPointsRaw(
                _Side, 
                _StartLimit, 
                _EndLimit, 
                1f + AdditionalScale);
            start = CoordinateConverter.ToLocalMazeItemPosition(start);
            end = CoordinateConverter.ToLocalMazeItemPosition(end);
            var dir = RmazorUtils.GetDirectionVector(_Side, EMazeOrientation.North);
            start += dir * CoordinateConverter.Scale * ViewSettings.PathItemBorderThickness * Vector2.one * .5f;
            end   += dir * CoordinateConverter.Scale * ViewSettings.PathItemBorderThickness * Vector2.one * .5f;
            bool dashed = Model.GetAllProceedInfos().Any(_Item =>
                _Item.CurrentPosition == Props.Position + dir 
                && (_Item.Type == EMazeItemType.TrapReact && _Item.Direction == -dir
                    || _Item.Type == EMazeItemType.TrapIncreasing));
            
            return new Tuple<Vector2, Vector2, bool>(start, end, dashed);
        }

        private Vector2 GetCornerCenter(
            bool _Right,
            bool _Up,
            bool _Inner)
        {
            Vector2 pos = Props.Position;
            var dir = (_Right ? Vector2.right : Vector2.left)
                      + (_Up ? Vector2.up : Vector2.down);
            var center = pos
                         + dir * 0.5f;
            center = CoordinateConverter.ToLocalMazeItemPosition(center);
            return center;
        }

        protected override void OnAppearStart(bool _Appear)
        {
            if (!_Appear && (!Props.Blank && PathItem.ItemPathItemMoney.IsCollected) 
                || _Appear && (Props.Blank || Props.IsStartNode))
            {
                PathItem.EnableInitializedShapes(false);
            }
            if (_Appear)
                EnableInitializedShapes(true);
        }

        protected override void OnAppearFinish(bool _Appear)
        {
            if (!_Appear && Props.Blank)
            {
                m_BlankHatch1.enabled = false;
                m_BlankHatch2.enabled = false;
            }
            base.OnAppearFinish(_Appear);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var borderSets = GetBorderAppearSets();
            var cornerSets = GetCornerAppearSets();
            var result = borderSets.ConcatWithDictionary(cornerSets);
            var mainCol = ColorProvider.GetColor(ColorIds.Main);
            if (Props.Blank)
                result.Add(new [] {m_BlankHatch1, m_BlankHatch2}, () => mainCol.SetA(0.25f));
            return result;
        }
        
        private Dictionary<IEnumerable<Component>, Func<Color>> GetBorderAppearSets()
        {
            var mainBorders = new List<Component>();
            if (m_BottomBorderInited) mainBorders.Add(m_BottomBorder);
            if (m_TopBorderInited)    mainBorders.Add(m_TopBorder);
            if (m_LeftBorderInited)   mainBorders.Add(m_LeftBorder);
            if (m_RightBorderInited)  mainBorders.Add(m_RightBorder);
            var mainBordersCol = GetBorderColor();
            var sets = new Dictionary<IEnumerable<Component>, Func<Color>> {{mainBorders, () => mainBordersCol}};
            return sets;
        }
        
        private Dictionary<IEnumerable<Component>, Func<Color>> GetCornerAppearSets()
        {
            var setsRaw = new Dictionary<Color, List<Component>>();
            var defCornerCol = ColorProvider.GetColor(ColorIds.Main);
            setsRaw.Add(defCornerCol, new List<Component>());
            if (BottomLeftCornerInited)
                setsRaw[defCornerCol].Add(m_BottomLeftCorner);
            if (BottomRightCornerInited)
                setsRaw[defCornerCol].Add(m_BottomRightCorner);
            if (TopLeftCornerInited)
                setsRaw[defCornerCol].Add(m_TopLeftCorner);
            if (TopRightCornerInited)
                setsRaw[defCornerCol].Add(m_TopRightCorner);
            var sets = setsRaw.
                ToDictionary(
                    _Kvp => (IEnumerable<Component>)_Kvp.Value,
                    _Kvp => new Func<Color>(() => _Kvp.Key));
            return sets;
        }
        
        protected override void EnableInitializedShapes(bool _Enable)
        {
            PathItem.EnableInitializedShapes(_Enable);
            if (m_LeftBorderInited)      m_LeftBorder       .enabled = _Enable;
            if (m_RightBorderInited)     m_RightBorder      .enabled = _Enable;
            if (m_BottomBorderInited)    m_BottomBorder     .enabled = _Enable;
            if (m_TopBorderInited)       m_TopBorder        .enabled = _Enable;
            if (BottomLeftCornerInited)  m_BottomLeftCorner .enabled = _Enable;
            if (BottomRightCornerInited) m_BottomRightCorner.enabled = _Enable;
            if (TopLeftCornerInited)     m_TopLeftCorner    .enabled = _Enable;
            if (TopRightCornerInited)    m_TopRightCorner   .enabled = _Enable;
        }

        protected Color GetBorderColor()
        {
            return ColorProvider.GetColor(ColorIds.Main);
        }

        #endregion
    }
}