using System;

namespace Common.Entities
{
    [Serializable]
    public class LevelArgs : FileNameArgs
    {
        public int Level { get; set; }
    }
}