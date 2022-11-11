using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;

namespace RMAZOR.Views.ContainerGetters
{
    public interface IContainersGetterRmazorInEditor : IContainersGetterRmazorBase { }
    
    public class ContainersGetterRmazorInEditor : ContainersGetterRmazorBase, IContainersGetterRmazorInEditor
    {
        private ICoordinateConverterInEditor CoordinateConverter { get; } 
        
        public ContainersGetterRmazorInEditor(
            IModelGame                             _Model,
            ICoordinateConverterInEditor _CoordinateConverter)
            : base(_Model, _CoordinateConverter)
        {
            CoordinateConverter = _CoordinateConverter;
        }

        protected override void UpdateCoordinateConverterState()
        {
            CoordinateConverter.SetMazeSize(Model.Data.Info.Size);
        }
    }
}