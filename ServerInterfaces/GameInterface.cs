using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ServerInterfaces
{
    public interface IGame
    {       
        /// <summary>
        /// Player's act
        /// </summary>
        /// <param name="x">Player</param>
        /// <param name="act">Act msg</param>
        /// <returns> -1 - no act, 0 - ok, 1 - win, 2 - lose</returns>
        int Act(string name, string act);
        
        string WhosTurn();

        int[,] GetMap(string x);
        string GetMapS(string x);

        bool HasPlayer(string x);
        string GetOpponent(string x);

        void Stop();

        Regex FormatAct();
    }
}
