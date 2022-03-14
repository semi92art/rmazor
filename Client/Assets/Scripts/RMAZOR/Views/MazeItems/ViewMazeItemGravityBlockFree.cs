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
        private Rectangle m_Shape;
        
        #endregion
        
        #region inject

        public ViewMazeItemGravityBlockFree(
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
            m_Shape = Object.AddComponentOnNewChild<Rectangle>("Block", out _)
                .SetSortingOrder(GetSortingOrder())
                .SetType(Rectangle.RectangleType.RoundedBorder)
                .SetColor(ColorProvider.GetColor(ColorIds.Main));
        }

        protected override void UpdateShape()
        {
            base.UpdateShape();
            float scale = CoordinateConverter.Scale;
            m_Shape.SetSize(scale * 0.9f)
                .SetThickness(ViewSettings.LineWidth * scale)
                .SetCornerRadius(ViewSettings.CornerRadius * scale);
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
            var sets = base.GetAppearSets(_Appear);
            sets.Add(Shapes, () => ColorProvider.GetColor(ColorIds.Main));
            return sets;
        }

        #endregion
    }
}