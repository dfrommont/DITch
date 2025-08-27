using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Datagrams;
using System.Runtime.CompilerServices;

namespace DITch
{
    public class UDP_Client
    {
        string serverAddress;
        UInt32 serverPort;
        Socket? server;
        UdpClient? udpClient;
        IPEndPoint? ipep;

        public UDP_Client(string serverAddress, UInt32 serverPort)
        {
            this.serverAddress = serverAddress;
            this.serverPort = serverPort;
            this.server = null;
            this.udpClient = null;
            this.ipep = null;
        }

        public void SetServerAddress(string address) => this.serverAddress = address;

        public string GetServerAddress() => this.serverAddress;

        public void SetServerPort(UInt32 port) => this.serverPort = port;

        public UInt32 GetServerPort() => this.serverPort;

        public bool ConnectToServer()
        {
            try
            {
                // Create local UDP client (auto-assign a port)
                this.udpClient = new UdpClient();

                // Save remote endpoint
                this.ipep = new IPEndPoint(IPAddress.Parse(serverAddress), (int)serverPort);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to UDP server {serverAddress}:{serverPort}, Error: {ex.Message}");
                return false;
            }
        }

        public bool DisconnectFromServer()
        {
            try
            {
                server.Close();
                this.server = null;
                this.ipep = null;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured disconnecting from the UDP server {serverAddress}:{serverPort}, Error: {ex.Message}");
                return false;
            }
        }

        public bool SendToServer<E>(E payload) where E : Datagram
        {
            try
            {
                byte[] data = payload.ToBytes();
                udpClient?.Send(data, data.Length, ipep); // send to server endpoint
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send payload, Error: {ex.Message}");
                return false;
            }
        }

        public byte[] ReceiveFromServer()
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            return udpClient.Receive(ref sender);
        }
    }
}
