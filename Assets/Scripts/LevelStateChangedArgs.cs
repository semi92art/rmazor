using System.Collections.Generic;
using Managers;

public delegate void LevelStateHandler(LevelStateChangedArgs _Args);
    
public class LevelStateChangedArgs
{
    public int Level { get; set; }
}