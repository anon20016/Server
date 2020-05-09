using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Server
{
    class ClientService
    {
        const int NUM_OF_THREAD = 10;

        private ClientConnectionPool ConnectionPool;
        private bool ContinueProcess = false;
        private Thread[] ThreadTask = new Thread[NUM_OF_THREAD];
        private ClientHandler waiting;
        private List<string> Names;
        private List<Tuple<ClientHandler, ClientHandler>> games;

        public bool HasName(string name)
        {
            return Names.Contains(name);
        }

        public ClientService(ClientConnectionPool ConnectionPool)
        {
            this.ConnectionPool = ConnectionPool;
            waiting = null;
            Names = new List<string>(0);
            games = new List<Tuple<ClientHandler, ClientHandler>>();
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
                    
                    if (client.StatusClient == Status.Waiting)
                    {
                        if (waiting != null) {
                            if (!waiting.Alive)
                            {
                                waiting = null;
                            }
                            else
                            {
                                if (client.Alive && client.Name != waiting.Name)
                                {
                                    client.StartPlaying(waiting.Name);
                                    waiting.StartPlaying(client.Name);
                                    games.Add(new Tuple<ClientHandler, ClientHandler>(waiting, client));
                                    waiting = null;
                                }
                            }
                        }
                        else
                        {
                            waiting = client;
                        }                    
                    }
                    if (client.Alive)
                    {
                        ConnectionPool.Enqueue(client);
                        if (!Names.Contains(client.Name))
                        {
                            Names.Add(client.Name);
                        }
                    }
                    else
                    {
                        if (client.StatusClient == Status.Playing)
                        {
                            var opp = GetOpponent(client);
                            games.Remove(new Tuple<ClientHandler, ClientHandler>(opp, client));
                            games.Remove(new Tuple<ClientHandler, ClientHandler>(client, opp));
                            opp.StopPlaying("Opponent has left the game");
                        }
                        Console.WriteLine(client.Name + " disconnected");
                        Names.Remove(client.Name);
                    }
                }

                Thread.Sleep(100);
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

        private ClientHandler GetOpponent(ClientHandler s)
        {
            foreach (var item in games)
            {
                if (item.Item1 == s)
                {
                    return item.Item2;
                }
                if (item.Item2 == s)
                {
                    return item.Item1;
                }
            }
            return null;
        }
    } // class ClientService
}
