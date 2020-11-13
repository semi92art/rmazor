using System.Collections.Generic;
using Managers;

namespace Entities
{
    public class Bank
    {
        public Dictionary<MoneyType, int> Money { get; set; } = new Dictionary<MoneyType, int>();
        public bool Loaded { get; set; }
    }
}