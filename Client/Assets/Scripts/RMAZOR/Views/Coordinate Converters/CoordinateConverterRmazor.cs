using System.Linq;
using Common.CameraProviders;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using UnityEngine;

namespace RMAZOR.Views.Coordinate_Converters
{
    public interface ICoordinateConverter : ICoordinateConverterRmazorBase
    {
        void SetMazeInfo(MazeInfo _Info);
    }
    
    public class CoordinateConverterRmazor : CoordinateConverterBase, ICoordinateConverter
    {
        #region constants

        private const int MinWidth  = 11;
        private const int MinHeight = 11;

        #endregion
        
        #region nonpublic members

        private bool m_CutLeft, m_CutRight, m_CutBottom, m_CutTop;
        
        #endregion

        #region inject

        protected CoordinateConverterRmazor(
            ViewSettings    _ViewSettings,
            ICameraProvider _CameraProvider)
            : base(_ViewSettings, _CameraProvider) { }

        #endregion

        #region api
        
        public void SetMazeInfo(MazeInfo _Info)
        {
            SetCorrectionInfo(_Info);
            SetCorrectMazeSize(_Info);
            MazeDataWasSet = true;
            CheckForErrors();
            SetScale();
        }

        #endregion

        #region nonpublic methods

        protected override Vector2 ToLocalMazePosition(Vector2 _Point)
        {
            CheckForErrors();
            var pos = ScaleValue * (_Point - MazeSizeForPositioning * 0.5f);
            if (m_CutLeft)
                pos += Vector2.left * (ScaleValue * 1f);
            if (m_CutBottom)
                pos += Vector2.down * (ScaleValue * 1f);
            return pos;
        }
        
        protected override void SetCenterPoint()
        {
            var bounds = GraphicUtils.GetVisibleBounds(CameraProvider?.Camera);
            float centerX = ((bounds.min.x + LeftOffset) + (bounds.max.x - RightOffset)) * 0.5f;
            float centerY = ((bounds.min.y + BottomOffset) + (bounds.max.y - TopOffset)) * 0.5f;
            Center = new Vector2(centerX, centerY);
        }

        private void SetCorrectionInfo(MazeInfo _Info)
        {
            bool? hasTrapsReact = null;
            bool hasOtherBlocks = false;
            int k = 0;
            while (!hasOtherBlocks && !hasTrapsReact.HasValue && k < _Info.Size.X)
            {
                foreach (var mazeItem in _Info.MazeItems.Where(_Item => _Item.Position.X == k))
                    IsTrapReactOrOtherBlock(mazeItem, EMazeMoveDirection.Left, ref hasTrapsReact, ref hasOtherBlocks);
                k++;
            }
            m_CutLeft = hasTrapsReact.HasValue && hasTrapsReact.Value && !hasOtherBlocks && k < _Info.Size.X;
            k = 0;
            hasTrapsReact = null;
            hasOtherBlocks = false;
            while (!hasOtherBlocks && !hasTrapsReact.HasValue && k < _Info.Size.Y)
            {
                foreach (var mazeItem in _Info.MazeItems.Where(_Item => _Item.Position.Y == k))
                    IsTrapReactOrOtherBlock(mazeItem, EMazeMoveDirection.Up, ref hasTrapsReact, ref hasOtherBlocks);
                k++;
            }
            m_CutBottom = hasTrapsReact.HasValue && hasTrapsReact.Value  && !hasOtherBlocks && k < _Info.Size.Y;
            k = _Info.Size.X;
            hasTrapsReact = null;
            hasOtherBlocks = false;
            while (!hasOtherBlocks && !hasTrapsReact.HasValue && k >= 0)
            {
                k--;
                foreach (var mazeItem in _Info.MazeItems.Where(_Item => _Item.Position.X == k))
                    IsTrapReactOrOtherBlock(mazeItem, EMazeMoveDirection.Right, ref hasTrapsReact, ref hasOtherBlocks);
            }
            m_CutRight = hasTrapsReact.HasValue && hasTrapsReact.Value  && !hasOtherBlocks && k >= 0;
            k = _Info.Size.Y;
            hasTrapsReact = null;
            hasOtherBlocks = false;
            while (!hasOtherBlocks && !hasTrapsReact.HasValue && k >= 0)
            {
                k--;
                foreach (var mazeItem in _Info.MazeItems.Where(_Item => _Item.Position.Y == k))
                    IsTrapReactOrOtherBlock(mazeItem, EMazeMoveDirection.Down, ref hasTrapsReact, ref hasOtherBlocks);
            }
            m_CutTop = hasTrapsReact.HasValue && hasTrapsReact.Value  && !hasOtherBlocks && k >= 0;
        }

        protected virtual void SetCorrectMazeSize(MazeInfo _Info)
        {
            MazeSizeForPositioning = MazeSizeForScale = _Info.Size;
            if (m_CutLeft && MazeSizeForScale.x > MinWidth)
            {
                MazeSizeForPositioning.x -= 1f;
                MazeSizeForScale.x -= 1f;
            }
            if (m_CutRight && MazeSizeForScale.x > MinWidth)
            {
                MazeSizeForPositioning.x -= 1f;
                MazeSizeForScale.x -= 1f;
            }
            if (m_CutBottom && MazeSizeForScale.y > MinHeight)
            {
                MazeSizeForPositioning.y -= 1f;
                MazeSizeForScale.y -= 1f;
            }
            if (m_CutTop && MazeSizeForScale.y > MinHeight)
            {
                MazeSizeForPositioning.y -= 1f;
                MazeSizeForScale.y -= 1f;
            }
        }

        private static void IsTrapReactOrOtherBlock(
            MazeItem _Item,
            EMazeMoveDirection _Direction,
            ref bool? _IsTrapsReact,
            ref bool _IsOtherBlock)
        {
            if (_Item.Type == EMazeItemType.TrapReact)
            {
                var dir = _Item.Directions.First();
                _IsTrapsReact = _Direction == RmazorUtils.GetMoveDirection(dir, MazeOrientation.North);
            }
            if (_Item.Type != EMazeItemType.Block && _Item.Type != EMazeItemType.TrapReact)
                _IsOtherBlock = true;
        }
        
        #endregion
    }
}