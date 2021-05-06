using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using BinarySerializerNamespace;

namespace Client
{
    class Client
    {
        private static readonly string ServerAdress = "127.0.0.1";
        private int port;
        private Socket socket;


        public Client(int port)
        {
            this.port = port;
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ServerAdress), this.port);

            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipPoint);
            Console.WriteLine("Connected to sever");
        }


        public void StartCommunication()
        {
            while (true)
            {
                Console.Write("Enter message: ");

                string clientMessage = Console.ReadLine();

                if (clientMessage == "")
                {
                    continue;
                }

                byte[] data = BinarySerializer.Serialize<string>(clientMessage);
                this.socket.Send(BinarySerializer.Serialize<int>(data.Length));
                this.socket.Send(data);

                if (clientMessage == "0")
                {
                    Console.WriteLine("Connection closed, end of work.");
                    break;
                }

                data = new byte[512];
                socket.Receive(data);
                int bytes = BinarySerializer.Deserialize<int>(data);

                data = new byte[bytes];
                socket.Receive(data);
                Dictionary<string, List<string>> serverResponse = BinarySerializer.Deserialize<Dictionary<string, List<string>>>(data);

                foreach (string lexem in serverResponse.Keys)
                {
                    Console.WriteLine(lexem + ": ");
                    foreach (string fileName in serverResponse[lexem])
                    {
                        Console.WriteLine("\t\t" + fileName);
                    }
                }
            }

            this.socket.Shutdown(SocketShutdown.Both);
            this.socket.Close();
        }
    }
}
