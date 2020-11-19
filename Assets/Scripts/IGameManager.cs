
using UI;

public interface IGameManager
{
    ILevelController LevelController { get; }
    IGameMenuUi GameMenuUi { get; }
    void Init(int _Level);
}
