using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToy
{
    class Player : PlayerInterface
    {
        public Player(string name)
        {
            Name = name;
            Score = 0;
            clear();
        }
        public string Name { get; set; }
        public int Score { get; set; }
        public int[,] Map { get; set; } = new int[3, 3];

        private void clear()
        {
            for (int i = 0; i < 9; i++)
            {
                Map[i / 3, i % 3] = 0;
            }
        }
    }
}
