using RootGenitor.core;
using RootGenitor.matchmaking;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RootGenitor
{
    class Program
    {

        static void Main(string[] args)
        {
            UnitModifier a = UnitModifier.BLESS;
            UnitModifier b = UnitModifier.CURSE;
            string names = UnitModifier.GetNames(a | b);
            Console.WriteLine(names);
            var battlefield = new Battlefield();
            
            for (var i = 0; i < 10; i++)
            {
                for (var j = 0; j < 10; j++)
                {
                    var hex = new Hex(true);
                    var point = new Point(i, j);
                    battlefield.registerHex(point, hex);
                }
            }

            string ip = "0.0.0.0";
            int port = 80;

            WSServer server = new WSServer();

            Console.WriteLine("run server...");
            server.serve(ip, port);
        }

        private static void p(object sender, EventArgs e)
        {
            Console.WriteLine("ta-da called");
        }
    }
}
