using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.Views.MazeItems
{
    public class ViewMazeItemPathWithBorders : ViewMazeItemPath
    {
        public ViewMazeItemPathWithBorders(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            IMazeCoordinateConverter    _CoordinateConverter,
            IContainersGetter           _ContainersGetter,
            IViewGameTicker             _GameTicker,
            IViewAppearTransitioner     _Transitioner,
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
        
        
    }
}