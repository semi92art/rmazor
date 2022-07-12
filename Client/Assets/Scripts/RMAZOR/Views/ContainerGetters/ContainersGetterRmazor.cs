using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;

namespace RMAZOR.Views.ContainerGetters
{
    public interface IContainersGetterRmazor : IContainersGetterRmazorBase { }
    
    public class ContainersGetterRmazor : ContainersGetterRmazorBase, IContainersGetterRmazor
    {
        private ICoordinateConverter CoordinateConverter { get; }

        public ContainersGetterRmazor(
            IModelGame               _Model,
            ICoordinateConverter _CoordinateConverter)
            : base(_Model, _CoordinateConverter)
        {
            CoordinateConverter = _CoordinateConverter;
        }

        protected override void UpdateCoordinateConverterState()
        {
            CoordinateConverter.SetMazeInfo(Model.Data.Info);
        }
    }
}