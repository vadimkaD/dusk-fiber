using RootGenitor.core;
using System;
using System.Collections.Generic;
using System.Text;

namespace RootGenitor.api
{
    interface BattleApi
    {
        public void registerInBattle(Player player, string battleId);
        public void subscribeToBattleStart(string battleId, EventHandler handler);
    }
}
