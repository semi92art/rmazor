using RMAZOR.Models;
using RMAZOR.Views.CoordinateConverters;

namespace RMAZOR.Views.ContainerGetters
{
    public interface IContainersGetterRmazor : IContainersGetterRmazorBase { }
    
    public class ContainersGetterRmazor : ContainersGetterRmazorBase, IContainersGetterRmazor
    {
        private ICoordinateConverterRmazor CoordinateConverter { get; }

        public ContainersGetterRmazor(
            IModelGame               _Model,
            ICoordinateConverterRmazor _CoordinateConverter)
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