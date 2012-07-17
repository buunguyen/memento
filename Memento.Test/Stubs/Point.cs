namespace Memento.Test.Stubs
{
    struct Point
    {
        public readonly int X;
        public readonly int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Point other)
        {
            return other.Y == Y && other.X == X;
        }

        public override bool Equals(object obj)
        {
            return obj is Point && Equals((Point) obj);
        }

        public override int GetHashCode()
        {
            return unchecked(Y*397 ^ X);
        }

        public static bool operator ==(Point left, Point right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !Equals(left, right);
        }
    }
}