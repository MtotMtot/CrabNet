using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    internal class Client
    {
        public static int dataBufferSize = 4096;

        public int id;
        public TCP tcp;

        public Client(int _clientId)
        {
            id = _clientId;
            tcp = new TCP(id);

        }

        public class TCP    //the TCP class, core of sending/receiving data between server + client(s).
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private byte[] receiveBuffer;
            public TCP(int _id)
            {
                id = _id;
            }

            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }


            private void ReceiveCallback(IAsyncResult _result)  // receiving info 
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
                catch (Exception e) //prevents errors for stopping server, logs exception to console.
                {
                    Console.WriteLine($"Error receiving TCP data: {e}");
                }
            }
        }
    }
}
