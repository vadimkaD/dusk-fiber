using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;


namespace RootGenitor.core
{
    class UnitModifier : Modifier
    {

        private UnitModifier(ulong flag, string name): base(flag, name)
        {
        }
        public static readonly UnitModifier BLESS = new UnitModifier(1 << 0, "BLESS");
        public static readonly UnitModifier CURSE = new UnitModifier(1 << 1, "CURSE");

    }
}

