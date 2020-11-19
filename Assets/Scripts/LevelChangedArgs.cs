public delegate void LevelHandler(LevelChangedArgs _Args);
    
public class LevelChangedArgs : System.EventArgs
{
    public int Level { get; }

    public LevelChangedArgs(int _Level)
    {
        Level = _Level;
    }
}