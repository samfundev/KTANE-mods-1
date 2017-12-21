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

        public void BuildMaze(AbstractRuleGenerator.RandomNext nextminmax)
        {
            for (var i = 0; i < 6; i++)
            {
                for (var j = 0; j < 6; j++)
                {
                    if (i > 0) nextminmax();
                    if (j > 0) nextminmax();
                }
            }
            PopulateMaze(nextminmax);
            nextminmax();
            nextminmax();   //Burn the coordinate circles.
        }

        public void PopulateMaze(AbstractRuleGenerator.RandomNext nextminmax)
        {
            var cellStack = new Stack<MazeCell>();
            var x = nextminmax(0, 6);
            var y = nextminmax(0, 6);
            var cell = GetCell(x, y);
            VisitCell(cell, cellStack, nextminmax);
        }

        public void VisitCell(MazeCell cell, Stack<MazeCell> cellStack, AbstractRuleGenerator.RandomNext nextminmax)
        {
            while (cell != null)
            {
                cell.Visited = true;
                var mazeCell = GetNextNeigbour(cell, nextminmax);
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

        public MazeCell GetNextNeigbour(MazeCell cell, AbstractRuleGenerator.RandomNext nextminmax)
        {
            var list = new List<MazeCell>();
            if(cell.X > 0                           && !CellGrid[cell.X - 1][cell.Y].Visited) list.Add(CellGrid[cell.X-1][cell.Y]);
            if(cell.X < CellGrid.Count - 1         && !CellGrid[cell.X + 1][cell.Y].Visited) list.Add(CellGrid[cell.X+1][cell.Y]);
            if(cell.Y > 0                           && !CellGrid[cell.X][cell.Y - 1].Visited) list.Add(CellGrid[cell.X][cell.Y - 1]);
            if(cell.Y < CellGrid[cell.X].Count - 1 && !CellGrid[cell.X][cell.Y + 1].Visited) list.Add(CellGrid[cell.X][cell.Y + 1]);
            return list.Count > 0 
                ? list[nextminmax(0, list.Count)] 
                : null;
        }

        public string ToSVG()
        {
            int sizeX = 300;
            int sizeY = 300;
            SVGGenerator svggenerator = new SVGGenerator(sizeX, sizeY);
            float num3 = (float)sizeX / (float)Size;
            float num4 = (float)sizeY / (float)Size;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    MazeCell mazeCell = CellGrid[i][j];
                    float num5 = (float)i * num3;
                    float num6 = (float)j * num4;
                    svggenerator.DrawCircle(num5 + num3 / 2f, num6 + num4 / 2f, 3f, true);
                    if (mazeCell.WallUp)
                    {
                        string strokeWidth = "3";
                        if (j == 0)
                        {
                            strokeWidth = "10";
                        }
                        svggenerator.DrawLine(num5, num6, num5 + num3, num6, strokeWidth, string.Empty);
                    }
                    if (mazeCell.WallLeft)
                    {
                        string strokeWidth2 = "3";
                        if (i == 0)
                        {
                            strokeWidth2 = "10";
                        }
                        svggenerator.DrawLine(num5, num6, num5, num6 + num4, strokeWidth2, string.Empty);
                    }
                    if (i == Size - 1)
                    {
                        svggenerator.DrawLine(num5 + num3, num6, num5 + num3, num6 + num4, "10", string.Empty);
                    }
                    if (j == Size - 1)
                    {
                        svggenerator.DrawLine(num5, num6 + num4, num5 + num3, num6 + num4, "10", string.Empty);
                    }
                }
            }
            return svggenerator.ToString();
        }
    }
}