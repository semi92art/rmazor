using System.Collections.Generic;
using System.Linq;
using Common.Enums;
using Common.Helpers;
using RMAZOR.Models;

namespace RMAZOR.Views.Common.Additional_Background
{
    public interface IViewMazeAdditionalBackgroundDrawerRmazor
        : IViewMazeAdditionalBackgroundDrawer { }
    
    public class ViewMazeAdditionalBackgroundDrawerRmazor 
        : InitBase,
          IViewMazeAdditionalBackgroundDrawer
    {
        #region inject

        private IModelGame                                        Model                { get; }
        private IViewMazeAdditionalBackgroundDrawerRmazorOnlyMask DrawerRmazorOnlyMask { get; }
        private IViewMazeAdditionalBackgroundDrawerRmazorFull     DrawerRmazorFull     { get; }

        public ViewMazeAdditionalBackgroundDrawerRmazor(
            IModelGame _Model,
            IViewMazeAdditionalBackgroundDrawerRmazorOnlyMask _DrawerRmazorOnlyMask, 
            IViewMazeAdditionalBackgroundDrawerRmazorFull     _DrawerRmazorFull)
        {
            Model = _Model;
            DrawerRmazorOnlyMask = _DrawerRmazorOnlyMask;
            DrawerRmazorFull     = _DrawerRmazorFull;
        }

        #endregion

        #region api

        public EAppearingState AppearingState => GetCurrentDrawer().AppearingState;

        public override void Init()
        {
            DrawerRmazorFull.Init();
            DrawerRmazorOnlyMask.Init();
            base.Init();
        }

        public void Appear(bool _Appear)
        {
            GetCurrentDrawer().Appear(_Appear);
        }

        public void Draw(List<PointsGroupArgs> _Groups, long _LevelIndex)
        {
            Disable();
            GetCurrentDrawer().Draw(_Groups, _LevelIndex);
        }

        public void Disable()
        {
            DrawerRmazorFull    .Disable();
            DrawerRmazorOnlyMask.Disable();
        }

        #endregion

        #region nonpublic methods

        private IViewMazeAdditionalBackgroundDrawer GetCurrentDrawer()
        {
            var proceedInfos = Model.GetAllProceedInfos();
            var rotTypes = RmazorUtils.GravityItemTypes;
            if (proceedInfos.Any(_Info => rotTypes.Contains(_Info.Type)))
                return DrawerRmazorFull;
            return DrawerRmazorOnlyMask;
        }

        #endregion
    }
}