using System.Collections.Generic;
using System.Linq;
using Managers;

namespace Entities
{
    public class ScoresEntity : EntityBase<Dictionary<int, int>>
    {
        public ScoresEntity()
        {
            Value = new Dictionary<int, int>();
        }

        public int? GetFirstScore()
        {
            if (Value.Any())
                return Value.First().Value;
            return null;
        }
        
        public int? GetScore(int _Key)
        {
            if (Value.ContainsKey(_Key))
                return Value[_Key];
            return null;
        }
    }
}