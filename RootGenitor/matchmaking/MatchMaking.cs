using RootGenitor.core;
using RootGenitor.datasource;
using RootGenitor.mobile_core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RootGenitor.matchmaking
{
    class MatchMaking
    {
        public const string IDENTIFY = "IDENTIFY";
        public const string REG = "REG";
        public const string CONFIRM = "CONFIRM";
        public const string USE_SLOT = "USE_SLOT";
        public const string CANCEL = "CANCEL";


        static readonly object queueLock;
        public static WSServer server;
        static readonly ConcurrentQueue<string> q;
        static readonly ConcurrentDictionary<string, byte> candidates;
        static readonly BattleManager battleManager;
        static MatchMaking()
        {
            queueLock = new object();
            q = new ConcurrentQueue<string>();
            candidates = new ConcurrentDictionary<string, byte>();
            battleManager = new BattleManager();
            var task = Task.Run(() =>
            {
                runMatchMaking();
            });
        }

        public static void Identify(DataSource dataSource, string[] lines, NetworkStream n)
        {
            string token = lines[1];
            Console.WriteLine("IDENTIFY: {0}", token);
            bool valid = dataSource.isTokenValid(token);
            if (!valid) return;
            server.manager.addClient(token, n);
            Console.WriteLine("IDENTIFY OK: {0}", token);
            server.sendMessage(n, "IDENTIFY_OK");
            if (battleManager.inBattle(token))
            {
                //TODO: join battle
            }
            return;
        }

        public static void Reg(NetworkStream n)
        {
            string token = server.manager.getTokenByStream(n);
            Console.WriteLine("REG: {0}", token);
            MatchMaking.register(token);
            Console.WriteLine("REG OK: {0}", token);
            server.sendMessage(n, "REGISTER_OK");
            return;
        }

        public static void Confirm(NetworkStream n)
        {
            string token = server.manager.getTokenByStream(n);
            Console.WriteLine("CONFIRM: {0}", token);
            MatchMaking.confirm(token);
            Console.WriteLine("START BATTLE: {0}", token);
            return;
        }

        public static void UseSlot(string[] lines, NetworkStream n)
        {
            string token = server.manager.getTokenByStream(n);
            Console.WriteLine("USE_SLOT: {0}", token);
            int slot = Int32.Parse(lines[1]);
            MatchMaking.useSlot(token, slot);
            Console.WriteLine("USE_SLOT: {0} {1}", token, slot.ToString());
        }

        public static void Cancel(NetworkStream n)
        {
            string token = server.manager.getTokenByStream(n);
            battleManager.cancelBattle(token);
        }

        public static void processTextFrame(string text, NetworkStream n)
        {
            Console.WriteLine("text: {0}", text);
            string[] lines = text.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

            string instruction = lines[0];
            var dataSource = new DataSource();

            switch (instruction)
            {
                case IDENTIFY:
                    {
                        MatchMaking.Identify(dataSource, lines, n);
                        return;
                    }

                case REG:
                    {
                        MatchMaking.Reg(n);
                        return;
                    }

                case CONFIRM:
                    {
                        MatchMaking.Confirm(n);
                        return;
                    }

                case USE_SLOT:
                    {
                        MatchMaking.UseSlot(lines, n);
                        return;
                    }
                case CANCEL:
                    {
                        MatchMaking.Cancel(n);
                        return;
                    }
                default:
                    return;
            }
        }
        public static bool inBattle(string token)
        {
            return battleManager.inBattle(token);
        }
        public static void unreg(string token)
        {
            lock (queueLock)
            {
                Console.WriteLine("unreg {0}", token);
                candidates.TryRemove(token, out byte absent);
                Console.WriteLine("candidates.Count {0}", candidates.Count);
            }
        }
        private static void runMatchMaking()
        {
            Console.WriteLine("MM is running...");
            string token;

            while (true)
            {
                lock (queueLock)
                {
                    if (!q.TryDequeue(out token))
                    {
                        Console.WriteLine("No object to dequeue, waiting...");
                        Monitor.Wait(queueLock);
                        Console.WriteLine("Monitor signals...");
                        q.TryDequeue(out token);
                    }

                    if (token != null)
                    {
                        Console.WriteLine("add token {0}", token);

                        bool ok = candidates.TryAdd(token, 1);

                        Console.WriteLine("candidates.Count {0}", candidates.Count.ToString());

                        if (candidates.Count > 1)
                        {
                            var i = 0;
                            Console.WriteLine("candidates Count b4: {0}", candidates.Count.ToString());
                            var battle = new MagicBattle(server);
                            foreach (KeyValuePair<string, byte> entry in candidates)
                            {
                                // do something with entry.Value or entry.Key
                                Console.WriteLine("candidate: {0}", entry.Key);
                                candidates.TryRemove(entry.Key, out byte absent);
                                battle.regPlayer(new Player(entry.Key), entry.Key);
                                battleManager.addBattle(entry.Key, battle);
                                i += 1;

                                if (i > 1)
                                {
                                    i = 0;
                                    break;
                                }
                            }

                            Console.WriteLine("candidates Count after: {0}", candidates.Count.ToString());
                        }
                    }
                }
            }
        }
        static void QueueMessage(string token)
        {
            lock (queueLock)
            {
                q.Enqueue(token);
                Monitor.Pulse(queueLock);
            }
        }
        public static void register(string token)
        {
            QueueMessage(token);
        }
        public static void confirm(string token)
        {
            MagicBattle battle = battleManager.getByToken(token);
            battle.confirm(token);
        }
        public static void useSlot(string token, int slot)
        {
            MagicBattle battle = battleManager.getByToken(token);
            battle.useSlot(token, slot);
        }
    }
}
