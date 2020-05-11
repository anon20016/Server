using ServerInterfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TicTacToyNamespace;

namespace Server
{
    class GameHandler
    {
        const int N = 5;

        Dictionary<IGame, Tuple<ClientHandler, ClientHandler>> list;

        private IGame GetGame(string x)
        {
            foreach (var i in list)
            {
                if (i.Key.HasPlayer(x))
                {
                    return i.Key;
                }
            }
            return null;
        }

        public GameHandler()
        {
            list = new Dictionary<IGame, Tuple<ClientHandler, ClientHandler>>();
        }

        public bool StartGame(ClientHandler fr, ClientHandler sc)
        {
            if (list.Count < N)
            {
                list.Add(new TicTacToy(fr.Name, sc.Name), new Tuple<ClientHandler, ClientHandler>(fr, sc));
                lock (fr)
                {
                    fr.StartPlaying(sc.Name, 0);
                }
                lock (sc)
                {
                    sc.StartPlaying(fr.Name, 0);
                }
                fr.SystemMessage("Your Turn");
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool StopGame(string x)
        {
            foreach (var item in list)
            {
                if (item.Key.HasPlayer(x))
                {
                    lock (item.Value.Item1)
                    {
                        item.Value.Item1.StopPlaying(item.Value.Item2.Name, 0);
                    }
                    lock (item.Value.Item2)
                    {
                        item.Value.Item2.StopPlaying(item.Value.Item1.Name, 0);
                    }
                    list.Remove(item.Key);
                    return true;
                }
            }
            return false;
        }
        private void StopGame(IGame x)
        {
            list.Remove(x);
        }

        public bool HasPlayer(string x)
        {
            foreach (var i in list)
            {
                if (i.Key.HasPlayer(x))
                {
                    return true;
                }
            }
            return false;
        }
        public string GetOpponent(string x)
        {
            foreach (var i in list)
            {
                var opp = i.Key.GetOpponent(x);
                if (opp != null)
                {
                    return opp;
                }
            }
            return null;
        }
        public ClientHandler GetOpponent(ClientHandler x)
        {
            foreach (var i in list)
            {
                if (i.Value.Item1 == x)
                {
                    return i.Value.Item2;
                }
                if (i.Value.Item2 == x)
                {
                    return i.Value.Item1;
                }
            }
            return null;
        }

        public bool CheckFormat(string name, string msg)
        {
            return GetGame(name).FormatAct().IsMatch(msg);
        }

        public void Act(ClientHandler client, string msg)
        {
            var name = client.Name;
            var game = GetGame(name);
            var opp = GetOpponent(client);
            switch (game.Act(name, msg))
            {
                case -1:
                    client.SendMessage("Invalid input");
                    break;
                case 0:
                    client.SendMessage(game.GetMapS(name));
                    opp.SendMessage(game.GetMapS(opp.Name));
                    opp.SendMessage("Your turn");
                    break;
                case 1:
                    client.SendMessage("You win!!!");
                    client.SendMessage(ResponseType.GameWin.ToString());
                    opp.SendMessage(ResponseType.GameLose.ToString());
                    opp.SendMessage("You lose :(");
                    opp.StopPlaying(name, 0);
                    client.StopPlaying(GetOpponent(name), 0);
                    StopGame(game);
                    break;
                case 2:
                    client.SendMessage("You lose!!!");
                    client.SendMessage(ResponseType.GameLose.ToString());
                    opp.SendMessage(ResponseType.GameWin.ToString());
                    opp.SendMessage("You win!!!");
                    opp.StopPlaying(name, 0);
                    client.StopPlaying(opp.Name, 0);
                    StopGame(game);

                    break;
                case 3:
                    client.SendMessage("It's a draw!");
                    opp.SendMessage("It's a draw!");
                    client.StopPlaying(opp.Name, 0);
                    opp.StopPlaying(name, 0);

                    opp.SendMessage(ResponseType.GameStop.ToString());
                    client.SendMessage(ResponseType.GameStop.ToString());

                    StopGame(game);

                    break;
                default:
                    break;
            }
        }
    }
}
