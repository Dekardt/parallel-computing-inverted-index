using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinarySerializerNamespace;

namespace Server
{
    class Server
    {
        private int port;
        private Indexer indexer;

        public Server(int port)
        {
            this.port = port;

            this.indexer = new Indexer("datasets/");
        }


        public void StartWork()
        {
            this.indexer.CreateIndex();

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
            while(true)
            {
                byte[] data = new byte[512];
                socketHandler.Receive(data);
                int bytes = BinarySerializer.Deserialize<int>(data);

                data = new byte[bytes];
                socketHandler.Receive(data);
                string clientMessage = BinarySerializer.Deserialize<string>(data);

                if(clientMessage == "0")
                {
                    Console.WriteLine("Client end work");
                    break;
                }

                Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + clientMessage);

                Dictionary<string, List<string>> resultDict = this.indexer.AnalyzeInput(clientMessage);

                data = BinarySerializer.Serialize<Dictionary<string, List<string>>>(resultDict);
                socketHandler.Send(BinarySerializer.Serialize<int>(data.Length));
                socketHandler.Send(data);
            }


            socketHandler.Shutdown(SocketShutdown.Both);
            socketHandler.Close();
        }
    }
}
