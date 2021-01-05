using System.Collections.Generic;

namespace Games.PathFinder
{
    public interface IHasNeighbours<N>
    {
        IEnumerable<N> Neighbours { get; }
    }
}
