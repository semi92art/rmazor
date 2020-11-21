using System.Collections.Generic;
using Managers;

public delegate void LevelStateHandler(LevelStateChangedArgs _Args);
    
public class LevelStateChangedArgs : System.EventArgs
{
    public int Level { get; set; }
    public long LifesLeft { get; set; }
    public Dictionary<MoneyType, long> Revenue { get; set; }
}