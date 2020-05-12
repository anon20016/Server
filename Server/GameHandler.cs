using ServerInterfaces;
using System;
using System.Collections.Generic;
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
                if (i.Value.Item1.Name == x || i.Value.Item2.Name == x)
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
                fr.Turn();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void StopGame(string x)
        {
            StopGame(GetGame(x));
        }
        private void StopGame(IGame x)
        {
            list.Remove(x);
        }

        public ClientHandler GetPlayer(string x)
        {
            foreach (var i in list)
            {
                if (i.Value.Item1.Name == x)
                {
                    return i.Value.Item1;
                }
                if (i.Value.Item2.Name == x)
                {
                    return i.Value.Item2;
                }
            }
            return null;
        }
        public string GetOpponent(string x)
        {            
            return GetOpponent(GetPlayer(x)).Name;
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


        public bool HasPlayer(string name)
        {
            return GetPlayer(name) != null;
        }
        public bool CheckFormat(string name, string msg)
        {
            return GetGame(name).FormatAct().IsMatch(msg);
        }

        public void Act(ClientHandler client, string msg)
        {

            var name = client.Name;
            var game = GetGame(name);
            if (game == null || !CheckFormat(name, msg))
            {
                return;
            }
            var opp = GetOpponent(client);
            switch (game.Act(name, msg))
            {
                case -1:
                    client.SendMessage("Invalid input");
                    break;
                case 0:
                    client.SendMessage(game.GetMapS(name));
                    opp.SendMessage(game.GetMapS(opp.Name));
                    opp.Turn();
                    break;
                case 1:
                    client.GameWin(opp.Name, 0);
                    opp.GameLose(client.Name, 0);
                    StopGame(game);
                    break;
                case 2:
                    client.GameLose(opp.Name, 0);
                    opp.GameWin(client.Name, 0);
                    StopGame(game);
                    break;
                case 3:
                    client.GameDraw(opp.Name, 0);
                    opp.GameDraw(client.Name, 0);
                    StopGame(game);
                    break;
                default:
                    break;
            }
        }
    }
}
