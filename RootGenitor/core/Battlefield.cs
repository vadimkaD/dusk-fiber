using System;
using System.Collections.Generic;
using System.Text;

namespace RootGenitor.core
{
    class Battlefield
    {
        private Dictionary<Point, Hex> hexfield = new Dictionary<Point, Hex>();
        public Battlefield()
        {

        }

        public Hex getHex(Point point)
        {
            return hexfield[point];
        }

        public void registerHex(Point point, Hex hex)
        {
            hexfield.Add(point, hex);
        }
    }
}
