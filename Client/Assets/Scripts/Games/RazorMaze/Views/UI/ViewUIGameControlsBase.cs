using System.Linq;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.InputConfigurators;
using UnityEngine.Events;

namespace Games.RazorMaze.Views.UI
{
    public interface IViewUIGameControls : IInit, IOnLevelStageChanged
    {
        void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args);
        void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args);

    }
    
    public abstract class ViewUIGameControlsBase : IViewUIGameControls
    {
        protected IViewInputCommandsProceeder CommandsProceeder { get; }

        protected ViewUIGameControlsBase(IViewInputCommandsProceeder _CommandsProceeder)
        {
            CommandsProceeder = _CommandsProceeder;
        }

        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;
        public virtual void Init()
        {
            Initialize?.Invoke();
            Initialized = true;
        }

        public abstract void OnLevelStageChanged(LevelStageArgs _Args);

        public virtual void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args)
        {
            var type = _Args.Info.Type;
            if (RazorMazeUtils.GravityItemTypes().Contains(type))
            {
                var commands = RazorMazeUtils.GetMoveCommands()
                    .Concat(RazorMazeUtils.GetRotateCommands());
                CommandsProceeder.LockCommands(commands);
            }
        }

        public virtual void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args)
        {
            var type = _Args.Info.Type;
            if (RazorMazeUtils.GravityItemTypes().Contains(type))
            {
                var commands = RazorMazeUtils.GetMoveCommands()
                    .Concat(RazorMazeUtils.GetRotateCommands());
                CommandsProceeder.UnlockCommands(commands);
            }
        }
    }
}