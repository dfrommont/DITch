using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Datagrams;

namespace DITch
{
    internal class NetworkManager
    {
        private UDP_Client udp_client;
        private TCP_Client tcp_client;
        private int protocol_mode; //0 - UDP, 1 - TCP
        private string serverAddress;
        private UInt32 serverPort;
        private bool isServerSecure = false;

        public NetworkManager(string UDPserverAddress, UInt32 UDPserverPort, string TCPserverAddress, UInt32 TCPserverPort, int mode)
        {
            this.protocol_mode = mode;
            this.serverAddress = UDPserverAddress;
            this.serverPort = UDPserverPort;
        }

        public NetworkManager(string UDPserverAddress, UInt32 UDPserverPort, string TCPserverAddress, UInt32 TCPserverPort, string mode)
        {
            if (mode == "UDP" || mode == "udp")
            {
                this.protocol_mode = 0;
            } else if (mode == "TCP" || mode == "tcp")
            {
                this.protocol_mode = 1;
            } else
            {
                this.protocol_mode = 0;
                Console.WriteLine("User supplied unknown protocol by name, defaulting to UDP");
            }
            this.serverAddress = UDPserverAddress;
            this.serverPort = UDPserverPort;
        }

        public int GetProtocolMode() => this.protocol_mode;

        public void SetProtocolMode(int mode) => this.protocol_mode = mode;

        public void SetProtocolMode(string mode)
        {
            if (mode == "UDP" || mode == "udp")
            {
                this.protocol_mode = 0;
            }
            else if (mode == "TCP" || mode == "tcp")
            {
                this.protocol_mode = 1;
            }
            else
            {
                this.protocol_mode = 0;
                Console.WriteLine("User supplied unknown protocol by name, defaulting to UDP");
            }
        }

        public UInt32 GetPort() => this.serverPort;

        public void SetPort(UInt32 port) => this.serverPort = port;

        private bool ConnectToUDPServer()
        {
            if (this.udp_client != null)
            {
                return false;
            } else
            {
                this.udp_client = new UDP_Client(this.serverAddress, this.serverPort);
                return this.udp_client.ConnectToServer();
            }
        }

        private bool ConnectToTCPServer()
        {
            if (this.tcp_client != null)
            {
                return false;
            }
            else
            {
                this.tcp_client = new TCP_Client(this.serverAddress, this.serverPort);
                return this.tcp_client.ConnectToServer();
            }
        }

        public bool ConnectToServer()
        {
            bool success = (this.protocol_mode == 0) ? ConnectToUDPServer() : ConnectToTCPServer();
            success &= SendData(0x1111, null);

            Payload0x1222 datagram = (Payload0x1222)ReceiveData();
            if (datagram is Payload0x1222 response)
            {
                Console.WriteLine($"Server responded with new port: {response.port_number}");
                SetPort(response.port_number);
                isServerSecure = (response.security_flag == 1);
            }
            else
            {
                Console.WriteLine("No response or unexpected payload type.");
            }

            return success;
        }

        public bool DisconnectFromServer()
        {
            SendData(0x1777, null);
            this.tcp_client?.DisconnectFromServer();

            this.udp_client?.DisconnectFromServer();

            return true; ;
        }

        private bool SendCall<E>(E payload) where E : Datagrams.Datagram
        {
            if (this.protocol_mode == 0) // UDP mode
            {
                return udp_client?.SendToServer(payload) ?? false;
            }
            else // TCP mode
            {
                return tcp_client?.SendToServer(payload) ?? false;
            }
        }

        public bool SendData(UInt32 payloadType, byte[]? data)
        {
            switch (payloadType) {
                case 0x1111:
                    Payload0x1111 payload0x1111 = new Payload0x1111();
                    return SendCall(payload0x1111);
                case 0x1222: //client doesn't send 0x1222
                    /*if (data?.Length != 4)
                    {
                        Console.WriteLine($"User attempted to populate a 0x1222 payload with poor data. Expected 4 bytes, got {data?.Length ?? 0}");
                        return false;
                    }
                    else
                    {
                        Payload0x1222 payload0x1222 = new Payload0x1222(BitConverter.ToUInt32(data), 0);
                        return SendCall(payload0x1222);
                    }*/
                default:
                    return false;
            }
        }

        public Datagram? ReceiveData()
        {
            byte[] data;
            if (this.protocol_mode == 0)
            {
                data = udp_client.ReceiveFromServer();
            }
            else
            {
                data = tcp_client.ReceiveFromServer();
            }

            UInt32 payloadType = BitConverter.ToUInt32(data);
            switch (payloadType)
            {
                case 0x1111:
                    return Payload0x1111.FromBytes(data);
                case 0x1222:
                    return Payload0x1222.FromBytes(data);
                default:
                    return null;
            }
        }
    }
}
