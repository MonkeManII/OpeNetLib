namespace OpeNetLib.Packet
{
    /// <summary>
    /// A class used to construct <see cref="byte[]"/> packets to send over network.
    /// </summary>
    public class PacketConstructor
    {
        /// <summary>
        /// The space remaining in this <see cref="byte[]"/> packet.
        /// </summary>
        public int FreeBytes => packet.Length - position;

        /// <summary>
        /// The pointer into the packet array to read/write from.
        /// </summary>
        int position;

        /// <summary>
        /// The byte data of this packet.
        /// </summary>
        readonly byte[] packet;

        public PacketConstructor(byte type, int maxSize)
        {
            packet = new byte[maxSize];
            packet[0] = type;
            position = 1;
        }

        /// <summary>
        /// Writes a <see cref="IByteConvertable{TThis}"/> object to this packet.
        /// </summary>
        /// <typeparam name="T">The type of the object to write.</typeparam>
        /// <param name="obj">The </param>
        /// <exception cref="PacketOverflowException"></exception>
        public void Write<T>(IByteConvertable<T> obj)
            where T : IByteConvertable<T>
        {
            Write(obj.ToBytes());
        }

        /// <summary>
        /// Writes a set of bytes to this packet, and advances <see cref="position"/> by bytes.Length + 2.
        /// </summary>
        /// <param name="bytes">The bytes to write to this packet.</param>
        public void Write(byte[] bytes)
        {
            if (bytes.Length > FreeBytes)
            {
                throw new PacketOverflowException(bytes.Length - FreeBytes);
            }

            byte[] size = BitConverter.GetBytes((ushort)bytes.Length);
            size.CopyTo(packet, position);
            position += size.Length;

            bytes.CopyTo(packet, position);
            position += bytes.Length;
        }

        /// <summary>
        /// Writes a set of bytes to this packet, and advances <see cref="position"/> by bytes.Length + 2.
        /// </summary>
        /// <param name="bytes">The bytes to write to this packet.</param>
        public void Write(Span<byte> bytes)
        {
            if (bytes.Length > FreeBytes)
            {
                throw new PacketOverflowException(bytes.Length - FreeBytes);
            }

            byte[] size = BitConverter.GetBytes((ushort)bytes.Length);
            size.CopyTo(packet, position);
            position += size.Length;

            bytes.CopyTo(packet.AsSpan()[position..]);
            position += bytes.Length;
        }

        /// <summary>
        /// Creates a <see cref="Span{T}"/> containing the bytes of this packet.
        /// </summary>
        /// <returns>A <see cref="Span{T}"/> containing only the used bytes in this packet.</returns>
        public Span<byte> ResultSpan()
        {
            return packet.AsSpan()[..position];
        }

        /// <summary>
        /// Creates a <see cref="byte[]"/> containing the bytes of this packet.
        /// </summary>
        /// <returns>A <see cref="byte[]"/> containing only the used bytes in this packet.</returns>
        public byte[] ResultBytes()
        {
            return packet[..position];
        }

        /// <summary>
        /// Gets all of the bytes in this packet, including unused ones.
        /// </summary>
        /// <returns>A <see cref="byte[]"/> containing all packet bytes.</returns>
        public byte[] ResultBytesFull()
        {
            return packet;
        }

        /// <summary>
        /// Gets all of the bytes in this packet, including unused ones.
        /// </summary>
        /// <returns>An <see cref="Span{T}"> of all packet bytes.</returns>
        public Span<byte> ResultSpanFull()
        {
            return packet.AsSpan();
        }
    }
}
