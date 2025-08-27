using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datagrams
{
    public interface Datagram
    {
        byte[] ToBytes();
    }

    public struct Payload0x1111 : Datagram
    {
        public UInt32 payload_type = 0x1111;

        public Payload0x1111()
        {
            this.payload_type = 0x1111;
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[sizeof(UInt32)];
            Buffer.BlockCopy(BitConverter.GetBytes(payload_type), 0, bytes, 0, sizeof(UInt32));
            return bytes;
        }

        public static Payload0x1111 FromBytes(byte[] data)
        {
            Payload0x1111 datagram = new Payload0x1111();
            datagram.payload_type = BitConverter.ToUInt32(data, 0);
            return datagram;
        }

        public override string ToString()
        {
            return $"Datagram(Id={payload_type})";
        }

    } //New client announcement

    public struct Payload0x1222 : Datagram
    {
        public UInt32 payload_type = 0x1222;
        public UInt32 port_number;
        public UInt16 security_flag;

        public Payload0x1222(UInt32 portNumber, UInt16 security_flag)
        {
            this.payload_type = 0x1222;
            this.port_number = portNumber;
            this.security_flag = security_flag;
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[sizeof(UInt32) * 2 + sizeof(UInt16)]; // exactly 8 bytes
            Buffer.BlockCopy(BitConverter.GetBytes(payload_type), 0, bytes, 0, sizeof(UInt32));
            Buffer.BlockCopy(BitConverter.GetBytes(port_number), 0, bytes, sizeof(UInt32), sizeof(UInt32));
            Buffer.BlockCopy(BitConverter.GetBytes(security_flag), 0, bytes, sizeof(UInt32) * 2, sizeof(UInt32));
            return bytes;
        }

        public static Payload0x1222 FromBytes(byte[] data)
        {
            Payload0x1222 datagram = new Payload0x1222();
            datagram.payload_type = BitConverter.ToUInt32(data, 0);
            datagram.port_number = BitConverter.ToUInt32(data, sizeof(UInt32));
            datagram.security_flag = BitConverter.ToUInt16(data, sizeof(UInt16));
            return datagram;
        }

        public override string ToString()
        {
            return $"Datagram(Id={payload_type}, Port Number={port_number}, Security Flag={security_flag})";
        }
    } //New client announcement response

    public struct Payload0x1333 : Datagram
    {
        public UInt32 payload_type = 0x1333;
        public UInt32 clientID;

        public Payload0x1333(UInt32 clientID)
        {
            this.payload_type = 0x1333;
            this.clientID = clientID;
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[sizeof(UInt32) * 2];
            Buffer.BlockCopy(BitConverter.GetBytes(payload_type), 0, bytes, 0, sizeof(UInt32));
            Buffer.BlockCopy(BitConverter.GetBytes(clientID), 0, bytes, sizeof(UInt32), sizeof(UInt32));
            return bytes;
        }

        public static Payload0x1333 FromBytes(byte[] data)
        {
            Payload0x1333 datagram = new Payload0x1333();
            datagram.payload_type = BitConverter.ToUInt32(data, sizeof(UInt32));
            datagram.clientID = BitConverter.ToUInt32(data, sizeof(UInt32));
            return datagram;
        }

        public override string ToString()
        {
            return $"Datagram(Id={payload_type}, clientID={clientID})";
        }

    } //Client Identification

    public struct Payload0x1444 : Datagram
    {
        public UInt32 payload_type = 0x1444;
        public UInt16 clientIDresponse;

        public Payload0x1444(UInt16 clientIDresponse)
        {
            this.payload_type = 0x1333;
            this.clientIDresponse = clientIDresponse;
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[sizeof(UInt32) * 2];
            Buffer.BlockCopy(BitConverter.GetBytes(payload_type), 0, bytes, 0, sizeof(UInt32));
            Buffer.BlockCopy(BitConverter.GetBytes(clientIDresponse), 0, bytes, sizeof(UInt32), sizeof(UInt16));
            return bytes;
        }

        public static Payload0x1444 FromBytes(byte[] data)
        {
            Payload0x1444 datagram = new Payload0x1444();
            datagram.payload_type = BitConverter.ToUInt32(data, sizeof(UInt32));
            datagram.clientIDresponse = BitConverter.ToUInt16(data, sizeof(UInt16));
            return datagram;
        }

        public override string ToString()
        {
            return $"Datagram(Id={payload_type}, clientIDresponse={clientIDresponse})";
        }

    } //Client Identification response

    public struct Payload0x1555 : Datagram
    {
        public UInt32 payload_type = 0x1555;
        public UInt32 clientID;
        public UInt32 client_password;

        public Payload0x1555(UInt32 clientID, UInt32 client_password)
        {
            this.payload_type = 0x1555;
            this.clientID = clientID;
            this.client_password = client_password;
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[sizeof(UInt32) * 2];
            Buffer.BlockCopy(BitConverter.GetBytes(payload_type), 0, bytes, 0, sizeof(UInt32));
            Buffer.BlockCopy(BitConverter.GetBytes(clientID), 0, bytes, sizeof(UInt32), sizeof(UInt32));
            Buffer.BlockCopy(BitConverter.GetBytes(client_password), 0, bytes, sizeof(UInt32) * 2, sizeof(UInt32));
            return bytes;
        }

        public static Payload0x1555 FromBytes(byte[] data)
        {
            Payload0x1555 datagram = new Payload0x1555();
            datagram.payload_type = BitConverter.ToUInt32(data, sizeof(UInt32));
            datagram.clientID = BitConverter.ToUInt32(data, sizeof(UInt32));
            datagram.client_password = BitConverter.ToUInt32(data, sizeof(UInt32));
            return datagram;
        }

        public override string ToString()
        {
            return $"Datagram(Id={payload_type}, clientID={clientID}, client password={client_password})";
        }

    } //Secure Client Identification

    public struct Payload0x1666 : Datagram
    {
        public UInt32 payload_type = 0x1666;
        public UInt16 clientIDresponse;

        public Payload0x1666(UInt16 clientIDresponse)
        {
            this.payload_type = 0x1666;
            this.clientIDresponse = clientIDresponse;
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[sizeof(UInt32) * 2];
            Buffer.BlockCopy(BitConverter.GetBytes(payload_type), 0, bytes, 0, sizeof(UInt32));
            Buffer.BlockCopy(BitConverter.GetBytes(clientIDresponse), 0, bytes, sizeof(UInt32), sizeof(UInt16));
            return bytes;
        }

        public static Payload0x1666 FromBytes(byte[] data)
        {
            Payload0x1666 datagram = new Payload0x1666();
            datagram.payload_type = BitConverter.ToUInt32(data, sizeof(UInt32));
            datagram.clientIDresponse = BitConverter.ToUInt16(data, sizeof(UInt16));
            return datagram;
        }

        public override string ToString()
        {
            return $"Datagram(Id={payload_type}, clientIDresponse={clientIDresponse})";
        }

    } //Secure Client Identification response

    public struct Payload0x1777 : Datagram
    {
        public UInt32 payload_type = 0x1777;

        public Payload0x1777()
        {
            this.payload_type = 0x1777;
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[sizeof(UInt32)];
            Buffer.BlockCopy(BitConverter.GetBytes(payload_type), 0, bytes, 0, sizeof(UInt32));
            return bytes;
        }

        public static Payload0x1777 FromBytes(byte[] data)
        {
            Payload0x1777 datagram = new Payload0x1777();
            datagram.payload_type = BitConverter.ToUInt32(data, 0);
            return datagram;
        }

        public override string ToString()
        {
            return $"Datagram(Id={payload_type})";
        }

    } //Client disconnection
}