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
        private byte[] bytes;                           // Data buffer for incoming data.
        private StringBuilder sb = new StringBuilder(); // Received data string.
        private GameHandler game;

        public ClientHandler(TcpClient ClientSocket, ClientService serv)
        {
            StatusClient = Status.Guest;
            Name = "Guest" + (new Random()).Next(1000).ToString();
            ClientSocket.ReceiveTimeout = 100; // 100 miliseconds
            this.ClientSocket = ClientSocket;
            networkStream = ClientSocket.GetStream();
            bytes = new byte[ClientSocket.ReceiveBufferSize];
            service = serv;
            game = serv.games;
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

        private void SendClient(StringBuilder response)
        {
            if (response.Length == 0)
            {
                response.Append("None");
            }
            byte[] sendBytes = Encoding.ASCII.GetBytes(response.ToString());
            networkStream.Write(sendBytes, 0, sendBytes.Length);            
        }
        private void SendClient(string response)
        {
            if (response.Length == 0)
            {
                response = "None";
            }
            byte[] sendBytes = Encoding.ASCII.GetBytes(response);
            networkStream.Write(sendBytes, 0, sendBytes.Length);
        }

        // handling comands
        private void CommandQuit()
        {
            Close();
        }
        private void CommandStatus()
        {
            SendMessage(StatusClient.ToString());
        }
        private void CommandAuth()
        {
            if (sb.Length < 6)
            {
                SendMessage(ResponseType.AuthFalse.ToString());
            }
            else
            {
                switch (StatusClient)
                {
                    case Status.Guest:
                        if (service.HasName(sb.ToString().Substring(5)))
                        {
                            SendMessage(ResponseType.AuthFalse.ToString());
                        }
                        else
                        {
                            SendMessage(ResponseType.AuthTrue.ToString());
                            Name = sb.ToString().Substring(5);
                            StatusClient = Status.Verified;
                        }
                        break;
                    case Status.Verified:
                        StatusClient = Status.Waiting;
                        SendMessage(ResponseType.AuthFalse.ToString());
                        break;
                    case Status.Waiting:
                        SendMessage(ResponseType.AuthFalse.ToString());
                        break;
                    case Status.Playing:
                        SendMessage(ResponseType.AuthFalse.ToString());
                        break;
                    default:
                        break;
                }
            }
        }
        private void CommandStop()
        {
            switch (StatusClient)
            {
                case Status.Guest:
                    SendMessage(ResponseType.StopFalse.ToString());
                    break;
                case Status.Verified:
                    StatusClient = Status.Waiting;
                    SendMessage(ResponseType.StopFalse.ToString());
                    break;
                case Status.Waiting:
                    SendMessage(ResponseType.StopTrue.ToString());

                    break;
                case Status.Playing:
                    SendMessage(ResponseType.StopTrue.ToString());
                    break;
                default:
                    break;
            }
        }
        private void CommandPlay()
        {
            switch (StatusClient)
            {
                case Status.Guest:
                    SendMessage(ResponseType.PlayFalse.ToString());
                    break;
                case Status.Verified:
                    StatusClient = Status.Waiting;
                    SendMessage(ResponseType.PlayTrue.ToString());
                    break;
                case Status.Waiting:
                    SendMessage(ResponseType.PlayFalse.ToString());
                    break;
                case Status.Playing:
                    SendMessage(ResponseType.PlayFalse.ToString());
                    break;
                default:
                    break;
            }
        }
        //

        private void ProcessDataReceived()
        {
            if (sb.Length > 0)
            {
                StringBuilder response = new StringBuilder();

                if (String.Compare(sb.ToString(), "quit", true) == 0)
                {
                    CommandQuit();
                } //bQuit
                if ((sb.Length > 4) && (String.Compare(sb.ToString().Substring(0, 4), "auth", true) == 0)){
                    CommandAuth();
                } //BAuth
                if (String.Compare(sb.ToString(), "play", true) == 0)
                {
                    CommandPlay();
                } //BPlay
                if (String.Compare(sb.ToString(), "status", true) == 0)
                {
                    CommandStatus();
                } //GetStatus
                if (String.Compare(sb.ToString(), "stop", true) == 0)
                {
                    CommandStop();
                } //BStop
                if (game.HasPlayer(Name))
                {
                    if (game.CheckFormat(Name, sb.ToString()))
                    {
                        game.Act(this, sb.ToString());                        
                    }
                }
                                
                Console.WriteLine("{0} : {1}", Name, sb.ToString());

                sb.Length = 0;
            }
        }

        public void StartPlaying(string nameOpponent, int idGame)
        {
            SendMessage("Start playing with " + nameOpponent);
            SendMessage(ResponseType.GameStart.ToString());
            StatusClient = Status.Playing;
        }
        public void StopPlaying(string nameOpponent, int idGame)
        {
            SendMessage("Game ended");
            SendMessage(ResponseType.GameStop.ToString());

            StatusClient = Status.Verified;
        }

        public void StopPlayingDisk()
        {
            try
            {
                StatusClient = Status.Verified;
                SendClient("Opponent disconected :(");
                SendClient(ResponseType.GameStop.ToString());
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
        public void SystemMessage(string msg)
        {
            try
            {               
                SendClient(msg);
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
        public void SendMessage(string msg)
        {
            try
            {
                SendClient(msg);
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
