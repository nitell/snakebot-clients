using System;
using System.Linq;
using Cygni.Snake.Client;

namespace Cygni.Snake.SampleBot.Nitell
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class Glennbot : SnakeBot
    {
        private Map map;
        private PathFinder pathFinder;

        public Glennbot() : base("Glenneth")
        {
        }


        public override Direction GetNextMove(Map map)
        {
            try
            {
                this.map = map;
                pathFinder = new PathFinder(map);

                //Go chase some food
                var foodPath = FindFoodPath();
                if (foodPath != null)
                    return Follow(foodPath);

                //Go chase some other snakes tail
                var tailPath = FindCloseTailPath();
                if (tailPath != null)
                    return Follow(tailPath);


                //ohh.. we're in trouble
                if (CanGo(Direction.Right))
                    return Direction.Right;
                if (CanGo(Direction.Left))
                    return Direction.Left;
                if (CanGo(Direction.Up))
                    return Direction.Up;

                return Direction.Down;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return Direction.Down;
            }
        }

        private MapCoordinate[] FindCloseTailPath()
        {
            foreach (
                var t in
                    map.Snakes.Where(s => s.Body.Any())
                    .Select(s => s.Body.Last()).OrderByDescending(s => s.GetManhattanDistanceTo(map.MySnake.HeadPosition)))
            {
                foreach (var p in pathFinder.GetNeighbours(t))
                {
                    var tailPath = pathFinder.FindShortestPath(map.MySnake.HeadPosition, p, AvoidHeadsAndWalls);
                    if (tailPath != null)
                        return tailPath;
                }
            }
            return null;
        }

        private bool CanGo(Direction dir)
        {
            var nextPos = map.MySnake.HeadPosition.GetDestination(dir);
            return nextPos.IsInsideMap(map.Width, map.Height) && !map.IsObstace(nextPos) && !map.IsSnake(nextPos);
        }

        private Direction Follow(MapCoordinate[] path)
        {
            if (path.First().X > map.MySnake.HeadPosition.X)
                return Direction.Right;
            if (path.First().X < map.MySnake.HeadPosition.X)
                return Direction.Left;
            if (path.First().Y > map.MySnake.HeadPosition.Y)
                return Direction.Down;
            return Direction.Up;
        }

        private MapCoordinate[] FindFoodPath()
        {
            //Try to find food that I'm closest to
            //If not, choose the food furthest away food, just to stay alive            
            MapCoordinate[] longestPath = null;

            foreach (var f in map.FoodPositions.OrderBy(p => map.MySnake.HeadPosition.GetManhattanDistanceTo(p)))
            {
                var myPath = pathFinder.FindShortestPath(map.MySnake.HeadPosition, f);
                if (myPath != null)
                {
                    var otherPaths = map.Snakes.Where(s => s.Id != map.MySnake.Id)
                        .Select(s => pathFinder.FindShortestPath(s.HeadPosition, f))
                        .Where(p => p != null && p.Any()).ToArray();
                    var shortestOtherPath = otherPaths.Any() ? otherPaths.Min(p => p.Length) : int.MaxValue;
                    if (myPath.Length < shortestOtherPath)
                        return pathFinder.FindShortestPath(map.MySnake.HeadPosition, f, AvoidHeadsAndWalls);

                    var longestSafePath = pathFinder.FindShortestPath(map.MySnake.HeadPosition, f, AvoidHeadsAndWalls);

                    if (longestPath == null || longestSafePath.Length < myPath.Length)
                        longestPath = longestSafePath;
                }
            }
            return longestPath;
        }

        private int AvoidHeadsAndWalls(Map map, MapCoordinate target)
        {
            if (map.Snakes.Any(s => s.Id != map.MySnake.Id && s.HeadPosition.GetManhattanDistanceTo(target) < 3))
                return 2;

            if (target.X == 0 || target.X == map.Width - 1 ||
                target.Y == 0 || target.Y == map.Height - 1)
                return 2;

            return 1;
        }
    }
}