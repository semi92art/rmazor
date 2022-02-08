using System;

namespace Common.Entities
{
    [Serializable]
    public class MoneyArgs : FileNameArgs
    {
        public long Money { get; set; }
    }
}