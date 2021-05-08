using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using BinarySerializerNamespace;

namespace Client {

    class Client {
        private const string _serverAdress = "127.0.0.1";
        private readonly int _port;
        private readonly Socket _socket;


        public Client(int port) {

            _port = port;
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(_serverAdress), _port);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(ipPoint);
            Console.WriteLine("Connected to sever");
        }


        public void StartCommunication() {
            while (true) {

                Console.Write("Enter message to send or enter '0' to end work: ");
                string clientMessage = Console.ReadLine();

                if (clientMessage == "") {
                    continue;
                }

                byte[] sendingBuffer = BinarySerializer.Serialize<string>(clientMessage);

                // send message size
                _socket.Send(BinarySerializer.Serialize<int>(sendingBuffer.Length));

                _socket.Send(sendingBuffer);

                if (clientMessage == "0") {
                    Console.WriteLine("Connection closed, end of work.");
                    break;
                }

                sendingBuffer = new byte[512];

                // receive answer size
                _socket.Receive(sendingBuffer);
                int bytes = BinarySerializer.Deserialize<int>(sendingBuffer);

                sendingBuffer = new byte[bytes];
                _socket.Receive(sendingBuffer);
                var serverDictResponse = BinarySerializer.Deserialize<Dictionary<string, List<string>>>(sendingBuffer);

                if (serverDictResponse.Count == 0) {
                    Console.WriteLine("None of the words were found in the files");
                } else {
                    foreach (string lexem in serverDictResponse.Keys)
                    {
                        Console.WriteLine(lexem + ": ");
                        foreach (string fileName in serverDictResponse[lexem])
                        {
                            Console.WriteLine("\t\t" + fileName);
                        }
                    }
                }
            }

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
    }
}