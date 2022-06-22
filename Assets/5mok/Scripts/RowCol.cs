namespace MyProject
{
    public struct RowCol
    {
        public int r;
        public int c;

        public RowCol(int r, int c)
        {
            this.r = r;
            this.c = c;
        }

        public static RowCol operator +(RowCol lhs, RowCol rhs)
            => new RowCol(lhs.r + rhs.r, lhs.c + rhs.c);
        public static RowCol operator -(RowCol lhs, RowCol rhs)
            => new RowCol(lhs.r - rhs.r, lhs.c - rhs.c);

        public override string ToString() => $"({this.r}, {this.c})";
    }
}