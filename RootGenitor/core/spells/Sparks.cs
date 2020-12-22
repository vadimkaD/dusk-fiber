using System;
using System.Collections.Generic;
using System.Text;

namespace RootGenitor.core.spells
{
    class Sparks : Spell
    {
        public override SpellResult cast(Player owner, Player opponent)
        {
            SpellResult result = new SpellResult();
            PlayerData ownerData = owner.data;
            PlayerData targetData = opponent.data;
            Random rnd = new Random();
            int value1 = rnd.Next(1, 4);
            int value2 = rnd.Next(1, 4);
            int value3 = rnd.Next(1, 4);
            targetData.Hp -= value1 + value2 + value3;
            if (targetData.Hp <= 0)
            {
                targetData.Hp = 0;
            }
            result.ownerData = ownerData;
            result.opponentData = targetData;
            return result;
        }
    }
}
