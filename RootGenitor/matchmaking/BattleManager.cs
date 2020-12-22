using RootGenitor.mobile_core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RootGenitor.matchmaking
{
    class BattleManager
    {
        readonly ConcurrentDictionary<string, MagicBattle> battlesByToken;
        public BattleManager()
        {
            battlesByToken = new ConcurrentDictionary<string, MagicBattle>();
        }
        public bool inBattle(string token)
        {
            return battlesByToken.ContainsKey(token);
        }

        public void addBattle(string token, MagicBattle battle)
        {
            battlesByToken.TryAdd(token, battle);
        }

        public MagicBattle getByToken(string token)
        {
            return battlesByToken[token];
        }

        public void cancelBattle(string token)
        {
            var battle = getByToken(token);
            var tokens = battle.getTokens();
            battle.cancel();
            foreach (var innerToken in tokens)
            {
                battlesByToken.TryRemove(innerToken, out MagicBattle absent);
            }
        }
    }
}
