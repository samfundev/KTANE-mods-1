using System.Collections.Generic;

namespace Assets.Scripts.RuleGenerator
{
    public class Maze
    {
        private int Size = 6;

        public List<List<MazeCell>> CellGrid;

        public Maze()
        {
            CellGrid = new List<List<MazeCell>>();
            for (var i = 0; i < 6; i++)
            {
                var list = new List<MazeCell>();
                CellGrid.Add(list);
                for (var j = 0; j < 6; j++)
                {
                    var mazeCell = new MazeCell(i, j);
                    list.Add(mazeCell);
                }
            }
        }

        public MazeCell GetCell(int x, int y)
        {
            if (x < 0 || x >= 6 || y < 0 || y >= 6)
                return null;
            return CellGrid[x][y];
        }

        public void BuildMaze(MonoRandom rng)
        {
            for (var i = 0; i < 6; i++)
            {
                for (var j = 0; j < 6; j++)
                {
                    if (i > 0) rng.Next();
                    if (j > 0) rng.Next();
                }
            }
            PopulateMaze(rng);
            rng.Next();
            rng.Next();   //Burn the coordinate circles.
        }

        public void PopulateMaze(MonoRandom rng)
        {
            var cellStack = new Stack<MazeCell>();
            var x = rng.Next(0, 6);
            var y = rng.Next(0, 6);
            var cell = GetCell(x, y);
            VisitCell(cell, cellStack, rng);
        }

        public void VisitCell(MazeCell cell, Stack<MazeCell> cellStack, MonoRandom rng)
        {
            while (cell != null)
            {
                cell.Visited = true;
                var mazeCell = GetNextNeigbour(cell, rng);
                if (mazeCell != null)
                {
                    MazeCell.RemoveWalls(cell, mazeCell);
                    cellStack.Push(cell);
                }
                else if (cellStack.Count > 0)
                {
                    mazeCell = cellStack.Pop();
                }
                cell = mazeCell;
            }
        }

        public MazeCell GetNextNeigbour(MazeCell cell, MonoRandom rng)
        {
            var list = new List<MazeCell>();
            if(cell.X > 0                           && !CellGrid[cell.X - 1][cell.Y].Visited) list.Add(CellGrid[cell.X-1][cell.Y]);
            if(cell.X < CellGrid.Count - 1         && !CellGrid[cell.X + 1][cell.Y].Visited) list.Add(CellGrid[cell.X+1][cell.Y]);
            if(cell.Y > 0                           && !CellGrid[cell.X][cell.Y - 1].Visited) list.Add(CellGrid[cell.X][cell.Y - 1]);
            if(cell.Y < CellGrid[cell.X].Count - 1 && !CellGrid[cell.X][cell.Y + 1].Visited) list.Add(CellGrid[cell.X][cell.Y + 1]);
            return list.Count > 0 
                ? list[rng.Next(0, list.Count)] 
                : null;
        }
    }
}