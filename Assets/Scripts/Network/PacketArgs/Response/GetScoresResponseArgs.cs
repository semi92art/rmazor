using System.Collections.Generic;

namespace Network.PacketArgs
{
    public class GetScoresResponseArgs
    {
        public List<GetScoreResponseArgs> Scores { get; set; }
    }
}