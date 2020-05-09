using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class SynchronousSocketListener
    {
        private readonly string adress = "25.150.152.13";
        private readonly int portNum = 20016;

        public SynchronousSocketListener(string adr, int port)
        {
            adress = adr;
            portNum = port;
        }        

        public void StartListening()
        {

            ClientService ClientTask;
            // Client Connections Pool
            ClientConnectionPool ConnectionPool = new ClientConnectionPool();

            // Client Task to handle client requests
            ClientTask = new ClientService(ConnectionPool);

            ClientTask.Start();

            TcpListener listener = new TcpListener(IPAddress.Parse(adress), portNum);
            try
            {
                listener.Start();

                int ClientNbr = 0;

                // Start listening for connections.
                Console.WriteLine("Waiting for a connection...");
                while (true)
                {
                    TcpClient handler = listener.AcceptTcpClient();
                    if (handler != null)
                    {
                        Console.WriteLine("Client#{0} accepted!", ++ClientNbr);
                        // An incoming connection needs to be processed.
                        ConnectionPool.Enqueue(new ClientHandler(handler, ClientTask));
                    }
                    else
                        break;
                }
                listener.Stop();
                ClientTask.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }
}
