using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Threading.Tasks;
using BinarySerializerNamespace;

namespace Server
{
    class Server
    {
        private int port;


        public Server(int port)
        {
            this.port = port;
        }


        public void StartWork()
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), this.port);
            Socket socketListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socketListener.Bind(ipPoint);
            socketListener.Listen();

            Console.WriteLine("Server is waiting for new connections...");

            while (true)
            {
                Socket socketHandler = socketListener.Accept();
                Task clientTask = Task.Run(() => ServeClient(socketHandler));
            }
        }


        private void ServeClient(Socket socketHandler)
        {
            byte[] data = new byte[512];
            socketHandler.Receive(data);
            int bytes = BinarySerializer.Deserialize<int>(data);

            data = new byte[bytes];
            socketHandler.Receive(data);
            string message = BinarySerializer.Deserialize<string>(data);

            Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + message);

            string answer = "Server got message";

            data = new byte[answer.Length];
            data = BinarySerializer.Serialize<string>(answer);
            socketHandler.Send(BinarySerializer.Serialize<int>(data.Length));
            socketHandler.Send(data);

            socketHandler.Shutdown(SocketShutdown.Both);
            socketHandler.Close();
        }
    }
}
