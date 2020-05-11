using ServerInterfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TicTacToyNamespace;

namespace Server
{
    class ClientService
    {
        const int NUM_OF_THREAD = 10;
        private ClientConnectionPool ConnectionPool;
        private bool ContinueProcess = false;
        private Thread[] ThreadTask = new Thread[NUM_OF_THREAD];

        private List<string> Names;
        private List<ClientHandler> verifiedPool;
        private List<ClientHandler> onlinePool;
        private List<ClientHandler> waitingPool;
        private List<ClientHandler> playingPool;

        public bool HasName(string name)
        {
            return Names.Contains(name);
        }

        public GameHandler games;
        public ClientService(ClientConnectionPool ConnectionPool)
        {
            verifiedPool = new List<ClientHandler>();
            onlinePool = new List<ClientHandler>();
            waitingPool = new List<ClientHandler>();
            playingPool = new List<ClientHandler>();

            this.ConnectionPool = ConnectionPool;
            Names = new List<string>(0);
            games = new GameHandler();
        }

        public void Start()
        {
            ContinueProcess = true;
            // Start threads to handle Client Task
            for (int i = 0; i < ThreadTask.Length; i++)
            {
                ThreadTask[i] = new Thread(new ThreadStart(this.Process));
                ThreadTask[i].Start();
            }
            var FindPlayers = new Thread(new ThreadStart(findPlayers));

            FindPlayers.Start();
        }

        public void BroadcastMsg(string s)
        {
            foreach (var i in onlinePool)
            {
                i.SystemMessage(s);
            }
        }

        private void findPlayers()
        {
            while (true)
            {
                var r = waitingPool.Count;
                lock (waitingPool)
                {
                    if (waitingPool.Count > 1)
                    {
                        var first = waitingPool[0];
                        var second = waitingPool[1];
                        
                        waitingPool.Remove(first);
                        waitingPool.Remove(second);

                        playingPool.Add(first);
                        playingPool.Add(second);

                        games.StartGame(first, second);
                    }
                }
                Thread.Sleep(100);
            }
        }

        private void Process()
        {
            while (ContinueProcess)
            {
                ClientHandler client = null;
                lock (ConnectionPool.SyncRoot)
                {
                    if (ConnectionPool.Count > 0)
                        client = ConnectionPool.Dequeue();
                }
                if (client != null)
                {
                    client.Process();
                    if (client.StatusClient == Status.Waiting && !waitingPool.Contains(client))
                    {
                        waitingPool.Add(client);
                    }
                    if (client.StatusClient != Status.Guest && !verifiedPool.Contains(client))
                    {
                        verifiedPool.Add(client);
                    }
                                         
                    if (client.Alive)
                    {
                        lock (onlinePool)
                        {
                            if (!onlinePool.Contains(client))
                            {
                                onlinePool.Add(client);
                            }
                        }
                        ConnectionPool.Enqueue(client);
                        if (!Names.Contains(client.Name))
                        {
                            Names.Add(client.Name);
                        }
                    }
                    else
                    {       
                        ClientDisconnect(client);
                        Console.WriteLine(client.Name + " disconnected");
                    }
                }

                Thread.Sleep(100);
            }
        }

        private void ClientDisconnect(ClientHandler client)
        {
            if (client.StatusClient == Status.Playing)
            {
                var opp = games.GetOpponent(client.Name);
                lock (opp)
                {
                    lock (playingPool)
                    {
                        playingPool.Remove(client);
                    }
                    lock (games)
                    {
                        games.StopGame(client.Name);
                    }
                    GetClientByName(opp).StopPlayingDisk();
                }
            }

            lock (Names)
            {
                Names.Remove(client.Name);
            }
            lock (verifiedPool)
            {
                verifiedPool.Remove(client);
            }
            lock (onlinePool)
            {
                onlinePool.Remove(client);
            }
            lock (waitingPool)
            {
                waitingPool.Remove(client);
            }
            lock (waitingPool)
            {
                playingPool.Remove(client);
            }
        }

        public void Stop()
        {
            ContinueProcess = false;
            for (int i = 0; i < ThreadTask.Length; i++)
            {
                if (ThreadTask[i] != null && ThreadTask[i].IsAlive)
                    ThreadTask[i].Join();
            }

            // Close all client connections
            while (ConnectionPool.Count > 0)
            {
                ClientHandler client = ConnectionPool.Dequeue();
                client.Close();
                Console.WriteLine("Client connection is closed!");
            }
        }


        private ClientHandler GetClientByName(string x)
        {
            foreach (var i in onlinePool)
            {
                if (i.Name == x)
                {
                    return i;
                }
            }
            return null;
        }
    } // class ClientService

    
}
