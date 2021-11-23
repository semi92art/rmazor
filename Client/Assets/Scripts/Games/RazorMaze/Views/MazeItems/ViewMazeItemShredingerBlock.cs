using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using DI.Extensions;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemShredingerBlock : IViewMazeItem, ICharacterMoveFinished
    {
        bool BlockClosed { get; set; }
    }
    
    public class ViewMazeItemShredingerBlock : ViewMazeItemBase, IViewMazeItemShredingerBlock, IUpdateTick
    {
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsOpenBlock => 
            new AudioClipArgs("shredinger_open", EAudioClipType.Sound);
        private static AudioClipArgs AudioClipArgsCloseBlock =>
            new AudioClipArgs("shredinger_close", EAudioClipType.Sound);
        
        private float m_LineOffset;
        private bool  m_IsBlockClosed;
        private bool  m_IsCloseCoroutineRunning;
        private bool  m_IsOpenCoroutineRunning;
        private bool  m_IsCloseOrOpenImmediately;
        
        #endregion

        #region shapes

        protected override string     ObjectName => "Shredinger Block";
        private readonly   List<Line> m_OpenedLines   = new List<Line>();
        private readonly   List<Disc> m_OpenedCorners = new List<Disc>();
        private            Rectangle  m_ClosedBlock;

        #endregion
        
        #region inject
        
        public ViewMazeItemShredingerBlock(
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
        
        public override Component[] Shapes =>
            new Component[] {m_ClosedBlock}
                .Concat(m_OpenedLines)
                .Concat(m_OpenedCorners)
                .ToArray();
        
        public override object Clone() => new ViewMazeItemShredingerBlock(
            ViewSettings, 
            Model,
            CoordinateConverter,
            ContainersGetter,
            GameTicker,
            Transitioner,
            Managers,
            ColorProvider,
            CommandsProceeder);
        
        public bool BlockClosed
        {
            get => m_IsBlockClosed;
            set
            {
                m_IsBlockClosed = value;
                if (!ActivatedInSpawnPool)
                    return;
                if (value)
                    CloseBlock();
                else OpenBlock();
            }
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            if (_Args.Stage == ELevelStage.Loaded)
                m_IsBlockClosed = false;
            if (_Args.Stage == ELevelStage.ReadyToStart)
                Coroutines.Run(CloseBlockCoroutine(false, true));
        }

        public void UpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            if (ProceedingStage == EProceedingStage.Inactive)
                return;
            ProceedOpenedBlockState();
        }
        
        public void OnCharacterMoveFinished(CharacterMovingEventArgs _Args)
        {
            const float delta = 0.5f;
            const float duration = 0.1f;
            var startPos = m_ClosedBlock.transform.localPosition;
            Vector2 dir = RazorMazeUtils.GetDirectionVector(_Args.Direction, Model.Data.Orientation);
            Coroutines.Run(Coroutines.Lerp(
                0f,
                delta,
                duration * 0.5f,
                _Progress => m_ClosedBlock.transform.localPosition = startPos + (Vector3) dir * _Progress,
                GameTicker,
                (_Finished, _) =>
                {
                    Coroutines.Run(Coroutines.Lerp(
                        delta,
                        0f,
                        duration * 0.5f,
                        _Progress => m_ClosedBlock.transform.localPosition = startPos + (Vector3) dir * _Progress,
                        GameTicker));
                }));
        }
        
        public override bool ActivatedInSpawnPool
        {
            get => m_ActivatedInSpawnPool;
            set
            {
                m_ActivatedInSpawnPool = value;
                ActivateShapes(false);
            }
        }
        
        #endregion
        
        #region nonpublic methods

        protected override void InitShape()
        {
            var closedBlock = Object.AddComponentOnNewChild<Rectangle>("Shredinger Block", out _);
            closedBlock.Type = Rectangle.RectangleType.RoundedBorder;
            closedBlock.Color = ColorProvider.GetColor(ColorIds.Main);
            closedBlock.SortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type);
            m_ClosedBlock = closedBlock;
            
            var lLeft    = Object.AddComponentOnNewChild<Line>("Left Line",     out _);
            var lRight   = Object.AddComponentOnNewChild<Line>("Right Line",    out _);
            var lBottom1 = Object.AddComponentOnNewChild<Line>("Bottom Line 1", out _);
            var lBottom2 = Object.AddComponentOnNewChild<Line>("Bottom Line 2", out _);
            var lTop1    = Object.AddComponentOnNewChild<Line>("Top Line 1",    out _);
            var lTop2    = Object.AddComponentOnNewChild<Line>("Top Line 2",    out _);
            var lCenter1 = Object.AddComponentOnNewChild<Line>("Center Line 1", out _);
            var lCenter2 = Object.AddComponentOnNewChild<Line>("Center Line 2", out _);

            var lp = GetLineStartEndPositions();
            (lBottom1.Start, lBottom1.End) = (lp[0], lp[1]);
            (lLeft.Start, lLeft.End)       = (lp[2], lp[3]);
            (lTop1.Start, lTop1.End)       = (lp[4], lp[5]);
            (lCenter1.Start, lCenter1.End) = (lp[6], lp[7]);
            (lBottom2.Start, lBottom2.End) = (lp[8], lp[9]);
            (lRight.Start, lRight.End)     = (lp[10], lp[11]);
            (lTop2.Start, lTop2.End)       = (lp[12], lp[13]);
            (lCenter2.Start, lCenter2.End) = (lp[14], lp[15]);
            
            m_OpenedLines.AddRange(new []
            {
                lLeft, lRight, lBottom1, lBottom2, lTop1, lTop2, lCenter1, lCenter2
            });
            foreach (var line in m_OpenedLines)
            {
                line.Dashed = true;
                line.DashSize = 2f;
                line.DashType = DashType.Rounded;
                line.Color = ColorProvider.GetColor(ColorIds.Main);
                line.SortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type);
            }

            List<Vector2> cPoss, cAngs;
            (cPoss, cAngs) = GetCornerCenterPositionsAndAngles();

            var cBL  = Object.AddComponentOnNewChild<Disc>("Bottom Left Corner",     out _);
            var cTL  = Object.AddComponentOnNewChild<Disc>("Top Left Corner",        out _);
            var cBR  = Object.AddComponentOnNewChild<Disc>("Bottom Right Corner",    out _);
            var cTR  = Object.AddComponentOnNewChild<Disc>("Top Right Corner",       out _);
            var cBC1 = Object.AddComponentOnNewChild<Disc>("Bottom Center Corner 1", out _);
            var cBC2 = Object.AddComponentOnNewChild<Disc>("Bottom Center Corner 2", out _);
            var cTC1 = Object.AddComponentOnNewChild<Disc>("Top Center Corner 1",    out _);
            var cTC2 = Object.AddComponentOnNewChild<Disc>("Top Center Corner 2",    out _);

            (cBL.transform.localPosition, cBL.AngRadiansStart, cBL.AngRadiansEnd)    = (cPoss[0], cAngs[0].x, cAngs[0].y);
            (cTL.transform.localPosition, cTL.AngRadiansStart, cTL.AngRadiansEnd)    = (cPoss[1], cAngs[1].x, cAngs[1].y);
            (cBR.transform.localPosition, cBR.AngRadiansStart, cBR.AngRadiansEnd)    = (cPoss[2], cAngs[2].x, cAngs[2].y);
            (cTR.transform.localPosition, cTR.AngRadiansStart, cTR.AngRadiansEnd)    = (cPoss[3], cAngs[3].x, cAngs[3].y);
            (cBC1.transform.localPosition, cBC1.AngRadiansStart, cBC1.AngRadiansEnd) = (cPoss[4], cAngs[4].x, cAngs[4].y);
            (cBC2.transform.localPosition, cBC2.AngRadiansStart, cBC2.AngRadiansEnd) = (cPoss[5], cAngs[5].x, cAngs[5].y);
            (cTC1.transform.localPosition, cTC1.AngRadiansStart, cTC1.AngRadiansEnd) = (cPoss[6], cAngs[6].x, cAngs[6].y);
            (cTC2.transform.localPosition, cTC2.AngRadiansStart, cTC2.AngRadiansEnd) = (cPoss[7], cAngs[7].x, cAngs[7].y);
            
            m_OpenedCorners.AddRange(new []
            {
                cBL, cTL, cBR, cTR, cBC1, cBC2, cTC1, cTC2
            });

            foreach (var corner in m_OpenedCorners)
            {
                corner.Type = DiscType.Arc;
                corner.Color = ColorProvider.GetColor(ColorIds.Main);
                corner.SortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type);
                corner.ArcEndCaps = ArcEndCap.Round;
            }
        }

        protected override void UpdateShape()
        {
            Object.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            m_ClosedBlock.Width = m_ClosedBlock.Height = CoordinateConverter.Scale * 0.9f;
            m_ClosedBlock.CornerRadius = ViewSettings.CornerRadius * CoordinateConverter.Scale;
            m_ClosedBlock.Thickness = ViewSettings.LineWidth * CoordinateConverter.Scale;
            foreach (var corner in m_OpenedCorners)
            {
                corner.Radius = GetCornerRadius();
                corner.Thickness = ViewSettings.LineWidth * CoordinateConverter.Scale;
            }
            foreach (var line in m_OpenedLines)
            {
                line.Thickness = ViewSettings.LineWidth * CoordinateConverter.Scale;
            }
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.Main)
            {
                m_ClosedBlock.Color = _Color;
                foreach (var item in m_OpenedCorners)
                    item.Color = _Color;
                foreach (var item in m_OpenedLines)
                    item.Color = _Color;
            }
        }

        private void ProceedOpenedBlockState()
        {
            m_LineOffset += GameTicker.DeltaTime * ViewSettings.ShredingerLineOffsetSpeed;
            for (int i = 0; i < m_OpenedLines.Count; i++)
                m_OpenedLines[i].DashOffset = m_LineOffset;
        }
        
        private void CloseBlock()
        {
            Managers.AudioManager.PlayClip(AudioClipArgsCloseBlock);
            Coroutines.Run(CloseBlockCoroutine(true));
        }

        private void OpenBlock()
        {
            Managers.AudioManager.PlayClip(AudioClipArgsOpenBlock);
            Coroutines.Run(CloseBlockCoroutine(false));
        }

        private IEnumerator CloseBlockCoroutine(bool _Close, bool _Immediately = false)
        {
            m_IsCloseOrOpenImmediately = _Immediately;
            if (_Immediately)
            {
                if (!ActivatedInSpawnPool)
                    yield break;
                var shapesOpen1 = m_OpenedLines
                    .Cast<ShapeRenderer>()
                    .Concat(m_OpenedCorners)
                    .ToList();
                m_ClosedBlock.enabled = _Close;
                shapesOpen1.ForEach(_Shape => _Shape.enabled = !_Close);
                m_IsBlockClosed = _Close;
                shapesOpen1.ForEach(_Shape => _Shape.Color = ColorProvider.GetColor(ColorIds.Main));
                m_ClosedBlock.Color = ColorProvider.GetColor(ColorIds.Main);
                yield break;
            }
            
            if (_Close)
                m_IsCloseCoroutineRunning = true;
            else 
                m_IsOpenCoroutineRunning = true;

            if (_Close)
            {
                while (m_IsOpenCoroutineRunning)
                    yield return null;
            }
            else
            {
                while (m_IsCloseCoroutineRunning)
                    yield return null;
            }

            if (m_IsCloseOrOpenImmediately)
            {
                m_IsCloseOrOpenImmediately = false;
                yield break;
            }
            
            var shapesOpen = m_OpenedLines
                .Cast<ShapeRenderer>()
                .Concat(m_OpenedCorners)
                .ToList();
            if (_Close)
                m_ClosedBlock.enabled = true;
            else
                shapesOpen.ForEach(_Shape => _Shape.enabled = true);
            
            yield return Coroutines.Lerp(
                0f,
                1f,
                0.2f,
                _Progress =>
                {
                    float cAppear = 1f - (_Progress - 1f) * (_Progress - 1f);
                    float cDissapear = 1f - _Progress * _Progress;
                    var partsOpenColor = ColorProvider.GetColor(ColorIds.Main).SetA(_Close ? cDissapear : cAppear);
                    var partsClosedColor = ColorProvider.GetColor(ColorIds.Main).SetA(_Close ? cAppear : cDissapear);
                    shapesOpen.ForEach(_Shape => _Shape.Color = partsOpenColor);
                    m_ClosedBlock.Color = partsClosedColor;
                },
                GameTicker,
                (_Breaked, _Progress) =>
                {
                    if (_Close)
                        m_IsCloseCoroutineRunning = false;
                    else 
                        m_IsOpenCoroutineRunning = false;
                    if (_Breaked)
                        return;
                    if (_Close)
                        shapesOpen.ForEach(_Shape => _Shape.enabled = false);
                    else
                        m_ClosedBlock.enabled = false;
                },
                () => m_IsCloseOrOpenImmediately);
        }

        protected override void OnAppearStart(bool _Appear)
        {
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
            m_ClosedBlock.enabled = BlockClosed;
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var shapes = !_Appear && BlockClosed ?
                new Component[] {m_ClosedBlock} :
                m_OpenedLines.Cast<Component>().Concat(m_OpenedCorners);
            return new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {shapes, () => ColorProvider.GetColor(ColorIds.Main)}
            };
        }

        private List<Vector2> GetLineStartEndPositions()
        {
            float cr = GetCornerRadius();
            var cornerPositions = GetCornerPositions();
            var centerPositions = GetCenterLinePositions();
            return new List<Vector2>
            {
                centerPositions[0] + Vector2.left  * cr,   // bottom 1 start
                cornerPositions[0] + Vector2.right * cr,   // bottom 1 end
                cornerPositions[0] + Vector2.up    * cr,   // left start
                cornerPositions[1] + Vector2.down  * cr,   // left end
                cornerPositions[1] + Vector2.right * cr,   // top 1 start
                centerPositions[1] + Vector2.left  * cr,   // top 1 end
                centerPositions[1] + Vector2.down  * cr,   // center 1 start
                centerPositions[0] + Vector2.up    * cr,   // center 1 end
                centerPositions[3] + Vector2.right * cr,   // bottom 2 start
                cornerPositions[3] + Vector2.left  * cr,   // bottom 2 end
                cornerPositions[3] + Vector2.up    * cr,   // right start
                cornerPositions[2] + Vector2.down  * cr,   // right end
                cornerPositions[2] + Vector2.left  * cr,   // top 2 start
                centerPositions[2] + Vector2.right * cr,   // top 2 end
                centerPositions[2] + Vector2.down  * cr,   // center 2 start
                centerPositions[3] + Vector2.up    * cr    // center 2 end
            };
        }

        private Tuple<List<Vector2>, List<Vector2>> GetCornerCenterPositionsAndAngles()
        {
            float cr = GetCornerRadius();
            var cornerPositions = GetCornerPositions();
            var centerPositions = GetCenterLinePositions();
            var positions = new List<Vector2>
            {
                cornerPositions[0] + Vector2.right * cr + Vector2.up    * cr,   // bottom left    
                cornerPositions[3] + Vector2.left  * cr  + Vector2.up   * cr,   // bottom right   
                cornerPositions[1] + Vector2.right * cr + Vector2.down  * cr,   // top left       
                cornerPositions[2] + Vector2.left  * cr  + Vector2.down * cr,   // top right      
                centerPositions[0] + Vector2.left  * cr  + Vector2.up   * cr,   // bottom center 1
                centerPositions[3] + Vector2.right * cr + Vector2.up    * cr,   // bottom center 2
                centerPositions[1] + Vector2.left  * cr  + Vector2.down * cr,   // top center 1   
                centerPositions[2] + Vector2.right * cr + Vector2.down  * cr    // top center 2   
            };
            var angles = new List<Vector2>
            {
                new Vector2(180f, 270f), // bottom left    
                new Vector2(0f, -90f),   // bottom right   
                new Vector2(180f, 90f),  // top left       
                new Vector2(0f, 90f),    // top right      
                new Vector2(0f, -90f),   // bottom center 1
                new Vector2(180f, 270f), // bottom center 2
                new Vector2(0f, 90f),    // top center 1   
                new Vector2(180f, 90f)   // top center 2   
            };
            positions = positions
                .Select(_Pos => _Pos)
                .ToList();
            angles = angles
                .Select(_Angles => _Angles * Mathf.Deg2Rad)
                .ToList();
            return new Tuple<List<Vector2>, List<Vector2>>(positions, angles);
        }
        
        private List<Vector2> GetCornerPositions()
        {
            const float c = 0.43f;
            float s = CoordinateConverter.Scale;
            var bottomLeft  = (Vector2.down + Vector2.left)  * (c * s);
            var bottomRight = (Vector2.down + Vector2.right) * (c * s);
            var topLeft     = (Vector2.up   + Vector2.left)  * (c * s);
            var topRight    = (Vector2.up   + Vector2.right) * (c * s);
            return new List<Vector2> {bottomLeft, topLeft, topRight, bottomRight};
        }
        
        private List<Vector2> GetCenterLinePositions()
        {
            const float c1 = 0.43f;
            const float c2 = 0.1f;
            float s = CoordinateConverter.Scale;
            var bottomLeft  = (Vector2.down + Vector2.left  * c2) * (c1 * s);
            var bottomRight = (Vector2.down + Vector2.right * c2) * (c1 * s);
            var topLeft     = (Vector2.up   + Vector2.left  * c2) * (c1 * s);
            var topRight    = (Vector2.up   + Vector2.right * c2) * (c1 * s);
            return new List<Vector2> {bottomLeft, topLeft, topRight, bottomRight};
        }

        private float GetCornerRadius() => ViewSettings.CornerRadius * CoordinateConverter.Scale * 0.5f;

        #endregion
    }
}