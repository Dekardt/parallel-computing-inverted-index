using System;

namespace Client {
    class ClientProgram {
        static void Main(string[] args) {
            Console.WriteLine("Enter port: ");
            Client client = new(Convert.ToInt32(Console.ReadLine()));
            client.StartCommunication();
        }
    }
}
