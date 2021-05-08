using BinarySerializerNamespace;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


namespace Server {
    class Server {
        private const int _port = 8005;
        private readonly Indexer _indexer;
        private int _clientCounter;

        public Server() {
            _indexer = new Indexer("indexer-assets/datasets/");
            _clientCounter = 0;
        }


        public void StartWork() {
            _indexer.CreateIndex();

            var ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);
            Socket socketListener = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketListener.Bind(ipPoint);
            socketListener.Listen();

            Console.WriteLine("Server is waiting for new connections on port: " + _port.ToString());

            while (true)
            {
                Socket socketHandler = socketListener.Accept();
                _clientCounter++;
                Task clientTask = Task.Run(() => ServeClient(socketHandler, _clientCounter));

            }
        }


        private void ServeClient(Socket socketHandler, int clientId) {
            while(true) {
                byte[] sendingBuffer = new byte[512];
                socketHandler.Receive(sendingBuffer);
                int messageSize = BinarySerializer.Deserialize<int>(sendingBuffer);

                sendingBuffer = new byte[messageSize];
                socketHandler.Receive(sendingBuffer);
                string clientMessage = BinarySerializer.Deserialize<string>(sendingBuffer);

                if(clientMessage == "0") {
                    Console.WriteLine("Client" + clientId.ToString() + " end work");
                    break;
                }

                Console.WriteLine(DateTime.Now.ToShortTimeString() + "\tClient " + clientId.ToString() + " : " + clientMessage);

                Dictionary<string, List<string>> resultDict = _indexer.AnalyzeInput(clientMessage);

                sendingBuffer = BinarySerializer.Serialize<Dictionary<string, List<string>>>(resultDict);
                socketHandler.Send(BinarySerializer.Serialize<int>(sendingBuffer.Length));
                socketHandler.Send(sendingBuffer);
            }

            socketHandler.Shutdown(SocketShutdown.Both);
            socketHandler.Close();
        }
    }
}
