using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new(8005);
            string serverResponse = client.SendMessage();
            Console.WriteLine(serverResponse);
            client.CloseSocket();
            Console.Read();
        }
    }
}
