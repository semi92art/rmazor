using System.Linq;
using Common;
using Common.Extensions;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.InputConfigurators;
using UnityEngine.Events;

namespace RMAZOR.Views.UI
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
            if (RazorMazeUtils.GravityItemTypes().ContainsAlt(type))
            {
                Dbg.Log(nameof(OnMazeItemMoveStarted) + " " + _Args.Info.Type);
                var commands = RazorMazeUtils.GetMoveCommands()
                    .Concat(RazorMazeUtils.GetRotateCommands());
                CommandsProceeder.LockCommands(commands, nameof(IViewUIGameControls));
            }
        }

        public virtual void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args)
        {
            var type = _Args.Info.Type;
            if (RazorMazeUtils.GravityItemTypes().ContainsAlt(type))
            {
                Dbg.Log(nameof(OnMazeItemMoveStarted) + " " + _Args.Info.Type);
                var commands = RazorMazeUtils.GetMoveCommands()
                    .Concat(RazorMazeUtils.GetRotateCommands());
                CommandsProceeder.UnlockCommands(commands, nameof(IViewUIGameControls));
            }
        }
    }
}