using System;
using System.Collections.Generic;

namespace Client
{
    class ClientProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter port: ");
            Client client = new(Convert.ToInt32(Console.ReadLine()));

            while(true)
            {
                Console.Write("Enter message: ");

                string message = Console.ReadLine();

                if(message == "")
                {
                    Console.WriteLine("Connection closed, end of work.");
                    break;
                }    

                SortedDictionary<string, List<string>> serverResponse = client.SendMessage(message);

                foreach (string lexem in serverResponse.Keys)
                {
                    Console.WriteLine(lexem + ": ");
                    foreach (string fileName in serverResponse[lexem])
                    {
                        Console.WriteLine("\t\t" + fileName);
                    }
                }
            }


            client.CloseSocket();
            Console.Read();
        }
    }
}
