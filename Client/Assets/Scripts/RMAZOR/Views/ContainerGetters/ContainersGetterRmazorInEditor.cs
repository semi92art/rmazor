using RMAZOR.Models;
using RMAZOR.Views.CoordinateConverters;

namespace RMAZOR.Views.ContainerGetters
{
    public interface IContainersGetterRmazorInEditor : IContainersGetterRmazorBase { }
    
    public class ContainersGetterRmazorInEditor : ContainersGetterRmazorBase, IContainersGetterRmazorInEditor
    {
        private ICoordinateConverterRmazorInEditor CoordinateConverter { get; } 
        
        public ContainersGetterRmazorInEditor(
            IModelGame                             _Model,
            ICoordinateConverterRmazorInEditor _CoordinateConverter)
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