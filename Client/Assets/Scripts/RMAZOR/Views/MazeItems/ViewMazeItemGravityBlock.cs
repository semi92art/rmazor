using System;
using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemGravityBlock : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemGravityBlock : ViewMazeItemGravityBlockFree, IViewMazeItemGravityBlock
    {
        #region shapes

        protected override string ObjectName => "Gravity Block";
        private Rectangle m_FilledShape;
        
        #endregion
        
        #region inject
        
        public ViewMazeItemGravityBlock(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IViewGameTicker _GameTicker,
            IViewBetweenLevelMazeTransitioner _Transitioner,
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
        
        public override Component[] Shapes => base.Shapes.Concat(new [] {m_FilledShape}).ToArray();
        
        public override object Clone() => new ViewMazeItemGravityBlock(
            ViewSettings,
            Model,
            CoordinateConverter, 
            ContainersGetter, 
            GameTicker,
            Transitioner,
            Managers,
            ColorProvider,
            CommandsProceeder);
        

        #endregion
        
        #region nonpublic methods

        protected override void InitShape()
        {
            base.InitShape();
            var sh = Object.AddComponentOnNewChild<Rectangle>("Joint", out _);
            sh.Type = Rectangle.RectangleType.RoundedSolid;
            sh.Color = ColorProvider.GetColor(ColorIds.Main);
            sh.SortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type);
            m_FilledShape = sh;
        }

        protected override void UpdateShape()
        {
            base.UpdateShape();
            m_FilledShape.Width = m_FilledShape.Height = CoordinateConverter.Scale * 0.9f;
            m_FilledShape.Thickness = ViewSettings.LineWidth * CoordinateConverter.Scale;
            m_FilledShape.CornerRadius = ViewSettings.CornerRadius * CoordinateConverter.Scale;
        }

        protected override void InitWallBlockMovingPaths()
        {
            InitWallBlockMovingPathsCore();
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.Main)
                m_FilledShape.Color = _Color.SetA(0.3f);
            base.OnColorChanged(_ColorId, _Color);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            return base.GetAppearSets(_Appear).Concat(new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new [] {m_FilledShape}, () => ColorProvider.GetColor(ColorIds.Main).SetA(0.3f)}
            }).ToDictionary(
                _Kvp => _Kvp.Key,
                _Kvp => _Kvp.Value);
        }

        #endregion
    }
}