using Entities;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.UI;

namespace Games.RazorMaze.Views.Factories
{
    public interface IRotatingPossibilityIndicatorFactory
    {
        IRotatingPossibilityIndicator Create();
    }
    
    public class RotatingPossibilityIndicatorFactory : IRotatingPossibilityIndicatorFactory
    {
        private IContainersGetter ContainersGetter { get; }
        private IManagersGetter   Managers         { get; }

        public RotatingPossibilityIndicatorFactory(
            IContainersGetter _ContainersGetter,
            IManagersGetter _Managers)
        {
            ContainersGetter = _ContainersGetter;
            Managers = _Managers;
        }

        public IRotatingPossibilityIndicator Create()
        {
            var indicator = new RotatingPossibilityIndicator(ContainersGetter, Managers);
            indicator.Init();
            return indicator;
        }
    }
}