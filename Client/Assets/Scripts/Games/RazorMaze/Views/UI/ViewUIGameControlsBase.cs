using System.Linq;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.InputConfigurators;
using UnityEngine.Events;

namespace Games.RazorMaze.Views.UI
{
    public abstract class ViewUIGameControlsBase : IViewUIGameControls
    {
        protected IViewInputConfigurator InputConfigurator { get; }

        protected ViewUIGameControlsBase(IViewInputConfigurator _InputConfigurator)
        {
            InputConfigurator = _InputConfigurator;
        }

        public event UnityAction Initialized;
        public void Init()
        {
            Initialized?.Invoke();
        }

        public abstract void OnLevelStageChanged(LevelStageArgs _Args);

        public virtual void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args)
        {
            var type = _Args.Info.Type;
            if (RazorMazeUtils.GravityItemTypes().Contains(type))
            {
                InputConfigurator.LockCommands(new []
                {
                    InputCommands.MoveLeft,
                    InputCommands.MoveRight,
                    InputCommands.MoveDown,
                    InputCommands.MoveUp,
                    InputCommands.RotateClockwise,
                    InputCommands.RotateCounterClockwise
                });
            }
        }

        public virtual void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args)
        {
            var type = _Args.Info.Type;
            if (RazorMazeUtils.GravityItemTypes().Contains(type))
            {
                InputConfigurator.UnlockCommands(new []
                {
                    InputCommands.MoveLeft,
                    InputCommands.MoveRight,
                    InputCommands.MoveDown,
                    InputCommands.MoveUp,
                    InputCommands.RotateClockwise,
                    InputCommands.RotateCounterClockwise
                });
            }
        }
    }
}