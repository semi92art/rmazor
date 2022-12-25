using mazing.common.Runtime.CameraProviders;
using RMAZOR.Models.MazeInfos;
using UnityEngine;

namespace RMAZOR.Views.Coordinate_Converters
{
    public interface ICoordinateConverterForBigLevels 
        : ICoordinateConverter { }
    
    public class CoordinateConverterForBigLevels 
        : CoordinateConverterForSmallLevels,
          ICoordinateConverterForBigLevels
    {
        #region constants

        private const int MaxWidth  = 20;
        private const int MaxHeight = 20;

        #endregion
        
        #region inject

        protected CoordinateConverterForBigLevels(
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
        
        protected override void SetScale()
        {
            ScaleValue = 3f;
        }

        #endregion
    }
}