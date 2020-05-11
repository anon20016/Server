using ServerInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TicTacToyNamespace
{
    public enum Turn
    {
        First = 1,
        Second = 2
    }

    public class TicTacToy : IGame
    {
        private Player[] players = new Player[2];
        public int turn { get; set; }
        public int[,] Map { get; set; } = new int[3, 3];

        public TicTacToy(string x, string y)
        {
            players[0] = new Player(x);
            players[1] = new Player(y);
            turn = 1;
            clear();
        }

        private bool Check(int t)
        {           
            for (int i = 0; i < 3; i++)
            {
                int c = 0;
                int k = 0;
                int p = 0;
                int s = 0;
                for (int j = 0; j < 3; j++)
                {
                    if (Map[i, j] == t)
                    {
                        c++;
                    }
                    if (Map[j, i] == t)
                    {
                        k++;
                    }
                    if (Map[j, j] == t)
                    {
                        p++;
                    }
                    if (Map[j, 3 - j - 1] == t)
                    {
                        s++;
                    }
                }
                if (c == 3 || k == 3 || p == 3 || s == 3)
                {
                    return true;
                }
            }
            return false;
        }
        private void GO()
        {
            if (turn == 1)
            {
                turn = 2;
            }
            else
            {
                turn = 1;
            }
        }
        private void clear()
        {
            for (int i = 0; i < 9; i++)
            {
                Map[i / 3, i % 3] = 0;
            }
        }
        private bool Draw()
        {
            int k = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (Map[i, j] != 0)
                    {
                        k++;
                    }
                }
            }
            return k == 9;
        }

        public int Act(string x, string act)
        {
            int X = 0, Y = 0;
            if (!HasPlayer(x) || GetTurnByName(x) != turn 
                || !Int32.TryParse(act[0].ToString(), out X)
                || !Int32.TryParse(act[2].ToString(), out Y))
            {
                return -1;
            }
            X--;
            Y--;
            if (X > 2 || Y > 2 || Map[X, Y] != 0)
            {
                return -1;
            }
            
            Map[X, Y] = turn;
            players[0].Map = Map;
            players[1].Map = Map;
            if (Check(turn))
            {
                return 1;
            }
            if (Draw())
            {
                return 3;
            }
            GO();
            return 0;
        }

        public string WhosTurn()
        {
            return GetNameByTurn(turn);
        }

        public int[,] GetMap(string x)
        {
            return players[GetTurnByName(x) - 1].Map;
        }

        public bool HasPlayer(string x)
        {
            return (x == players[0].Name) || (x == players[1].Name);
        }

        public void Stop()
        {

        }

        private string GetNameByTurn(int t)
        {
            return players[t - 1].Name;
        }
        private int GetTurnByName(string name)
        {
            if (players[0].Name == name)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        public Regex FormatAct()
        {
            return new Regex(@"\d \d");
        }

        public string GetOpponent(string x)
        {
            if (HasPlayer(x))
            {
                if (players[0].Name == x)
                {
                    return players[1].Name;
                }
                else
                {
                    return players[0].Name;
                }
            }
            return null;
        }

        public string GetMapS(string x)
        {
            return players[GetTurnByName(x) - 1].GetMap();
        }
    }
}
