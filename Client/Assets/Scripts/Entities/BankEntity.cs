using System.Collections.Generic;
using Managers;

namespace Entities
{
    public class BankEntity
    {
        public Dictionary<BankItemType, long> BankItems { get; set; } 
            = new Dictionary<BankItemType, long>();
        public bool Loaded { get; set; }
    }
}