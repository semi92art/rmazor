using System.Collections.Generic;
using Common.Helpers;
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
            ViewSettings         _ViewSettings,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter    _ContainersGetter) 
            : base(
                _ViewSettings,
                _CoordinateConverter,
                _ContainersGetter) { }

        public override void Appear(bool _Appear) { }

        public override void Init()
        {
            InitMasks();
            base.Init();
        }

        public override void Draw(List<PointsGroupArgs> _Groups, long _LevelIndex)
        {
            TextureRendererMasks.DeactivateAll();
            foreach (var group in _Groups)
                DrawMaskForGroup(group);
        }
    }
}