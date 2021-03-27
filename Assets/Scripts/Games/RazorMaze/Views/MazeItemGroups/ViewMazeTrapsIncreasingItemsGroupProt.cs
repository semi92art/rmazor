using System.Collections;
using System.Linq;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.MazeCommon;
using Games.RazorMaze.Views.MazeItems;
using Utils;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeTrapsIncreasingItemsGroupProt : IViewMazeTrapsIncreasingItemsGroup
    {
        #region inject
        
        private IViewMazeCommon MazeCommon { get; }
        private ICoordinateConverter Converter { get; }

        public ViewMazeTrapsIncreasingItemsGroupProt(IViewMazeCommon _MazeCommon, ICoordinateConverter _Converter)
        {
            MazeCommon = _MazeCommon;
            Converter = _Converter;
        }

        #endregion
        
        public void Init()
        {
            
        }
        
        
        public void OnMazeTrapIncreasingStageChanged(MazeItemTrapIncreasingEventArgs _Args)
        {
            var item = MazeCommon.MazeItems.First(_Item => _Item.Equal(_Args.Item));
            HandleItemOnStage(_Args, item);
        }

        private void HandleItemOnStage(MazeItemTrapIncreasingEventArgs _Args, IViewMazeItem _Item)
        {
            switch (_Args.Stage)
            {
                case MazeTrapsIncreasingProceeder.StageIdle: Coroutines.Run(HandleItemIdle(_Item)); break;
                case MazeTrapsIncreasingProceeder.StageIncreased: Coroutines.Run(HandleItemIncreased(_Item)); break;
            }
        }

        private IEnumerator HandleItemIdle(IViewMazeItem _Item)
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
        
        private IEnumerator HandleItemIncreased(IViewMazeItem _Item)
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