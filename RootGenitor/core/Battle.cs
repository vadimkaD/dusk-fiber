using System;
using System.Collections.Generic;
using System.Text;

namespace RootGenitor.core
{
    class Battle
    {
        public bool addPlayer(Player player)
        {
            return false;
        }

        public event EventHandler BattleStarted = delegate { };

        internal void OnBattleStarted(EventArgs e)
        {
            var handler = BattleStarted;
            handler?.Invoke(this, e);
        }
    }
}
