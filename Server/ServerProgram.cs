using System;

namespace Server
{
    class ServerProgram
    {
        static void Main(string[] args)
        {
            Server server = new();
            server.StartWork();
        }
    }
}