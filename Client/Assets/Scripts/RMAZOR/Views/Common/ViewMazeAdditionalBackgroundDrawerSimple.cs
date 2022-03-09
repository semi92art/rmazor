using Common.Helpers;
using Common.Managers;
using Common.Providers;
using RMAZOR.Models;
using RMAZOR.Views.Helpers;

namespace RMAZOR.Views.Common
{
    public class ViewMazeAdditionalBackgroundDrawerSimple 
        : ViewMazeAdditionalBackgroundDrawerBase
    {
        public ViewMazeAdditionalBackgroundDrawerSimple(
            ViewSettings                  _ViewSettings,
            IModelGame                    _Model,
            IColorProvider                _ColorProvider,
            IContainersGetter             _ContainersGetter,
            IMazeCoordinateConverter      _CoordinateConverter,
            IPrefabSetManager             _PrefabSetManager,
            IViewBetweenLevelTransitioner _Transitioner) 
            : base(
                _ViewSettings,
                _Model,
                _ColorProvider,
                _ContainersGetter,
                _CoordinateConverter,
                _PrefabSetManager,
                _Transitioner) { }

        protected override void DrawHolesForGroup(PointsGroupArgs _Group)
        {
            // ничего не делаем
        }
    }
}