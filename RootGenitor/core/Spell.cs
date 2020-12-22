using System;
using System.Collections.Generic;
using System.Text;

namespace RootGenitor.core
{
    class SpellResult
    {
        public PlayerData ownerData;
        public PlayerData opponentData;
    }
    abstract class Spell
    {
        abstract public SpellResult cast(Player owner, Player opponent);
    }
}
