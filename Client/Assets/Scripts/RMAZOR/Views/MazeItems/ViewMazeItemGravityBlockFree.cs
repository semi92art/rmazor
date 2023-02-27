using System;
using System.Collections.Generic;
using Common.Extensions;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemGravityBlockFree : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemGravityBlockFree 
        : ViewMazeItemMovingBase,
          IViewMazeItemGravityBlockFree 
    {
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsBlockSlidingDown => 
            new AudioClipArgs("block_sliding_down", EAudioClipType.GameSound, 0.15f, true);

        #endregion
        
        #region shapes

        protected override string ObjectName => "Gravity Block Free";
        private Rectangle m_Border;
        private Rectangle m_Shape;
        
        #endregion
        
        #region inject

        protected ViewMazeItemGravityBlockFree(
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
        
        public override Component[] Renderers => new Component[] {m_Border, m_Shape};
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

        protected override int LinesAndJointsColorId => GetMazeItemBlockColorId();

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
            m_Border = Object.AddComponentOnNewChild<Rectangle>("Block", out _)
                .SetSortingOrder(GetSortingOrder())
                .SetType(Rectangle.RectangleType.RoundedBorder)
                .SetColor(ColorProvider.GetColor(GetMazeItemBlockColorId()));
            m_Shape = Object.AddComponentOnNewChild<Rectangle>("Joint", out _)
                .SetSortingOrder(GetSortingOrder())
                .SetType(Rectangle.RectangleType.RoundedSolid)
                .SetColor(ColorProvider.GetColor(GetMazeItemBlockColorId()));
        }

        protected override void UpdateShape()
        {
            base.UpdateShape();
            float scale = CoordinateConverter.Scale;
            m_Border.SetSize(scale)
                .SetThickness(ViewSettings.LineThickness * scale)
                .SetCornerRadius(ViewSettings.LineThickness * scale);
            m_Shape.SetSize(scale)
                .SetThickness(ViewSettings.LineThickness * scale)
                .SetCornerRadius(ViewSettings.LineThickness * scale);
        }

        protected override void InitWallBlockMovingPaths() { }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId != GetMazeItemBlockColorId()) 
                return;
            m_Border.Color = _Color;
            m_Shape.Color = _Color.SetA(0.3f);
        }
        
        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var sets = base.GetAppearSets(_Appear);
            sets.Add(new [] {m_Shape},  () => ColorProvider.GetColor(GetMazeItemBlockColorId()).SetA(0.3f));
            sets.Add(new [] {m_Border}, () => ColorProvider.GetColor(GetMazeItemBlockColorId()));
            return sets;
        }

        #endregion
    }
}