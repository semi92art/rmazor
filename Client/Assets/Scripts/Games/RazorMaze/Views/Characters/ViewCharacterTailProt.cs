using Games.RazorMaze.Models;

namespace Games.RazorMaze.Views.Characters
{
    public class ViewCharacterTailProt : IViewCharacterTail
    {
        public event NoArgsHandler Initialized;
        public bool Activated { get; set; }
        public void Init() { }
        public void ShowTail(CharacterMovingEventArgs _Args) { }
        public void HideTail(CharacterMovingEventArgs _Args) { }
    }
}