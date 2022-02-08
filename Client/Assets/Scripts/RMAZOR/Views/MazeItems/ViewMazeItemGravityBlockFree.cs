using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Common;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemGravityBlockFree : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemGravityBlockFree : ViewMazeItemMovingBase, IViewMazeItemGravityBlockFree 
    {
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsBlockSlidingDown => 
            new AudioClipArgs("block_sliding_down", EAudioClipType.GameSound, 0.15f, true);

        #endregion
        
        #region shapes

        protected override string ObjectName => "Gravity Block Free";
        protected Rectangle m_Shape;
        
        #endregion
        
        #region inject
        
        public ViewMazeItemGravityBlockFree(
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
        
        public override Component[] Shapes => new Component[] {m_Shape};
        public override object Clone() => new ViewMazeItemGravityBlockFree(
            ViewSettings,
            Model,
            CoordinateConverter, 
            ContainersGetter, 
            GameTicker,
            Transitioner,
            Managers,
            ColorProvider,
            CommandsProceeder);

        public override void OnMoveStarted(MazeItemMoveEventArgs _Args)
        {
            Managers.AudioManager.PlayClip(AudioClipArgsBlockSlidingDown);
        }

        public override void OnMoving(MazeItemMoveEventArgs _Args)
        {
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            var pos = Vector2.Lerp(_Args.From, _Args.To, _Args.Progress);
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(pos));
        }

        public override void OnMoveFinished(MazeItemMoveEventArgs _Args)
        {
            base.OnMoveFinished(_Args);
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(_Args.To));
            Managers.AudioManager.StopClip(AudioClipArgsBlockSlidingDown);
        }

        #endregion
        
        #region nonpublic methods

        protected override void InitShape()
        {
            var sh = Object.AddComponentOnNewChild<Rectangle>("Block", out _);
            sh.Type = Rectangle.RectangleType.RoundedBorder;
            sh.Color = ColorProvider.GetColor(ColorIds.Main);
            sh.SortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type);
            m_Shape = sh;
        }

        protected override void UpdateShape()
        {
            base.UpdateShape();
            m_Shape.Width = m_Shape.Height = CoordinateConverter.Scale * 0.9f;
            m_Shape.Thickness = ViewSettings.LineWidth * CoordinateConverter.Scale;
            m_Shape.CornerRadius = ViewSettings.CornerRadius * CoordinateConverter.Scale;
        }

        protected override void InitWallBlockMovingPaths() { }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId == ColorIds.Main)
                m_Shape.Color = _Color;
        }
        
        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            return base.GetAppearSets(_Appear).Concat(new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {Shapes, () => ColorProvider.GetColor(ColorIds.Main)}
            }).ToDictionary(_Kvp => _Kvp.Key,
                _Kvp => _Kvp.Value);
        }

        #endregion
    }
}