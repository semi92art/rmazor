using Common;
using Common.Helpers;
using RMAZOR.Models;
using UnityEngine.Events;

namespace RMAZOR.Views.InputConfigurators
{
    public interface IViewInputController : IInit, IOnLevelStageChanged
    {
        IViewInputCommandsProceeder CommandsProceeder { get; }
        IViewInputTouchProceeder    TouchProceeder    { get; }
    }
    
    public class ViewInputController : InitBase, IViewInputController
    {
        public IViewInputCommandsProceeder CommandsProceeder { get; }
        public IViewInputTouchProceeder    TouchProceeder    { get; }

        public ViewInputController(
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewInputTouchProceeder _TouchProceeder)
        {
            CommandsProceeder = _CommandsProceeder;
            TouchProceeder = _TouchProceeder;
        }

        public virtual void Init()
        {
            CommandsProceeder.Init();
            TouchProceeder.Init();
            base.Init();
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            TouchProceeder.OnLevelStageChanged(_Args);
        }
    }
}