using System;

namespace Common.Entities
{
    [Serializable]
    public class SavedGame : FileNameArgs
    {
        public long  Level { get; set; }
        public long Money { get; set; }
    }
}