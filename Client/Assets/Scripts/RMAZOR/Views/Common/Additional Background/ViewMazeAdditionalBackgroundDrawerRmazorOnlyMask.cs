using System.Collections.Generic;
using Common.Helpers;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using RMAZOR.Views.Coordinate_Converters;

namespace RMAZOR.Views.Common.Additional_Background
{
    public interface IViewMazeAdditionalBackgroundDrawerRmazorOnlyMask 
        : IViewMazeAdditionalBackgroundDrawer { }
    
    public class ViewMazeAdditionalBackgroundDrawerRmazorOnlyMask
        : ViewMazeAdditionalBackgroundDrawerRmazorBase, 
          IViewMazeAdditionalBackgroundDrawerRmazorOnlyMask
    {
        public ViewMazeAdditionalBackgroundDrawerRmazorOnlyMask(
            ViewSettings                _ViewSettings,
            ICoordinateConverter        _CoordinateConverter,
            IContainersGetter           _ContainersGetter,
            IColorProvider              _ColorProvider,
            IRendererAppearTransitioner _Transitioner) 
            : base(
                _ViewSettings,
                _CoordinateConverter,
                _ContainersGetter,
                _ColorProvider,
                _Transitioner) { }

        public override void Appear(bool _Appear) { }
        
        public override void Draw(List<PointsGroupArgs> _Groups, long _LevelIndex)
        {
            TextureRendererMasksPool.DeactivateAll();
            foreach (var group in _Groups)
            {
                DrawMaskForGroup(group);
                DrawNetLines(group);
            }
        }
    }
}