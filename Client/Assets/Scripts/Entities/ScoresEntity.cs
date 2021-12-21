using System.Collections.Generic;
using System.Linq;
using DI.Extensions;

namespace Entities
{
    public class ScoresEntity : EntityBase<Dictionary<ushort, long>>
    {
        public ScoresEntity()
        {
            Value = new Dictionary<ushort, long>();
        }

        public long? GetFirstScore()
        {
            if (Value.Any())
                return Value.First().Value;
            return null;
        }
        
        public long? GetScore(ushort _Key)
        {
            return Value.GetSafe(_Key, out _);
        }
    }
}