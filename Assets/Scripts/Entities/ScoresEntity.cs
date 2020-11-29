using System.Collections.Generic;

namespace Entities
{
    public class ScoresEntity
    {
        public Dictionary<string, int> Scores { get; set; } = new Dictionary<string, int>();
        public bool Loaded { get; set; }
    }
}