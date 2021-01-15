using System.Collections.Generic;
using Managers;

namespace Entities
{
    public class ScoresEntity
    {
        public Dictionary<ScoreType, int> Scores { get; set; } = new Dictionary<ScoreType, int>();
        public bool Loaded { get; set; }
    }
}