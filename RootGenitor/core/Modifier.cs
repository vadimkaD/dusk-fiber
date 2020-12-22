using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RootGenitor.core
{
    public abstract class Modifier
    {
        public static ulong operator &(Modifier A, Modifier B)
        {
            return A.flag & B.flag;
        }

        public static ulong operator |(Modifier A, Modifier B)
        {
            return A.flag | B.flag;
        }

        public readonly ulong flag;
        private readonly string name;
        public Modifier(ulong flag, string name)
        {
            this.flag = flag;
            this.name = name;
        }

        public override string ToString()
        {
            return this.name;
        }

        public static string GetNames(ulong value)
        {
            Type t = typeof(UnitModifier);
            List<string> total = new List<string>();
            foreach (FieldInfo m in t.GetFields())
            {

                if (m.FieldType.Equals(typeof(UnitModifier)))
                {
                    UnitModifier modifier = (UnitModifier)m.GetValue(null);
                    bool contains = Convert.ToBoolean(modifier.flag & value);
                    if (contains)
                    {
                        total.Add(m.Name);
                    }
                }

            }
            return String.Join(", ", total);
        }
    }
}
