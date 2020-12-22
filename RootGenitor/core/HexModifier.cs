using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;


namespace RootGenitor.core
{
    class HexModifier : Modifier
    {

        private HexModifier(ulong flag, string name) : base(flag, name)
        {
        }
        public static readonly HexModifier FIRE = new HexModifier(1 << 0, "FIRE");
        public static readonly HexModifier ICE = new HexModifier(1 << 1, "ICE");

    }
}
