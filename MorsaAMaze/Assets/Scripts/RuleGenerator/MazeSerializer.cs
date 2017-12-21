using System;
using System.Collections.Generic;
using System.Linq;


namespace Assets.Scripts.RuleGenerator
{
    public static class MazeSerializer
    {
        [Flags]
        private enum MazeDirections
        {
            None = 0,
            Up = 1,
            Down = 2,
            Left = 4,
            Right = 8
        }

        private const string MazeCode = "0123456789ABCDEF";

        public static string Serialize(List<Maze> mazes)
        {
            var mazeStrings = new List<string>();
            foreach (var maze in mazes)
            {
                var mazeColumns = new List<string>();
                for (var x = 0; x < 6; x++)
                {
                    var mazeRows = "";
                    for (var y = 0; y < 6; y++)
                    {
                        var row = MazeDirections.None;
                        if (maze.GetCell(x, y).WallUp) row |= MazeDirections.Up;
                        if (maze.GetCell(x, y).WallDown) row |= MazeDirections.Down;
                        if (maze.GetCell(x, y).WallLeft) row |= MazeDirections.Left;
                        if (maze.GetCell(x, y).WallRight) row |= MazeDirections.Right;
                        mazeRows += MazeCode[(int) row];
                    }
                    mazeColumns.Add(mazeRows);
                }
                mazeStrings.Add(string.Join(":", mazeColumns.ToArray()));
            }
            return string.Join("|", mazeStrings.ToArray());
        }

        public static List<Maze> DeSerialize(string input)
        {
            if (string.IsNullOrEmpty(input) || 
                input.Split('|').Length != 18 || 
                input.Split('|').Any(maze => maze.Split(':').Length != 6 || 
                    maze.Split(':').Any(col => col.Length != 6 ||
                        col.Any(row => !MazeCode.Contains(row) || 
                            row.Equals('0') || 
                            row.Equals('F')))))
                return null;

            var mazes = new List<Maze>();

            var mazeStrings = input.Split('|');
            foreach (var mazeString in mazeStrings)
            {
                var maze = new Maze();
                for (var x = 0; x < 6; x++)
                {
                    var col = mazeString.Split(':')[x];
                    for (var y = 0; y < 6; y++)
                    {
                        var cell = maze.GetCell(x, y);
                        var row = (MazeDirections) MazeCode.IndexOf(col[y]);
                        cell.WallUp = (row & MazeDirections.Up) == MazeDirections.Up;
                        cell.WallDown = (row & MazeDirections.Down) == MazeDirections.Down;
                        cell.WallLeft = (row & MazeDirections.Left) == MazeDirections.Left;
                        cell.WallRight = (row & MazeDirections.Right) == MazeDirections.Right;
                    }
                }
                mazes.Add(maze);
            }

            return mazes;
        }
    }
}