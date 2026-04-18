using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace OpenWorldEvents
{
    public class MazeGenerator
    {
        private readonly int width, height;
        private readonly bool[,] visited;
        private readonly bool[,] verticalWalls;
        private readonly bool[,] horizontalWalls;
        public Point exit;
        private readonly Random rng = new Random();

        public MazeGenerator(int width, int height)
        {
            this.width = width;
            this.height = height;
            visited = new bool[width, height];
            verticalWalls = new bool[width, height + 1];
            horizontalWalls = new bool[width + 1, height];

            // Initialize all walls as present (true)
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height + 1; j++)
                    verticalWalls[i, j] = true;

            for (int i = 0; i < width + 1; i++)
                for (int j = 0; j < height; j++)
                    horizontalWalls[i, j] = true;
        }

        public void GenerateMaze()
        {
            // Start from the upper-left cell (0,0)
            CarvePassagesFrom(0, 0);
        }

        private void CarvePassagesFrom(int x, int y)
        {
            visited[x, y] = true;

            (int dx, int dy)[] directions = GetRandomDirections();
            foreach (var (dx, dy) in directions)
            {
                int nx = x + dx, ny = y + dy;
                if (IsInBounds(nx, ny) && !visited[nx, ny])
                {
                    // Remove the wall between the current cell and the next cell
                    RemoveWallBetween(x, y, nx, ny);
                    // Carve the path from the next cell
                    CarvePassagesFrom(nx, ny);
                }
            }
        }

        enum Direction
        {
            North,
            East,
            South,
            West
        }

        public HashSet<Point> SimulateHandRule(Point start, bool isLeftHand)
        {
            var path = new HashSet<Point>();
            Point current = start;
            Direction direction = Direction.North; // Assume starting direction is north

            do
            {
                path.Add(current);
                var (next, nextDirection) = GetNextCellAndDirection(current, direction, isLeftHand);
                if (next.Equals(current)) break; // No further way or looping
                current = next;
                direction = nextDirection;
            } while (!current.Equals(start)); // Assuming start is not immediately a dead end

            return path;
        }

        public List<Point> FindDeadEnds()
        {
            List<Point> deadEnds = new List<Point>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (IsDeadEnd(x, y))
                    {
                        deadEnds.Add(new Point(x, y));
                    }
                }
            }
            return deadEnds;
        }

        public Point ChooseStairsLocation(HashSet<Point> leftHandPath, HashSet<Point> rightHandPath)
        {
            List<Point> deadEnds = FindDeadEnds();
            // Filter out dead ends that are on either hand rule path
            var candidateDeadEnds = deadEnds.Where(de => !leftHandPath.Contains(de) && !rightHandPath.Contains(de)).ToList();

            if (candidateDeadEnds.Count > 0)
            {
                // Select a dead end randomly from candidates to place the stairs
                return candidateDeadEnds[new Random().Next(candidateDeadEnds.Count)];
            }

            return new Point(-1, -1); // Fallback if no suitable dead end is found
        }



        private (Point, Direction) GetNextCellAndDirection(Point current, Direction currentDirection, bool isLeftHand)
        {
            // Adjusted order based on left or right-hand rule
            var priorityDirections = GetPriorityDirections(currentDirection, isLeftHand);

            foreach (var dir in priorityDirections)
            {
                Point next = GetNextCell(current, dir);
                if (IsInBounds(next.X, next.Y) && CanMoveToNextCell(current, next, dir))
                {
                    return (next, dir);
                }
            }

            // If no direction is valid, it implies being stuck or at a dead-end.
            // Returning the current point and direction to handle this gracefully.
            return (current, currentDirection);
        }

        // Returns the priority of directions to try, based on the current facing direction and the hand rule
        private IEnumerable<Direction> GetPriorityDirections(Direction currentDirection, bool isLeftHand)
        {
            if (isLeftHand)
            {
                switch (currentDirection)
                {
                    case Direction.North: return new[] { Direction.West, Direction.North, Direction.East, Direction.South };
                    case Direction.South: return new[] { Direction.East, Direction.South, Direction.West, Direction.North };
                    case Direction.East: return new[] { Direction.North, Direction.East, Direction.South, Direction.West };
                    case Direction.West: return new[] { Direction.South, Direction.West, Direction.North, Direction.East };
                }
            }
            else // Right-hand rule
            {
                switch (currentDirection)
                {
                    case Direction.North: return new[] { Direction.East, Direction.North, Direction.West, Direction.South };
                    case Direction.South: return new[] { Direction.West, Direction.South, Direction.East, Direction.North };
                    case Direction.East: return new[] { Direction.South, Direction.East, Direction.North, Direction.West };
                    case Direction.West: return new[] { Direction.North, Direction.West, Direction.South, Direction.East };
                }
            }
            return Enumerable.Empty<Direction>(); // Fallback, shouldn't happen
        }

        private bool CanMoveToNextCell(Point current, Point next, Direction direction)
        {
            // Check if moving from current to next is blocked by a wall
            switch (direction)
            {
                case Direction.North:
                    return next.Y >= 0 && !horizontalWalls[next.X, next.Y + 1];
                case Direction.South:
                    return next.Y < height && !horizontalWalls[next.X, next.Y];
                case Direction.East:
                    return next.X < width && !verticalWalls[next.X, next.Y];
                case Direction.West:
                    return next.X >= 0 && !verticalWalls[next.X + 1, next.Y];
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), "Invalid direction");
            }
        }

        private Point GetNextCell(Point current, Direction direction)
        {
            // Determine the next cell based on the direction
            switch (direction)
            {
                case Direction.North: return new Point(current.X, current.Y - 1);
                case Direction.South: return new Point(current.X, current.Y + 1);
                case Direction.East: return new Point(current.X + 1, current.Y);
                case Direction.West: return new Point(current.X - 1, current.Y);
                default: return current; // Should never happen
            }
        }


        private (int dx, int dy)[] GetRandomDirections()
        {
            var directions = new[]
            {
                (dx: 1, dy: 0),  // Right
                (dx: -1, dy: 0), // Left
                (dx: 0, dy: 1),  // Down
                (dx: 0, dy: -1)  // Up
            };

            // Shuffle the directions
            for (int i = directions.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var temp = directions[i];
                directions[i] = directions[j];
                directions[j] = temp;
            }

            return directions;
        }


        private bool AllNeighborsVisited(int x, int y)
        {
            (int dx, int dy)[] directions = { (1, 0), (-1, 0), (0, 1), (0, -1) };
            foreach (var (dx, dy) in directions)
            {
                int nx = x + dx, ny = y + dy;
                if (IsInBounds(nx, ny) && !visited[nx, ny])
                {
                    return false; // Found an unvisited neighbor
                }
            }
            return true; // All neighbors are visited
        }

        private List<(int, int)> GetUnvisitedNeighbors(int x, int y)
        {
            var neighbors = new List<(int, int)>();

            if (x > 0 && !visited[x - 1, y])
                neighbors.Add((x - 1, y));
            if (y > 0 && !visited[x, y - 1])
                neighbors.Add((x, y - 1));
            if (x < width - 1 && !visited[x + 1, y])
                neighbors.Add((x + 1, y));
            if (y < height - 1 && !visited[x, y + 1])
                neighbors.Add((x, y + 1));

            return neighbors;
        }

        private void RemoveWallBetween(int x1, int y1, int x2, int y2)
        {
            if (x1 == x2)
            {
                horizontalWalls[x1, Math.Max(y1, y2)] = false;
            }
            else
            {
                verticalWalls[Math.Max(x1, x2), y1] = false;
            }
        }


        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        private void Shuffle<T>(T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        public void SaveMazeImage(string filename)
        {
            int cellSize = 20; // Size of each cell
            int wallThickness = 2; // Thickness of the walls
            int imgWidth = width * cellSize + (width + 1) * wallThickness;
            int imgHeight = height * cellSize + (height + 1) * wallThickness;
            Bitmap bmp = new Bitmap(imgWidth, imgHeight);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);

                // Setup for drawing walls
                using (Pen wallPen = new Pen(Color.Black, wallThickness))
                {
                    // Draw horizontal walls
                    for (int i = 0; i < horizontalWalls.GetLength(0); i++)
                    {
                        for (int j = 0; j < horizontalWalls.GetLength(1); j++)
                        {
                            if (horizontalWalls[i, j])
                            {
                                int x1 = i * (cellSize + wallThickness);
                                int y1 = j * (cellSize + wallThickness);
                                g.DrawLine(wallPen, x1, y1, x1 + cellSize, y1);
                            }
                        }
                    }

                    // Draw vertical walls
                    for (int i = 0; i < verticalWalls.GetLength(0); i++)
                    {
                        for (int j = 0; j < verticalWalls.GetLength(1); j++)
                        {
                            if (verticalWalls[i, j])
                            {
                                int x1 = i * (cellSize + wallThickness);
                                int y1 = j * (cellSize + wallThickness);
                                g.DrawLine(wallPen, x1, y1, x1, y1 + cellSize);
                            }
                        }
                    }
                }

                // Draw the stairs (exit)
                int stairsX = exit.X * (cellSize + wallThickness) + wallThickness;
                int stairsY = exit.Y * (cellSize + wallThickness) + wallThickness;
                using (Brush stairsBrush = new SolidBrush(Color.Red)) // Choose a color that stands out
                {
                    g.FillRectangle(stairsBrush, stairsX, stairsY, cellSize, cellSize);
                }

                // Optionally, draw stairs symbol with lines to mimic steps
                using (Pen stairsPen = new Pen(Color.Black, 2))
                {
                    int stepSize = cellSize / 4; // Just as an example, adjust based on your cell size
                    for (int i = 1; i <= 3; i++) // Draw 3 steps
                    {
                        g.DrawLine(stairsPen, stairsX, stairsY + i * stepSize, stairsX + cellSize, stairsY + i * stepSize);
                    }
                }
            }

            bmp.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
        }


        public void CreateLoops(int numberOfLoops)
        {
            for (int i = 0; i < numberOfLoops; i++)
            {
                int x, y;
                do
                {
                    x = rng.Next(1, width - 1);
                    y = rng.Next(1, height - 1);
                } while (!IsDeadEnd(x, y)); // Assure it's a dead-end

                RemoveOppositeWall(x, y);
            }
        }

        private bool IsDeadEnd(int x, int y)
        {
            // Check if there is exactly one open space (not a wall) around the cell
            int openCount = 0;
            if (x > 0 && !verticalWalls[x, y]) openCount++;
            if (x < width - 1 && !verticalWalls[x + 1, y]) openCount++;
            if (y > 0 && !horizontalWalls[x, y]) openCount++;
            if (y < height - 1 && !horizontalWalls[x, y + 1]) openCount++;

            return openCount == 1;
        }

        private void RemoveOppositeWall(int x, int y)
        {
            // Remove the wall opposite the open space
            if (x > 0 && !verticalWalls[x, y])
            {
                // Open space is to the left, remove the right wall if possible
                if (x < width - 1) verticalWalls[x + 1, y] = false;
            }
            else if (x < width - 1 && !verticalWalls[x + 1, y])
            {
                // Open space is to the right, remove the left wall if possible
                if (x > 0) verticalWalls[x, y] = false;
            }
            else if (y > 0 && !horizontalWalls[x, y])
            {
                // Open space is above, remove the bottom wall if possible
                if (y < height - 1) horizontalWalls[x, y + 1] = false;
            }
            else if (y < height - 1 && !horizontalWalls[x, y + 1])
            {
                // Open space is below, remove the top wall if possible
                if (y > 0) horizontalWalls[x, y] = false;
            }
        }
    }
}
