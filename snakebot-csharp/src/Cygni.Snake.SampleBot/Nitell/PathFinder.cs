using System;
using System.Collections.Generic;
using System.Linq;
using Cygni.Snake.Client;

namespace Cygni.Snake.SampleBot.Nitell
{
    public class PathFinder
    {
        private readonly Map map;

        public PathFinder(Map map)
        {
            this.map = map;
        }

        public MapCoordinate[] FindShortestPath(MapCoordinate start, MapCoordinate target)
        {
            return FindShortestPath(start, target, (m, n) => 1);
        }

        public MapCoordinate[] FindShortestPath(MapCoordinate start, MapCoordinate target,
            Func<Map, MapCoordinate, int> weight)
        {
            // The set of nodes already evaluated.
            var closedSet = new HashSet<MapCoordinate>();
            // The set of currently discovered nodes still to be evaluated.
            // Initially, only the start node is known.
            var openSet = new HashSet<MapCoordinate> {start};
            // For each node, which node it can most efficiently be reached from.
            // If a node can be reached from many nodes, cameFrom will eventually contain the
            // most efficient previous step.
            var cameFrom = new Dictionary<MapCoordinate, MapCoordinate>();
            // For each node, the cost of getting from the start node to that node.
            var gScore = new Dictionary<MapCoordinate, int>();
            // The cost of going from start to start is zero.
            gScore[start] = 0;
            // For each node, the total cost of getting from the start node to the goal
            // by passing by that node. That value is partly known, partly heuristic.
            var fScore = new Dictionary<MapCoordinate, int>();
            // For the first node, that value is completely heuristic.
            fScore[start] = start.GetManhattanDistanceTo(target);

            while (openSet.Any())
            {
                //the node in openSet having the lowest fScore[] value
                var current = openSet.OrderBy(n => fScore.ContainsKey(n) ? fScore[n] : int.MaxValue).First();
                if (current.Equals(target))
                    return ReconstructPath(cameFrom, current);
                openSet.Remove(current);
                closedSet.Add(current);
                foreach (var neighbor in GetNeighbours(current, target))
                {
                    if (closedSet.Contains(neighbor))
                        continue; // Ignore the neighbor which is already evaluated.
                    // The distance from start to a neighbor
                    var tentativeScore = gScore[current] + weight(map, neighbor);
                    if (!openSet.Contains(neighbor)) // Discover a new node
                        openSet.Add(neighbor);
                    else if (tentativeScore >= gScore[neighbor])
                        continue; // This is not a better path.

                    // This path is the best until now. Record it!
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeScore;
                    fScore[neighbor] = gScore[neighbor] + neighbor.GetManhattanDistanceTo(target);
                }
            }
            return null;
        }

        private IEnumerable<MapCoordinate> GetNeighbours(MapCoordinate current, MapCoordinate target)
        {
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                var destination = current.GetDestination(direction);
                if (destination.IsInsideMap(map.Width, map.Height) && !map.IsObstace(destination) &&
                    !map.IsSnake(destination))
                    yield return destination;
            }
        }

        private MapCoordinate[] ReconstructPath(Dictionary<MapCoordinate, MapCoordinate> cameFrom, MapCoordinate current)
        {
            var list = new List<MapCoordinate>();
            while (cameFrom.Keys.Contains(current))
            {
                list.Add(current);
                current = cameFrom[current];
            }
            list.Reverse();
            return list.ToArray();
        }
    }
}