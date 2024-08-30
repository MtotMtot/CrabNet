using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Configuration;

namespace GameServer
{
    internal class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client> ();

        private static TcpListener tcpListener;

        public static void Start(int _maxPlayers, int _port)    //sets up server to receive client connections.
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            Console.WriteLine("Starting Server...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Server started on {Port}.");
        }

        private static void TCPConnectCallback(IAsyncResult _result)    //called when client connects
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient( _result );  //EndAccept to catch this client only, allow transfer of data.
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);  // restart listen for new clients. its async :)
            Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint}...");  //writes to console current incoming client connection

            for (int i = 1; i <= MaxPlayers; i++)   //adds client to list if entry is null vvvvvvv
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }

            Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect: Server Full");   //if list is iterated through and no null slots, server is full. disconnect

        }

        private static void InitializeServerData()
        {
            for (int i = 1; i < MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }
        }
    }
}
