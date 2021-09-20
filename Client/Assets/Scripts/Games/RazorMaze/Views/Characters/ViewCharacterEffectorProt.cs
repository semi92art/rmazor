
namespace Games.RazorMaze.Views.Characters
{
    public class ViewCharacterEffectorProt : IViewCharacterEffector
    {
        public event NoArgsHandler Initialized;
        public void Init() { }
        public bool Activated { get; set; }
        public void OnRevivalOrDeath(bool _Alive) { }
        public void OnLevelStageChanged(LevelStageArgs _Args) { }
    }
}