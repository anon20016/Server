using System;
using System.Net.Sockets;
using System.Net;

namespace Server
{
    public enum Status
    {
        Guest,
        Verified,
        Waiting,
        Playing
    }
    public enum ResponseType
    {
        AuthTrue,
        AuthFalse, 
        StopTrue,
        StopFalse,
        PlayTrue, 
        PlayFalse, 
        GameStart,
        GameStop,
        GameWin, 
        GameLose, 
        GameDraw,
        OpponentDisconnected,
        YourTurn
    }

    class TCPSerever
    {
        public static int Main(String[] args)
        {
            SynchronousSocketListener a = new SynchronousSocketListener("25.150.152.13", 20016);
            a.StartListening();
            return 0;
        }
    }
}