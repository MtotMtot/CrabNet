using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }
    public static Dictionary<int, ServerClient> serverClients = new Dictionary<int, ServerClient>();
    public delegate void PacketHandler(int _fromServerClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    /// <summary>Starts the server.</summary>
    /// <param name="_maxPlayers">The maximum players that can be connected simultaneously.</param>
    /// <param name="_port">The port to start the server on.</param>
    public static void Start(int _maxPlayers, int _port)
    {
        MaxPlayers = _maxPlayers;
        Port = _port;

        Debug.Log("Starting server...");
        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallback, null);

        Debug.Log($"Server started on port {Port}.");
    }

    /// <summary>Handles new TCP connections.</summary>
    private static void TCPConnectCallback(IAsyncResult _result)
    {
        TcpClient _ServerClient = tcpListener.EndAcceptTcpClient(_result);
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
        Debug.Log($"Incoming connection from {_ServerClient.Client.RemoteEndPoint}...");

        for (int i = 1; i <= MaxPlayers; i++)
        {
            if (serverClients[i].tcp.socket == null)
            {
                serverClients[i].tcp.Connect(_ServerClient);
                return;
            }
        }

        Debug.Log($"{_ServerClient.Client.RemoteEndPoint} failed to connect: Server full!");
    }

    /// <summary>Receives incoming UDP data.</summary>
    private static void UDPReceiveCallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _ServerClientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = udpListener.EndReceive(_result, ref _ServerClientEndPoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (_data.Length < 4)
            {
                return;
            }

            using (Packet _packet = new Packet(_data))
            {
                int _ServerClientId = _packet.ReadInt();

                if (_ServerClientId == 0)
                {
                    return;
                }

                if (serverClients[_ServerClientId].udp.endPoint == null)
                {
                    // If this is a new connection
                    serverClients[_ServerClientId].udp.Connect(_ServerClientEndPoint);
                    return;
                }

                if (serverClients[_ServerClientId].udp.endPoint.ToString() == _ServerClientEndPoint.ToString())
                {
                    // Ensures that the ServerClient is not being impersonated by another by sending a false ServerClientID
                    serverClients[_ServerClientId].udp.HandleData(_packet);
                }
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error receiving UDP data: {_ex}");
        }
    }

    /// <summary>Sends a packet to the specified endpoint via UDP.</summary>
    /// <param name="_ServerClientEndPoint">The endpoint to send the packet to.</param>
    /// <param name="_packet">The packet to send.</param>
    public static void SendUDPData(IPEndPoint _ServerClientEndPoint, Packet _packet)
    {
        try
        {
            if (_ServerClientEndPoint != null)
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _ServerClientEndPoint, null, null);
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error sending data to {_ServerClientEndPoint} via UDP: {_ex}");
        }
    }

    /// <summary>Initializes necessary server data.</summary>
    private static void InitializeServerData()
    {
        for (int i = 1; i <= MaxPlayers; i++)
        {
            serverClients.Add(i, new ServerClient(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
            };
        Debug.Log("Initialized packets.");
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
}
