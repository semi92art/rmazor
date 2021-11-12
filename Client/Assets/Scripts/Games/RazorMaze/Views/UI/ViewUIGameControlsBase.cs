using System.Linq;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.InputConfigurators;
using UnityEngine.Events;

namespace Games.RazorMaze.Views.UI
{
    public abstract class ViewUIGameControlsBase : IViewUIGameControls
    {
        protected IViewInputCommandsProceeder CommandsProceeder { get; }

        protected ViewUIGameControlsBase(IViewInputCommandsProceeder _CommandsProceeder)
        {
            CommandsProceeder = _CommandsProceeder;
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
                CommandsProceeder.LockCommands(new []
                {
                    EInputCommand.MoveLeft,
                    EInputCommand.MoveRight,
                    EInputCommand.MoveDown,
                    EInputCommand.MoveUp,
                    EInputCommand.RotateClockwise,
                    EInputCommand.RotateCounterClockwise
                });
            }
        }

        public virtual void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args)
        {
            var type = _Args.Info.Type;
            if (RazorMazeUtils.GravityItemTypes().Contains(type))
            {
                CommandsProceeder.UnlockCommands(new []
                {
                    EInputCommand.MoveLeft,
                    EInputCommand.MoveRight,
                    EInputCommand.MoveDown,
                    EInputCommand.MoveUp,
                    EInputCommand.RotateClockwise,
                    EInputCommand.RotateCounterClockwise
                });
            }
        }
    }
}