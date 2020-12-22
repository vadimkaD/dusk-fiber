using RootGenitor.api;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RootGenitor.core
{
    class BattleStartedArgs : EventArgs
    {
        public string battleId;
    }
    class BattleController: BattleApi
    {
        public ConcurrentDictionary<string, Battle> battles = new ConcurrentDictionary<string, Battle>();

        public void registerInBattle(Player player, string battleId)
        {
            Battle battle;
            if (battles.ContainsKey(battleId))
            {
                battles.TryGetValue(battleId, out battle);
            }
            else
            {
                battle = new Battle();
                battles.TryAdd(battleId, battle);
            }

            battle.addPlayer(player);
        }

        public void callBattleStart(string battleId)
        {
            onBattleStart(battleId);
        }
        
        private void onBattleStart(string battleId)
        {
            Battle battle;
            battles.TryGetValue(battleId, out battle);
            if (battle != null)
            {
                battle.OnBattleStarted(new EventArgs());
            }
           
            
        }
        public void subscribeToBattleStart(string battleId, EventHandler handler)
        {
            Battle battle;
            battles.TryGetValue(battleId, out battle);
            if (battle != null)
            {
                battle.BattleStarted += handler;
            }
        }
    }
}
