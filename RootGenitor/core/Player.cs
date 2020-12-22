using RootGenitor.core.spells;
using System;
using System.Collections.Generic;
using System.Text;

namespace RootGenitor.core
{   
    class PlayerData
    {
        public int MaxHp 
        {
            set; get;
        }
        public int Hp
        {
            set; get;
        }
        public bool IsAlive
        {
            set; get;
        }
    }
    class Player
    {
        public PlayerData data;
        Dictionary<int, Spell> slots;
        private readonly string token;
        public Player(string token)
        {
            data = new PlayerData();
            slots = new Dictionary<int, Spell>();
            this.token = token;
        }
        public void updateInfo()
        {
            data.MaxHp = 42;
            data.Hp = 42;
            data.IsAlive = true;
            slots[0] = new Sparks();
        }

        internal SpellResult cast(int slot, Player opponent)
        {
            Spell spell = slots[slot];
            return spell.cast(this, opponent);
        }
    }
}
