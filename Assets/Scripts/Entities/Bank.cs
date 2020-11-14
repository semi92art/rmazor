using System.Collections.Generic;
using Managers;

namespace Entities
{
    public class Bank
    {
        public Dictionary<MoneyType, long> Money { get; set; } = new Dictionary<MoneyType, long>();
        public bool Loaded { get; set; }
    }
}