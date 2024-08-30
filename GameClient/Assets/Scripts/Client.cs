using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;
    public TCP tcp;

    private void Awake()    // making instance of class
    {
        if (instance == null)
        {
            instance = this;

        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object");
            Destroy(this);
        }

    }

    private void Start()    // creates a new TCP client on start
    {
        tcp = new TCP();
    }

    public void ConnectToServer()   // used by start menu butto to connect.
    {
        tcp.Connect();
    }

    public class TCP    //TCP class, same as class on server using basic setup with stream and receiveBuffer
    {
        public TcpClient socket;

        private NetworkStream stream;
        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient  //sets new socket's rec and send buffer size to data buffer size : 4096
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result); //allows a connection to be had between target and client. thread is blocked (cant send data) until EndConnect is called.

            if (!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);  //begin stream read with current receiveBuffer, offset 0, size "bufferSize" : 4096, null object ref.
        }

        private void ReceiveCallback(IAsyncResult _result)  // receiving info from server
        {
            try
            {
                int _byteLength = stream.EndRead(_result);  // legacy way of handing async I/O operations, not needed to func now /\/\/\/\/\ CAN CHANGE /\/\/\/\/\
                if (_byteLength <= 0)
                {
                    return; // if byteLength is <= 0 then nothing to read, return.
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);  //copies receive buffer to new byte array for reading.

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception e) // catches any exceptions to not brick client, instead logs error.
            {
                Console.WriteLine($"Error receiving TCP data: {e}");
            }
        }
    }
}