using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
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
            return Value.GetSafe(_Key, out _);
        }
    }
}