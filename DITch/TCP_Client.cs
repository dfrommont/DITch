using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Datagrams;

namespace DITch
{
    public class TCP_Client
    {
        string serverAddress = "127.0.0.1";
        UInt32 serverPort = 8000;
        NetworkStream? stream;

        public TCP_Client(string serverAddress, UInt32 serverPort)
        {
            this.serverAddress = serverAddress;
            this.serverPort = serverPort;
        }

        public void SetServerAddress(string address) => this.serverAddress = address;

        public string GetServerAddress() => this.serverAddress;

        public void SetServerPort(UInt32 port) => this.serverPort = port;

        public UInt32 GetServerPort() => this.serverPort;

        public bool ConnectToServer()
        {
            using (TcpClient client = new TcpClient())
            {
                try
                {
                    client.Connect(serverAddress, (Int32)serverPort);
                    Console.WriteLine($"Connected to server at {serverAddress}:{serverPort}");
                    stream = client.GetStream();

                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to connect to server: {serverAddress}:{serverPort}, Error: {e.Message}");
                    return false;
                }
            }
        }

        public bool ConnectToServer(string address, UInt32 port)
        {
            this.serverAddress = address;
            this.serverPort = port;
            using (TcpClient client = new TcpClient())
            {
                try
                {
                    client.Connect(serverAddress, (Int32)serverPort);
                    Console.WriteLine($"Connected to server at {serverAddress}:{serverPort}");
                    stream = client.GetStream();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to connect to server: {serverAddress}:{serverPort}, Error: {e.Message}");
                    return false;
                }
            }
        }

        public bool DisconnectFromServer()
        {
            try
            {
                stream.Flush();
                stream.Close();
                return true;
            }
            catch (Exception e) {
                Console.WriteLine($"Failed to close connection to server correctly, aborting connection");
                return false;
            }
        }

        public bool SendToServer<E>(E payload) where E : Datagram
        {
            try
            {
                byte[] data = payload.ToBytes();
                byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

                stream.Write(lengthPrefix, 0, 4);
                stream.Write(data, 0, data.Length);
                Console.WriteLine($"Sent: {payload.ToString()}");
                return true;
            }
            catch (Exception e) {
                Console.WriteLine($"TCP client failed to send payload {payload}");
                return false;
            }

        }

        public byte[] ReceiveFromServer()
        {
            byte[] responseLengthBytes = new byte[4];
            int bytesRead = stream.Read(responseLengthBytes, 0, 4);
            if (bytesRead == 0)
            {
                Console.WriteLine("Server closed connection.");
                return [];
            }

            int responseLength = BitConverter.ToInt32(responseLengthBytes, 0);
            byte[] responseData = new byte[responseLength];
            int totalRead = 0;

            while (totalRead < responseLength)
            {
                int read = stream.Read(responseData, totalRead, responseLength - totalRead);
                if (read == 0)
                {
                    Console.WriteLine("Server disconnected unexpectedly.");
                    return [];
                }
                totalRead += read;
            }

            return responseData;
        }
    }
}