using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class PathFinderInGame
{
    //distance f-ion should return distance between two adjacent nodes
    //estimate should return distance between any node and destination node
    public static Path<Tile> FindPath(Tile _Start, Tile _Destination)
    {
        var closed = new List<Tile>();
        var queue = new PriorityQueue<double, Path<Tile>>();
        queue.Enqueue(0, new Path<Tile>(_Start));

        while (!queue.IsEmpty && closed.Count < 10000)
        {
            var path = queue.Dequeue();
          
            foreach (Tile tile in closed)
            {
                if (tile.Equals(path.LastStep))
                {
                    continue;
                }
            }

            if (path.LastStep.Equals(_Destination))
                           return path;         

            closed.Add(path.LastStep);

            foreach (Tile n in path.LastStep.Neighbours)
            {
                double d = Distance(path.LastStep, n);
                var newPath = path.AddStep(n, d);
                queue.Enqueue(newPath.TotalCost + Estimate(n, _Destination),
                    newPath);
            }
        }

        return null;
    }
    static double Distance(Tile _Tile1, Tile _Tile2)
    {
        return 1;
    }

    
    static double Estimate(Tile _Tile, Tile _DestTile)
    {
        float dx = Mathf.Abs(_DestTile.X - _Tile.X) + Random.Range(0f, 100f);
        float dy = Mathf.Abs(_DestTile.Y - _Tile.Y) + Random.Range(0f, 100f);;
        int z1 = -(_Tile.X + _Tile.Y);
        int z2 = -(_DestTile.X + _DestTile.Y);
        float dz = Mathf.Abs(z2 - z1)  + Random.Range(0f, 100f);;

        return Mathf.Max(dx, dy, dz);
    }

   
}
