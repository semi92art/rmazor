using System;
using System.Collections.Generic;

namespace Common.Entities
{
    [Serializable]
    public class FileNameArgs
    {
        public string FileName { get; set; }
    }
    
    [Serializable]
    public class SavedGame : FileNameArgs
    {
        public long                       Level { get; set; }
        public long                       Money { get; set; }
        public Dictionary<string, object> Args  { get; set; }
    }
}