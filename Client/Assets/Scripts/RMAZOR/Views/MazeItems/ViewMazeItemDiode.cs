using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemDiode : IViewMazeItem
    {
        void OnDiodeBlock(DiodeEventArgs _Args);
        void OnDiodePass(DiodeEventArgs  _Args);
    }
    
    public class ViewMazeItemDiode : ViewMazeItemBase, IViewMazeItemDiode, IUpdateTick
    {
        #region nonpublic members

        protected override string ObjectName => "Diode Block";

        private Line  m_SolidLine, m_IntermittentLine;
        private float m_DashedOffset;

        #endregion

        #region inject
        
        private ViewMazeItemDiode(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            ICoordinateConverter        _CoordinateConverter,
            IContainersGetter           _ContainersGetter,
            IViewGameTicker             _GameTicker,
            IRendererAppearTransitioner _Transitioner,
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

        public override Component[] Renderers => new Component[] {};

        public override object Clone()
        {
            return new ViewMazeItemDiode(
                ViewSettings, 
                Model,
                CoordinateConverter, 
                ContainersGetter, 
                GameTicker,
                Transitioner, 
                Managers,
                ColorProvider,
                CommandsProceeder);
        }
        
        public void OnDiodeBlock(DiodeEventArgs _Args)
        {
            
        }

        public void OnDiodePass(DiodeEventArgs _Args)
        {
            
        }
        
        public void UpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            ShiftOffsetsOfIntermittentLine();
        }

        #endregion

        #region nonpublic methods
        
        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.MazeItem2)
                return;
            m_SolidLine.SetColor(_Color);
            m_IntermittentLine.SetColor(_Color);
        }
        
        protected override void InitShape()
        {
            m_SolidLine = Object.AddComponentOnNewChild<Line>("Diode Solid Line", out _)
                .SetSortingOrder(SortingOrders.GetBlockSortingOrder(Props.Type))
                .SetEndCaps(LineEndCap.Round)
                .SetDashed(false);
            m_IntermittentLine = Object.AddComponentOnNewChild<Line>("Diode Intermittent Line", out _)
                .SetSortingOrder(SortingOrders.GetBlockSortingOrder(Props.Type))
                .SetEndCaps(LineEndCap.Round)
                .SetDashed(true)
                .SetDashType(DashType.Rounded)
                .SetDashSnap(DashSnapping.Off)
                .SetDashSpace(DashSpace.FixedCount);
        }

        protected override void UpdateShape()
        {
            float scale = CoordinateConverter.Scale;
            m_SolidLine.SetThickness(ViewSettings.LineThickness * scale);
            m_IntermittentLine.SetThickness(ViewSettings.LineThickness * scale);
            var dir = Props.Directions.First();
            (m_SolidLine.Start, m_SolidLine.End) = GetLinePoints(dir, false);
            (m_IntermittentLine.Start, m_IntermittentLine.End) = GetLinePoints(dir, true);
            float dashSize = 4f * Vector3.Distance(
                m_IntermittentLine.Start, m_IntermittentLine.End) / scale;
            m_IntermittentLine.SetDashSize(dashSize);
        }

        private void ShiftOffsetsOfIntermittentLine()
        {
            const float maxOffset = 10f;
            float dOffset = GameTicker.DeltaTime * 3f;
            m_DashedOffset += dOffset;
            m_DashedOffset = MathUtils.ClampInverse(m_DashedOffset, 0, maxOffset);
            float offset = m_DashedOffset + 0.5f;
                m_IntermittentLine.DashOffset = offset;
        }
        
        private Tuple<Vector2, Vector2> GetLinePoints(
            V2Int _Direction,
            bool  _Intermittent)
        {
            const float intermAddict = .15f;
            const float sc1 = .5f;
            float sc2 = .5f - ViewSettings.LineThickness;
            Vector2 start, end;
            Vector2 left, right, down, up, zero;
            (left, right, down, up, zero) = (Vector2.left, Vector2.right, Vector2.down, Vector2.up, Vector2.zero);
            var diodeDir = RmazorUtils.GetMoveDirection(_Direction, EMazeOrientation.North);
            switch (diodeDir)
            {
                case EMazeMoveDirection.Up:
                    start = up * sc1 + left * sc2  + (_Intermittent ? down * intermAddict : zero);
                    end = up * sc1 + right * sc2 + (_Intermittent ? down * intermAddict : zero);
                    break;
                case EMazeMoveDirection.Right:
                    start = right * sc1 + down * sc2 + (_Intermittent ? left * intermAddict : zero);
                    end = right * sc1 + up * sc2 + (_Intermittent ? left * intermAddict : zero);
                    break;
                case EMazeMoveDirection.Down:
                    start = down * sc1 + left * sc2 + (_Intermittent ? up * intermAddict : zero);
                    end = down * sc1 + right * sc2 + (_Intermittent ? up * intermAddict : zero);
                    break;
                case EMazeMoveDirection.Left:
                    start = left * sc1 + down * sc2 + (_Intermittent ? right * intermAddict : zero);
                    end = left * sc1 + up * sc2 + (_Intermittent ? right * intermAddict : zero);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(diodeDir);
            }
            float scale = CoordinateConverter.Scale;
            start *= scale;
            end *= scale;
            return new Tuple<Vector2, Vector2>(start, end);
        }

        protected override void OnAppearStart(bool _Appear)
        {
            if (_Appear)
            {
                m_SolidLine.enabled = true;
                m_IntermittentLine.enabled = true;
            }
            base.OnAppearStart(_Appear);
        }

        protected override void OnAppearFinish(bool _Appear)
        {
            if (!_Appear)
            {
                m_SolidLine.enabled = false;
                m_IntermittentLine.enabled = false;
            }
            base.OnAppearFinish(_Appear);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var mainCol = ColorProvider.GetColor(ColorIds.MazeItem2);
            return new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new [] { m_SolidLine, m_IntermittentLine}, () => mainCol}
            };
        }

        #endregion
    }
}