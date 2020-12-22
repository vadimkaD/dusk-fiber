using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RootGenitor.core
{
    class Hex
    {
        public bool isPassable;
        public List<HexModifier> modifiers;

        public Hex(bool isPassable)
        {
            this.isPassable = isPassable;
            this.modifiers = new List<HexModifier>();
        }

        public override string ToString() {
            string total = isPassable.ToString();
            if (Convert.ToBoolean(modifiers.Count))
            {
                total = total + ", "
                + HexModifier.GetNames(modifiers.Aggregate<HexModifier, ulong>(0, (ulong total, HexModifier current) => total | current.flag));
            }
            return total;
        }
    }
}
