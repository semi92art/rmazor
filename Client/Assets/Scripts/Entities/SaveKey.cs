using System;
using System.Collections.Generic;
using Games.RazorMaze.Models;

namespace Entities
{

    
    public class SaveKey<T>
    {
        public string Key { get; }
    
        public SaveKey(string _Key)
        {
            Key = _Key;
        }
    }
}