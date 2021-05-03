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

            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ServerAdress), port);

            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipPoint);
        }

        public SortedDictionary<string, List<string>> SendMessage(string clientMessage)
        {
            //Console.Write("Enter message: ");

            //string message = Console.ReadLine();
            byte[] data = BinarySerializer.Serialize<string>(clientMessage);
            this.socket.Send(BinarySerializer.Serialize<int>(data.Length));
            this.socket.Send(data);

            data = new byte[512];
            socket.Receive(data);
            int bytes = BinarySerializer.Deserialize<int>(data);

            data = new byte[bytes];
            socket.Receive(data);
            SortedDictionary<string, List<string>> answer = BinarySerializer.Deserialize<SortedDictionary<string, List<string>>>(data);

            return answer;
        }

        public void CloseSocket()
        {
            this.socket.Shutdown(SocketShutdown.Both);
            this.socket.Close();
        }
    }
}
