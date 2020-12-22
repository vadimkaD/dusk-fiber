using System;
using System.Collections.Generic;
using System.Text;

namespace RootGenitor.core
{
    class Unit
    {
        private string id;
        public int maxHp;
        public int currentHp;
        public int maxFatigue;
        public int currentFatigue;
        public float damage;
        public bool occupiesHex;
        public bool isAlive;
        public List<UnitModifier> unitModifiers;

    }
}
