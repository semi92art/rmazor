using System.Collections.Generic;
using System.Linq;
using Managers;

namespace Entities
{
    public class ScoresEntity : EntityBase<Dictionary<int, long>>
    {
        public ScoresEntity()
        {
            Value = new Dictionary<int, long>();
        }

        public long? GetFirstScore()
        {
            if (Value.Any())
                return Value.First().Value;
            return null;
        }
        
        public long? GetScore(int _Key)
        {
            if (Value.ContainsKey(_Key))
                return Value[_Key];
            return null;
        }
    }
}