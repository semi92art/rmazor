using Games.RazorMaze.Models.ItemProceeders;
using UnityEngine.Events;

namespace Games.RazorMaze.Views.UI
{
    public class ViewUIGameControlsProt : IViewUIGameControls
    {
        public IViewUIPrompts Prompts { get; }
        public event UnityAction Initialized;
        public void Init() { Initialized?.Invoke();}
        public void OnLevelStageChanged(LevelStageArgs _Args) { }
        public void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args) { }
        public void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args) { }
    }
}