using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToyNamespace
{
    class Player : IPlayer
    {
        public Player(string name)
        {
            Name = name;
            Score = 0;
            Clear();
        }
        public string Name { get; set; }
        public int Score { get; set; }
        public int[,] Map { get; set; } = new int[3, 3];

        private void Clear()
        {
            for (int i = 0; i < 9; i++)
            {
                Map[i / 3, i % 3] = 0;
            }
        }

        public string GetMap()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    switch (Map[i, j])
                    {
                        case 1:
                            sb.Append('X');
                            break;
                        case 2:
                            sb.Append('0');
                            break;
                        default:
                            sb.Append('.');
                            break;
                    }
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }
        
    }
}
