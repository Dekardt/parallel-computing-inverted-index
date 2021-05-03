using System;

namespace Server
{
    class ServerProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter server port: ");
            Server server = new(Convert.ToInt32(Console.ReadLine()));

            server.StartWork();
        }
    }
}