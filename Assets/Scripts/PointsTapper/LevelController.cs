
namespace PointsTapper
{
    public class LevelController : ILevelController
    {
        public int Level { get; set; }
        public event LevelHandler OnLevelStarted;
        public event LevelHandler OnLevelFinished;
        
        public void StartLevel()
        {
            OnLevelStarted?.Invoke(new LevelChangedArgs(Level));
        }
        
        public void FinishLevel()
        {
            OnLevelFinished?.Invoke(new LevelChangedArgs(Level));
        }
    }

    
}