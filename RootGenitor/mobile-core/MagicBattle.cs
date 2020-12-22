using RootGenitor.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RootGenitor.mobile_core
{
    public enum Status
    {
        BeforeFight,
        Confirmation,
        Fight,
        Finished
    }

    class TurnResult
    {
        PlayerData yours;
        PlayerData opponents;
        int stepNumber;
        public PlayerData Yours { get => yours; set => yours = value; }
        public PlayerData Opponents { get => opponents; set => opponents = value; }
        public int StepNumber { get => stepNumber; set => stepNumber = value; }
    }

    class MagicBattle
    {
        private static object turnLock = new object();
        private readonly Dictionary<string, Player> playersByToken;
        private readonly Dictionary<Player, bool> confirmed;
        
        private readonly int maxPlayers;
        private int stepNumber;
        private string[] turnQueue;
        private string currentPlayerToken;

        private Status status;

        private readonly System.Timers.Timer turnTimer;
        private readonly static int TURN_MAX_TIME = 20000;

        readonly WSServer server;
        public MagicBattle (WSServer server, int maxPlayers = 2)
        {
            playersByToken = new Dictionary<string, Player>();
            confirmed = new Dictionary<Player, bool>();
            this.server = server;
            this.maxPlayers = maxPlayers;
            status = Status.BeforeFight;
            stepNumber = 1;
            turnTimer = new System.Timers.Timer(TURN_MAX_TIME);
            turnTimer.Elapsed += TurnElapsed;
        }

        public void cancel()
        {
            foreach (string token in playersByToken.Keys)
            {
                server.trySendMessage(token, "CANCEL_OK");
            }
        }

        public string[] getTokens()
        {
            List<string> list = new List<string>();
            foreach (string token in playersByToken.Keys)
            {
                list.Add(token);
            }

            return list.ToArray();
        }
        public void regPlayer(Player player, string token)
        {
            playersByToken.Add(token, player);
            if (playersByToken.Count >= maxPlayers)
            {
                status = Status.Confirmation;
                askConfirmations();
            }
        }


        private void askConfirmations()
        {
            foreach (string token in playersByToken.Keys)
            {
                server.trySendMessage(token, "CONFIRMATION_NEEDED");
            }
        }

        public void confirm(string token)
        {
            Player player = playersByToken[token];
            confirmed[player] = true;
            if (confirmed.Count == maxPlayers)
            {
                var count = 0;
                foreach(bool val in confirmed.Values.ToList())
                {
                    if (val) count += 1;
                }
                if (count == maxPlayers)
                {
                    start();
                }
            }
        }

        private void killThoseWhoMustDie()
        {
            foreach (string token1 in playersByToken.Keys)
            {
                Player player = playersByToken[token1];
                if (player.data.Hp <= 0)
                {
                    player.data.IsAlive = false;
                }
            }
        }

        private string[] getDeadList()
        {
            List<string> total = new List<string>();
            foreach (string token1 in playersByToken.Keys)
            {
                Player player = playersByToken[token1];
                if (!player.data.IsAlive) total.Add(token1);
            }
            return total.ToArray();
        }


        public void useSlot(string token, int slot)
        {
            if (token != currentPlayerToken) return;

            lock (turnLock)
            {
                turnTimer.Stop();
                Player player = playersByToken[token];
                foreach (string token1 in playersByToken.Keys)
                {
                    if (token1 != token)
                    {
                        Player opponent = playersByToken[token1];
                        SpellResult result = player.cast(slot, opponent);
                        player.data = result.ownerData;
                        opponent.data = result.opponentData;
                        killThoseWhoMustDie();
                        string[] deadList = getDeadList();
                        if (deadList.Count() > 0)
                        {
                            status = Status.Finished;
                            bool draw = deadList.Count() > 1;
                            if (draw)
                            {
                                for (var i = 0; i < deadList.Count(); i++)
                                {
                                    server.trySendMessage(deadList[i], "BATTLE_END\r\nDRAW");
                                }
                                Monitor.Pulse(turnLock);
                            }
                            else
                            {
                                foreach (string token2 in playersByToken.Keys)
                                {
                                    string message = deadList.Contains(token2) ? "BATTLE_END\r\nLOSE" : "BATTLE_END\r\nWIN";
                                    server.trySendMessage(token2, message);
                                }
                                Monitor.Pulse(turnLock);
                            }
                        }

                        else
                        {
                            finishTurn();
                            informResult();
                            Monitor.Pulse(turnLock);
                        }
                        break;
                    }
                }
            }
        }

        private void finishTurn()
        {
            lock (turnLock)
            {
                stepNumber += 1;
                int index = Array.IndexOf(turnQueue, currentPlayerToken);
                index = index >= turnQueue.Count() - 1 ? 0 : index + 1;
                currentPlayerToken = turnQueue[index];
            }
        }

        private void informResult()
        {
            lock (turnLock)
            {
                PlayerData player1data = new PlayerData();
                player1data.MaxHp = 0;
                string player1Token = "";
                PlayerData player2data = new PlayerData();
                player2data.MaxHp = 0;
                foreach (string token in playersByToken.Keys)
                {
                    if (player1data.MaxHp == 0)
                    {
                        player1data = playersByToken[token].data;
                        player1Token = token;
                    }

                    else
                    {
                        player2data = playersByToken[token].data;
                    }
                }

                foreach (string token in playersByToken.Keys)
                {
                    TurnResult result = new TurnResult();
                    result.StepNumber = stepNumber - 1;
                    if (player1Token == token)
                    {
                        result.Yours = player1data;
                        result.Opponents = player2data;
                        string msg = "TURN_RESULT\r\n" + JsonSerializer.Serialize(result);
                        server.trySendMessage(token, msg);
                    }

                    else
                    {
                        result.Yours = player2data;
                        result.Opponents = player1data;
                        string msg = "TURN_RESULT\r\n" + JsonSerializer.Serialize(result);
                        server.trySendMessage(token, msg);
                    }
                }

            }
        }
        private void TurnElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("Turn Elapsed for {0}", currentPlayerToken.ToString());
            useSlot(currentPlayerToken, 0);
        }
        private void start()
        {
            foreach (string token in playersByToken.Keys)
            {
                Player player = playersByToken[token];
                player.updateInfo();
               
            }
            informResult();
            foreach (string token in playersByToken.Keys)
            {
                server.trySendMessage(token, "START");

            }
            status = Status.Fight;
            Random rnd = new Random();
            turnQueue = playersByToken.Keys.ToList().OrderBy(x => rnd.Next()).ToArray();
            Task.Run(() =>
            {
                currentPlayerToken = turnQueue.First();
                while(true)
                {
                    lock (turnLock)
                    {
                        foreach (string token in playersByToken.Keys)
                        {
                            if (token == currentPlayerToken)
                            {
                                server.trySendMessage(token, "YOUR_TURN");
                            }
                            else
                            {
                                server.trySendMessage(token, "WAIT_FOR_TURN");
                            }
                        }

                        turnTimer.Start();
                        Monitor.Wait(turnLock);
                        if (status == Status.Finished)
                        {
                            Console.WriteLine("Battle end");
                            break;
                        }

                    }
                }
            });
        }
    }
    
}
