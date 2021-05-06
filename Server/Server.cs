using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinarySerializerNamespace;
using System.IO;

namespace Server
{
    class Server
    {
        private static readonly int port = 8005;
        private Indexer indexer;
        private int clientCounter;

        public Server()
        {
            this.indexer = new Indexer("indexer-assets/datasets/");
            this.clientCounter = 0;
        }


        public void StartWork()
        {
            this.indexer.CreateIndex();

            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            Socket socketListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socketListener.Bind(ipPoint);
            socketListener.Listen();

            Console.WriteLine("Server is waiting for new connections on port: " + port.ToString());

            while (true)
            {
                Socket socketHandler = socketListener.Accept();
                this.clientCounter++;
                Task clientTask = Task.Run(() => ServeClient(socketHandler, this.clientCounter));

            }
        }


        private void ServeClient(Socket socketHandler, int clientId)
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
                    Console.WriteLine("Client" + clientId.ToString() + " end work");
                    break;
                }

                Console.WriteLine(DateTime.Now.ToShortTimeString() + "\tClient " + clientId.ToString() + " : " + clientMessage);

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
