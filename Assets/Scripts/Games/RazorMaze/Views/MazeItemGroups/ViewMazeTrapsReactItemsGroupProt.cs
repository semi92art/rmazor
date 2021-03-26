using System.Collections;
using System.Linq;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.MazeCommon;
using Games.RazorMaze.Views.MazeItems;
using Utils;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeTrapsReactItemsGroupProt : IViewMazeTrapsReactItemsGroup
    {

        #region inject
        
        private IViewMazeCommon MazeCommon { get; }
        private ICoordinateConverter Converter { get; }

        public ViewMazeTrapsReactItemsGroupProt(IViewMazeCommon _MazeCommon, ICoordinateConverter _Converter)
        {
            MazeCommon = _MazeCommon;
            Converter = _Converter;
        }

        #endregion
        
        public void Init()
        {
            
        }
        
        
        public void OnMazeTrapReactStageChanged(MazeItemTrapReactEventArgs _Args)
        {
            var item = MazeCommon.MazeItems.First(_Item => _Item.Equal(_Args.Item));
            HandleItemOnStage(_Args, item);
        }

        private void HandleItemOnStage(MazeItemTrapReactEventArgs _Args, IViewMazeItem _Item)
        {
            IEnumerator coroutine = null;
            switch (_Args.Stage)
            {
                case MazeTrapsReactProceeder.StageIdle: break;
                case MazeTrapsReactProceeder.StagePreReact:   coroutine = HandleItemOnPreReact(_Item); break;
                case MazeTrapsReactProceeder.StageReact:      coroutine = HandleItemOnReact(_Item); break;
                case MazeTrapsReactProceeder.StageAfterReact: coroutine = HandleItemOnAfterReact(_Item); break;
            }

            if (coroutine != null)
                Coroutines.Run(coroutine);
        }

        private IEnumerator HandleItemOnPreReact(IViewMazeItem _Item)
        {
            var props = _Item.Props;
            var from = Converter.ToLocalMazeItemPosition(props.Position);
            var toRaw = Converter.ToLocalMazeItemPosition(props.Position + props.Directions[0]);
            var dir = (toRaw - from).normalized;
            var to = from + 0.2f * dir;
            var delta = to - from;
            
            yield return Coroutines.Lerp(
                0f,
                1f,
                0.1f,
                _Progress =>
                {
                    var pos = from + delta * _Progress;
                    _Item.SetLocalPosition(pos);
                },
                GameTimeProvider.Instance
            );
        }
        
        private IEnumerator HandleItemOnReact(IViewMazeItem _Item)
        {
            var props = _Item.Props;
            var fromRaw = Converter.ToLocalMazeItemPosition(props.Position);
            var to = Converter.ToLocalMazeItemPosition(props.Position + props.Directions[0]);
            var dir = (to - fromRaw).normalized;
            var from = fromRaw + 0.2f * dir;
            var delta = to - from;
            
            yield return Coroutines.Lerp(
                0f,
                1f,
                0.1f,
                _Progress =>
                {
                    var pos = from + delta * _Progress;
                    _Item.SetLocalPosition(pos);
                },
                GameTimeProvider.Instance
            );
        }
        
        private IEnumerator HandleItemOnAfterReact(IViewMazeItem _Item)
        {
            var props = _Item.Props;
            var to = Converter.ToLocalMazeItemPosition(props.Position);
            var from = Converter.ToLocalMazeItemPosition(props.Position + props.Directions[0]);
            var delta = to - from;
            
            yield return Coroutines.Lerp(
                0f,
                1f,
                0.1f,
                _Progress =>
                {
                    var pos = from + delta * _Progress;
                    _Item.SetLocalPosition(pos);
                },
                GameTimeProvider.Instance
            );
        }
    }
}