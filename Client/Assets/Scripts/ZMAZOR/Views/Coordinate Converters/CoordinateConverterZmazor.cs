using Common.CameraProviders;
using RMAZOR;
using RMAZOR.Views.Coordinate_Converters;

namespace ZMAZOR.Views.Coordinate_Converters
{
    public class CoordinateConverterZmazor : CoordinateConverterForBigLevels
    {
        protected CoordinateConverterZmazor(
            ViewSettings _ViewSettings, 
            ICameraProvider _CameraProvider) 
            : base(_ViewSettings, _CameraProvider) { }
    }
}