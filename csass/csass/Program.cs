using RobotCleaner;
using System;
using System.Threading;

namespace RobotCleaner
{
    public class Map
    {
        private enum CellType { Empty, Dirt, Obstacle, Cleaned };
        private CellType[,] _grid;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Map(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            _grid = new CellType[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _grid[x, y] = CellType.Empty;
                }
            }
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < this.Width && y >= 0 && y < this.Height;
        }

        public bool IsDirt(int x, int y)
        {
            return IsInBounds(x, y) && _grid[x, y] == CellType.Dirt;
        }

        public bool IsObstacle(int x, int y)
        {
            return IsInBounds(x, y) && _grid[x, y] == CellType.Obstacle;
        }

        public void AddObstacle(int x, int y)
        {
            if (IsInBounds(x, y))
                _grid[x, y] = CellType.Obstacle;
        }
        public void AddDirt(int x, int y)
        {
            if (IsInBounds(x, y))
                _grid[x, y] = CellType.Dirt;
        }

        public void Clean(int x, int y)
        {
            if (IsInBounds(x, y))
            {
                _grid[x, y] = CellType.Cleaned;
            }
        }

        public void Display(int robotX, int robotY)
        {
            Console.Clear();
            Console.WriteLine("Vacuum cleaner robot simulation");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Legends: #=Obstacles, D=Dirt, .=Empty, R=Robot, C=Cleaned");

            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    if (x == robotX && y == robotY)
                    {
                        Console.Write("R ");
                    }
                    else
                    {
                        switch (_grid[x, y])
                        {
                            case CellType.Empty: Console.Write(". "); break;
                            case CellType.Dirt: Console.Write("D "); break;
                            case CellType.Obstacle: Console.Write("# "); break;
                            case CellType.Cleaned: Console.Write("C "); break;
                        }
                    }
                }
                Console.WriteLine();
            }
            Thread.Sleep(200);
        }
    }

  
    public interface ICleaningStrategy
    {
        void Clean(Robot robot, Map map);
    }

    public class SPatternStrategy : ICleaningStrategy
    {
        public void Clean(Robot robot, Map map)
        {
            int direction = 1; // 1 = right, -1 = left
            for (int row = 0; row < map.Height; row++)
            {
                int startX = (direction == 1) ? 0 : map.Width - 1;
                int endX = (direction == 1) ? map.Width : -1;

                for (int x = startX; x != endX; x += direction)
                {
                    robot.Move(x, row);
                    robot.CleanCurrentSpot();
                }
                direction *= -1; // reverse direction each row
            }
        }
    }

    public class VerticalSweepStrategy : ICleaningStrategy
    {
        public void Clean(Robot robot, Map map)
        {
            int direction = 1; // 1 = down, -1 = up
            for (int col = 0; col < map.Width; col++)
            {
                int startY = (direction == 1) ? 0 : map.Height - 1;
                int endY = (direction == 1) ? map.Height : -1;

                for (int y = startY; y != endY; y += direction)
                {
                    robot.Move(col, y);
                    robot.CleanCurrentSpot();
                }

                direction *= -1; // reverse direction for the next column
            }
        }
    }

    public class Robot
    {
        private readonly Map _map;
        private ICleaningStrategy _strategy; // no longer readonly

        public int X { get; set; }
        public int Y { get; set; }

        public Map Map { get { return _map; } }

        public Robot(Map map, ICleaningStrategy strategy)
        {
            _map = map;
            _strategy = strategy;
            X = 0;
            Y = 0;
        }

        public void SetStrategy(ICleaningStrategy strategy)
        {
            _strategy = strategy;
        }

        public bool Move(int newX, int newY)
        {
            if (_map.IsInBounds(newX, newY) && !_map.IsObstacle(newX, newY))
            {
                X = newX;
                Y = newY;
                _map.Display(X, Y);
                return true;
            }
            return false;
        }

        public void CleanCurrentSpot()
        {
            if (_map.IsDirt(X, Y))
            {
                _map.Clean(X, Y);
                _map.Display(X, Y);
            }
        }

        public void StartCleaning()
        {
            _strategy.Clean(this, _map);
        }
    }

    // MAIN 
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Initialize robot");

            Map map = new Map(20, 10);

            map.AddObstacle(3, 8);
            map.AddObstacle(10, 4);
            map.AddObstacle(5, 5);
            map.AddObstacle(13, 6);
            

            map.AddDirt(2, 2);
            map.AddDirt(4, 5);
            map.AddDirt(7, 7);
            map.AddDirt(12, 3);
            map.AddDirt(17, 6);

            map.Display(0, 0);

            Robot robot = new Robot(map, new SPatternStrategy());
            Console.WriteLine("Running S-Pattern Strategy...");
            robot.StartCleaning();

          
            Console.WriteLine("Switching to Vertical Sweep Strategy...");
            robot.SetStrategy(new VerticalSweepStrategy());
            robot.StartCleaning();

            Console.WriteLine("Done.");
        }
    }
}
