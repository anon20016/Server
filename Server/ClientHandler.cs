using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class ClientHandler
    {
        private readonly ClientService service;
        public Status StatusClient { get; set; }
        public string Name { get; set; }

        private TcpClient ClientSocket;
        private NetworkStream networkStream;
        private byte[] bytes;       // Data buffer for incoming data.
        private StringBuilder sb = new StringBuilder(); // Received data string.
        private string data = null; // Incoming data from the client.

        public ClientHandler(TcpClient ClientSocket, ClientService serv )
        {
            StatusClient = Status.Guest;
            Name = null;
            ClientSocket.ReceiveTimeout = 100; // 100 miliseconds
            this.ClientSocket = ClientSocket;
            networkStream = ClientSocket.GetStream();
            bytes = new byte[ClientSocket.ReceiveBufferSize];
            service = serv;
        }

        public void Process()
        {

            try
            {
                int BytesRead = networkStream.Read(bytes, 0, (int)bytes.Length);
                if (BytesRead > 0)
                    // There might be more data, so store the data received so far.
                    sb.Append(Encoding.ASCII.GetString(bytes, 0, BytesRead));
                else
                    // All the data has arrived; put it in response.
                    ProcessDataReceived();

            }
            catch (IOException)
            {
                // All the data has arrived; put it in response.
                ProcessDataReceived();
            }
            catch (SocketException)
            {
                networkStream.Close();
                ClientSocket.Close();
                Console.WriteLine("Conection is broken!");
            }

        }  // Process()

        private void ProcessDataReceived()
        {
            if (sb.Length > 0)
            {
                bool bQuit = (String.Compare(sb.ToString(), "quit", true) == 0);
                bool BAuth = (sb.Length > 4) && (String.Compare(sb.ToString().Substring(0, 4), "auth", true) == 0);
                bool BPlay = (String.Compare(sb.ToString(), "play", true) == 0);
                bool GetStatus = (String.Compare(sb.ToString(), "status", true) == 0);
                bool BStop = (String.Compare(sb.ToString(), "stop", true) == 0);


                data = sb.ToString();


                Console.WriteLine("Text received from client:");
                Console.WriteLine(data);

                StringBuilder response = new StringBuilder();
                //response.Append("Received at ");
                //response.Append(DateTime.Now.ToString());
                // 
                //response.Append(data);                

                // Client stop processing  
                if (BStop)
                {
                    switch (StatusClient)
                    {
                        case Status.Guest:
                            response.Append(ResponseType.StopFalse.ToString());
                            break;
                        case Status.Verified:
                            StatusClient = Status.Waiting;
                            response.Append(ResponseType.StopFalse.ToString());                             
                            break;
                        case Status.Waiting:
                            response.Append(ResponseType.StopTrue.ToString());
                             
                            break;
                        case Status.Playing:
                            response.Append(ResponseType.StopTrue.ToString());                             
                            break;
                        default:
                            break;
                    }

                }
                if (GetStatus)
                {
                    response.Append(StatusClient.ToString());
                     
                }
                if (bQuit)
                {
                    networkStream.Close();
                    ClientSocket.Close();
                }
                if (BAuth)
                {
                    if (sb.Length < 6)
                    {
                        response.Append(ResponseType.AuthFalse.ToString());
                    }
                    else
                    {
                        switch (StatusClient)
                        {
                            case Status.Guest:
                                if (service.HasName(sb.ToString().Substring(5)))
                                {
                                    response.Append(ResponseType.AuthFalse.ToString());
                                }
                                else
                                {
                                    response.Append(ResponseType.AuthTrue.ToString());
                                    Name = sb.ToString().Substring(5);
                                    StatusClient = Status.Verified;
                                }
                                break;
                            case Status.Verified:
                                StatusClient = Status.Waiting;
                                response.Append(ResponseType.AuthFalse.ToString());
                                break;
                            case Status.Waiting:
                                response.Append(ResponseType.AuthFalse.ToString());

                                break;
                            case Status.Playing:
                                response.Append(ResponseType.AuthFalse.ToString());

                                break;
                            default:
                                break;
                        }
                    }
                }
                if (BPlay)
                {
                    switch (StatusClient)
                    {
                        case Status.Guest:
                            response.Append(ResponseType.PlayFalse.ToString());                             
                            break;
                        case Status.Verified:
                            StatusClient = Status.Waiting;
                            response.Append(ResponseType.PlayTrue.ToString());                             
                            break;
                        case Status.Waiting:
                            response.Append(ResponseType.PlayFalse.ToString());                             
                            break;
                        case Status.Playing:
                            response.Append(ResponseType.PlayFalse.ToString());                             
                            break;
                        default:
                            break;
                    }
                    
                }
               
                if (response.Length == 0)
                {
                    response.Append(data);
                }
                sb.Length = 0;

                byte[] sendBytes = Encoding.ASCII.GetBytes(response.ToString());
                networkStream.Write(sendBytes, 0, sendBytes.Length);
            }
        }

        public void StartPlaying(string nameOpponent)
        {
            try
            {
                StringBuilder response = new StringBuilder();
                response.Append("Start playing with ");
                response.Append(nameOpponent);
                StatusClient = Status.Playing;
                byte[] sendBytes = Encoding.ASCII.GetBytes(response.ToString());
                networkStream.Write(sendBytes, 0, sendBytes.Length);
            } 
            catch (IOException)
            {
                Console.WriteLine("IOError");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public void StopPlaying(string msg)
        {
            try
            {
                StringBuilder response = new StringBuilder();
                response.Append("Game ended ");
                response.Append(msg);
                StatusClient = Status.Verified;
                byte[] sendBytes = Encoding.ASCII.GetBytes(response.ToString());
                networkStream.Write(sendBytes, 0, sendBytes.Length);
            }
            catch (IOException)
            {
                Console.WriteLine("IOError");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Close()
        {
            networkStream.Close();
            ClientSocket.Close();
        }

        public bool Alive
        {
            get
            {
                return ClientSocket.Connected;
            }
        }

    }
}
