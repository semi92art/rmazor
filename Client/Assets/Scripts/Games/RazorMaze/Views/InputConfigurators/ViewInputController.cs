using UnityEngine.Events;

namespace Games.RazorMaze.Views.InputConfigurators
{
    public interface IViewInputController : IInit, IOnLevelStageChanged { }
    
    public class ViewInputController : IViewInputController
    {
        protected IViewInputCommandsProceeder CommandsProceeder { get; }
        protected IViewInputTouchProceeder    TouchProceeder    { get; }

        public ViewInputController(
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewInputTouchProceeder _TouchProceeder)
        {
            CommandsProceeder = _CommandsProceeder;
            TouchProceeder = _TouchProceeder;
        }

        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;
        
        public virtual void Init()
        {
            TouchProceeder.Init();
            Initialize?.Invoke();
            Initialized = true;
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            TouchProceeder.OnLevelStageChanged(_Args);
        }
    }
}