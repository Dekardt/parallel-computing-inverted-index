namespace Server
{
    class ServerProgram
    {
        static void Main(string[] args)
        {
            Server server = new(8005);
            server.StartWork();
        }
    }
}