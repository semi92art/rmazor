
public interface ILevelController
{
    int Level { get; set; }
    event LevelHandler OnLevelStarted;
    event LevelHandler OnLevelFinished;
    void StartLevel();
    void FinishLevel();
}
