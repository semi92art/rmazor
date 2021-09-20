using SpawnPools;

namespace Games.RazorMaze.Views.Characters
{
    public interface IViewCharacterEffector : IInit, IActivated, IOnRevivalOrDeath, IOnLevelStageChanged { }
}