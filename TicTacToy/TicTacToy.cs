using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToy
{
    public class TicTacToy
    {
        public enum Turn
        {
            First = 1,
            Second = 2
        }

        private Player[] players = new Player[2];
        public Turn turn { get; set; }
        public int[,] Map { get; set; } = new int[3, 3];

        public TicTacToy(string x, string y)
        {
            players[0] = new Player(x);
            players[1] = new Player(y);
            turn = 0;
            clear();            
        }

        public int Act(int x, int y)
        {
            if (Map[x, y] != 0)
            {
                return -1;
            }
            switch (turn)
            {
                case Turn.First:
                    Map[x, y] = '1';
                    players[0].Map = Map;
                    break;
                case Turn.Second:
                    Map[x, y] = '2';
                    players[1].Map = Map;
                    break;
                default:
                    break;
            }

            if (check(turn))
            {
                return (int)turn;
            }
            GO();
            return 0;
        }

        public int whosTurn()
        {
            return (int)turn;
        }

        private bool check(Turn t)
        {
            int x = 1;
            if (t == Turn.Second)
            {
                x = 2;
            }
            for (int i = 0; i < 3; i++)
            {
                int c = 0;
                int k = 0;
                int p = 0;
                int s = 0;
                for (int j = 0; j < 3; j++)
                {
                    if (Map[i, j] == x)
                    {
                        c++;
                    }
                    if (Map[j, i] == x)
                    {
                        k++;
                    }
                    if (Map[j, j] == x)
                    {
                        p++;
                    }
                    if (Map[j, 3 - j - 1] == x)
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
            if (turn == Turn.First)
            {
                turn = Turn.Second;
            }
            else
            {
                turn = Turn.First;
            }
        }
        private void clear()
        {
            for (int i = 0; i < 9; i++)
            {
                Map[i / 3, i % 3] = 0;
            }
        }
    }
}
