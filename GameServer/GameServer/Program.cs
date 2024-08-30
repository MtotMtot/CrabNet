using System;

namespace GameServer
{
    internal class Program
    {
        static void Main(string[] args) // main program exec, sets title, starts server with maxplayer 4, port 26950, waits for keypress to close.
        {
            Console.Title = "Game Server";

            Server.Start(4, 26950);

            Console.ReadKey();
        }
    }
}
