using OpeNetLib.Compat;
using OpeNetLib.Serializer;
using System.Net;

namespace OpeNetLib
{
    /// <summary>
    /// Represents a unique identifier per <see cref="IPEndPoint"/> that can be written directly to and from packets.
    /// </summary>
    public readonly struct EndpointIdentifier :
        IEquatable<EndpointIdentifier>
    {
        public static int ByteWidth => 16;
        public static readonly ObjectSerializer<EndpointIdentifier> Serializer = new(
            (obj) => obj.ToBytes(),
            FromBytes,
            ByteWidth
        );
        
        /// <summary>
        /// The address of this endpoint, represented as an <see cref="Int128"/>.
        /// </summary>
        public readonly Int128 Address;

        /// <summary>
        /// The port of this endpoint.
        /// </summary>
        public readonly ushort Port;

        private EndpointIdentifier(Int128 address, ushort port)
        {
            Port = port;
            Address = address;
        }

        private EndpointIdentifier(Span<byte> bytes)
        {
            if (bytes.Length < ByteWidth)
            {
                throw new ArgumentException($"Argument {nameof(bytes)} must be at least {ByteWidth} long.", nameof(bytes));
            }
            Port = BitConverter.ToUInt16(bytes[0..2]);
            Address = ConversionExtension.ToInt128(bytes[2..]);
        }

        public IPEndPoint ToEndpoint()
        {
            IPAddress addr = new(ConversionExtension.GetBytes(Address));
            return new IPEndPoint(addr, Port);
        }

        public static EndpointIdentifier FromEndpoint(IPEndPoint endpoint)
        {
            byte[] address = new byte[ByteWidth];
            endpoint.Address.GetAddressBytes().CopyTo((Span<byte>)address);

            Int128 addr = ConversionExtension.ToInt128(address);
            ushort port = (ushort)endpoint.Port;
            return new EndpointIdentifier(addr, port);
        }

        public readonly bool Equals(EndpointIdentifier other)
        {
            return
                other.Address == Address &&
                other.Port == Port;
        }

        public readonly byte[] ToBytes()
        {
            byte[] ret = new byte[ByteWidth];
            BitConverter.GetBytes(Port).CopyTo(ret, 0);
            ConversionExtension.GetBytes(Address).CopyTo(ret, 2);
            return ret;
        }

        public static EndpointIdentifier FromBytes(Span<byte> bytes)
        {
            return new(bytes);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Address, Port);
        }

        public override readonly bool Equals(object? obj)
        {
            if (obj is not EndpointIdentifier id) return false;
            return Equals(id);
        }

        public static bool operator ==(EndpointIdentifier v1, EndpointIdentifier v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(EndpointIdentifier v1, EndpointIdentifier v2)
        {
            return !v1.Equals(v2);
        }
    }
}
