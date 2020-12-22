using System;

namespace RootGenitor.core
{
    public class Point
    {
        public readonly int y;
        public readonly int x;

        public Point (int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj)
        {
            return obj is Point point &&
                   y == point.y &&
                   x == point.x;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(y, x);
        }
    }
}