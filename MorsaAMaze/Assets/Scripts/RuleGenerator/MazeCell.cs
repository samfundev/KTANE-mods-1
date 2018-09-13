namespace RuleGenerator
{
    public class MazeCell
    {
        public bool Visited;
        public bool WallUp = true;
        public bool WallDown = true;
        public bool WallLeft = true;
        public bool WallRight = true;
        public int X;
        public int Y;

        public bool ShouldSerializeVisited()
        {
            return false;
        }

        public MazeCell(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static void RemoveWalls(MazeCell m1, MazeCell m2)
        {
            if (m1.X - m2.X == 1)
            {
                m1.WallLeft = false;
                m2.WallRight = false;
            }
            if (m1.X - m2.X == -1)
            {
                m1.WallRight = false;
                m2.WallLeft = false;
            }
            if (m1.Y - m2.Y == 1)
            {
                m1.WallUp = false;
                m2.WallDown = false;
            }
            if (m1.Y - m2.Y == -1)
            {
                m1.WallDown = false;
                m2.WallUp = false;
            }
        }
    }
}