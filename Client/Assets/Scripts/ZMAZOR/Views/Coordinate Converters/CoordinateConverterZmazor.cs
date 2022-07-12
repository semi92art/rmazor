using Common.CameraProviders;
using RMAZOR;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Coordinate_Converters;
using UnityEngine;

namespace ZMAZOR.Views.Coordinate_Converters
{
    public class CoordinateConverterZmazor : CoordinateConverterRmazor
    {
        #region constants

        private const int MaxWidth  = 20;
        private const int MaxHeight = 20;

        #endregion
        
        #region inject

        protected CoordinateConverterZmazor(
            ViewSettings    _ViewSettings, 
            ICameraProvider _CameraProvider) 
            : base(_ViewSettings, _CameraProvider) { }

        #endregion

        #region nonpublic methods

        protected override void SetCorrectMazeSize(MazeInfo _Info)
        {
            base.SetCorrectMazeSize(_Info);
            if (MazeSizeForScale.x > MaxWidth)  MazeSizeForScale.x = MaxWidth / 1.5f;
            if (MazeSizeForScale.y > MaxHeight) MazeSizeForScale.y = MaxHeight / 1.5f;
        }

        protected override void SetCenterPoint()
        {
            Center = Vector2.zero;
        }

        #endregion
    }
}