using System;

namespace GameServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Game Server";

            Server.Start(4, 26950);

            Console.ReadKey();
        }
    }
}
