using System.Linq;
using Common;
using Common.Extensions;
using Common.Helpers;
using mazing.common.Runtime;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
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
    
    public abstract class ViewUIGameControlsBase : InitBase, IViewUIGameControls
    {
        protected IModelGame                  Model             { get; }
        protected IViewInputCommandsProceeder CommandsProceeder { get; }

        protected ViewUIGameControlsBase(IModelGame _Model, IViewInputCommandsProceeder _CommandsProceeder)
        {
            Model = _Model;
            CommandsProceeder = _CommandsProceeder;
        }

        public abstract void OnLevelStageChanged(LevelStageArgs _Args);

        public virtual void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args)
        {
            var type = _Args.Info.Type;
            if (!RmazorUtils.GravityItemTypes.ContainsAlt(type)) 
                return;
            CommandsProceeder.LockCommands(
                RmazorUtils.MoveAndRotateCommands, 
                nameof(IViewUIGameControls));
        }

        public virtual void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args)
        {
            var type = _Args.Info.Type;
            if (!RmazorUtils.GravityItemTypes.ContainsAlt(type)) 
                return;
            if (Model.ModelItemsProceedersSet.GravityItemsProceeder.ProceedInfos
                .Any(_I => _I.ProceedingStage == ModelCommonData.GravityItemStageDrop))
            {
                return;
            }
            CommandsProceeder.UnlockCommands(
                RmazorUtils.MoveAndRotateCommands, 
                nameof(IViewUIGameControls));
        }
    }
}