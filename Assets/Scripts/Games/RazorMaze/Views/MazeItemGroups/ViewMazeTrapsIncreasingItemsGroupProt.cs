using System.Collections;
using System.Linq;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.MazeCommon;
using Games.RazorMaze.Views.MazeItems;
using Utils;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeTrapsIncreasingItemsGroupProt : IViewMazeTrapsIncreasingItemsGroup
    {
        #region inject
        
        private IViewMazeCommon MazeCommon { get; }

        public ViewMazeTrapsIncreasingItemsGroupProt(IViewMazeCommon _MazeCommon)
        {
            MazeCommon = _MazeCommon;
        }

        #endregion
        
        public void Init() { }
        
        
        public void OnMazeTrapIncreasingStageChanged(MazeItemTrapIncreasingEventArgs _Args)
        {
            var item = MazeCommon.MazeItems.First(_Item => _Item.Equal(_Args.Item));
            HandleItemOnStage(_Args, item);
        }

        private void HandleItemOnStage(MazeItemTrapIncreasingEventArgs _Args, IViewMazeItem _Item)
        {
            switch (_Args.Stage)
            {
                case TrapsIncreasingProceeder.StageIdle: Coroutines.Run(HandleItemIdle(_Item)); break;
                case TrapsIncreasingProceeder.StageIncreased: Coroutines.Run(HandleItemIncreased(_Item)); break;
            }
        }

        private static IEnumerator HandleItemIdle(IViewMazeItem _Item)
        {
            Dbg.Log("HandleItemIdle");
            yield return Coroutines.Lerp(
                1f,
                3f,
                0.5f,
                _Item.SetLocalScale,
                GameTimeProvider.Instance
            );
        }
        
        private static IEnumerator HandleItemIncreased(IViewMazeItem _Item)
        {
            Dbg.Log("HandleItemIncreased");
            yield return Coroutines.Lerp(
                3f,
                1f,
                0.5f,
                _Item.SetLocalScale,
                GameTimeProvider.Instance
            );
        }
    }
}