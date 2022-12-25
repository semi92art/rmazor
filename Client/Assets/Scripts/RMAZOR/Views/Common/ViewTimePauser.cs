using mazing.common.Runtime.Ticker;

namespace RMAZOR.Views.Common
{
    public interface IViewTimePauser
    {
        void PauseTimeInGame();
        void UnpauseTimeInGame();
        
        void PauseTimeInUi();
        void UnpauseTimeInUi();
    }
    
    public class ViewTimePauser : IViewTimePauser
    {
        private IViewGameTicker  ViewGameTicker  { get; }
        private IModelGameTicker ModelGameTicker { get; }
        private IUITicker        UiTicker        { get; }

        public ViewTimePauser(
            IViewGameTicker  _ViewGameTicker,
            IModelGameTicker _ModelGameTicker,
            IUITicker        _UITicker)
        {
            ViewGameTicker  = _ViewGameTicker;
            ModelGameTicker = _ModelGameTicker;
            UiTicker        = _UITicker;
        }

        public void PauseTimeInGame()
        {
            ViewGameTicker.Pause = true;
            ModelGameTicker.Pause = true;
        }

        public void UnpauseTimeInGame()
        {
            ViewGameTicker.Pause = false;
            ModelGameTicker.Pause = false;
        }

        public void PauseTimeInUi()
        {
            UiTicker.Pause = true;
        }

        public void UnpauseTimeInUi()
        {
            UiTicker.Pause = false;
        }
    }
}