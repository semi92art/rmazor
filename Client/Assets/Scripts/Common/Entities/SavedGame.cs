using System;

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
        public long     Level { get; set; }
        public long     Money { get; set; }
        public object[] Args  { get; set; }
    }
}