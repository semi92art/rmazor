using System.Collections.Generic;
using System.Linq;
using Managers;

namespace Entities
{
    public class ScoresEntity
    {
        public Dictionary<int, int> Scores { get; set; } = new Dictionary<int, int>();
        public bool Loaded { get; set; }

        public int? GetFirstScore()
        {
            if (Scores.Any())
                return Scores.First().Value;
            return null;
        }
        
        public int? GetScore(int _Key)
        {
            if (Scores.ContainsKey(_Key))
                return Scores[_Key];
            return null;
        }
    }
}