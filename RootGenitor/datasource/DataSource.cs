using System;
using System.Collections.Generic;
using System.Text;

namespace RootGenitor.datasource
{
    class DataSource
    {
        public bool isTokenValid(string token)
        {
            return token.Length > 0;
        }

        public string getProfile(string token)
        {
            return "junkee";
        }
    }
}
