using System.Text;

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
        /// Writes a set of bytes to this packet, and advances <see cref="position"/> by bytes.Length.
        /// <para>
        /// It is worth noting that this saves two bytes, but only works if the reciever knows the exact length of this <see cref="Span{T}"/>.
        /// </para>
        /// </summary>
        /// <param name="bytes">The bytes to write to this packet.</param>
        public void WriteExactly(Span<byte> bytes)
        {
            if (bytes.Length > FreeBytes)
            {
                throw new PacketOverflowException(bytes.Length - FreeBytes);
            }
            bytes.CopyTo(packet.AsSpan()[position..]);
            position += bytes.Length;
        }

        /// <summary>
        /// Writes a set of bytes to this packet, and advances <see cref="position"/> by bytes.Length.
        /// <para>
        /// It is worth noting that this saves two bytes, but only works if the reciever knows the exact length of this <see cref="byte[]"/>.
        /// </para>
        /// </summary>
        /// <param name="bytes">The bytes to write to this packet.</param>
        public void WriteExactly(byte[] bytes)
        {
            if (bytes.Length > FreeBytes)
            {
                throw new PacketOverflowException(bytes.Length - FreeBytes);
            }
            bytes.CopyTo(packet, position);
            position += bytes.Length;
        }

        /// <summary>
        /// Writes a set of bytes to this packet, and advances <see cref="position"/> by bytes.Length + 2.
        /// </summary>
        /// <param name="bytes">The bytes to write to this packet.</param>
        public void Write(Span<byte> bytes)
        {
            if (bytes.Length + 2 > FreeBytes)
            {
                throw new PacketOverflowException((bytes.Length + 2) - FreeBytes);
            }

            byte[] size = BitConverter.GetBytes((ushort)bytes.Length);
            WriteExactly(size);
            WriteExactly(bytes);
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
            WriteExactly(size);
            WriteExactly(bytes);
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

        #region Specialized Write Functions

        /// <summary>
        /// Writes a <see cref="string"/> to the packet in <see cref="Encoding.UTF8"/> format.
        /// </summary>
        public void WriteUTF8(string dat) => Write(Encoding.UTF8.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="string"/> to the packet in <see cref="Encoding.UTF32"/> format.
        /// </summary>
        public void WriteUTF32(string dat) => Write(Encoding.UTF32.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="string"/> to the packet in <see cref="Encoding.ASCII"/> format.
        /// </summary>
        public void WriteASCII(string dat) => Write(Encoding.ASCII.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="ulong"/> to the packet.
        /// </summary>
        public void WriteULong(ulong dat) => WriteExactly(BitConverter.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="uint"/> to the packet.
        /// </summary>
        public void WriteUInt(uint dat) => WriteExactly(BitConverter.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="ushort"/> to the packet.
        /// </summary>
        public void WriteUShort(ushort dat) => WriteExactly(BitConverter.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="sbyte"/> to the packet.
        /// </summary>
        public void WriteSByte(sbyte dat) => WriteExactly( [ (byte)dat ] );

        /// <summary>
        /// Writes a <see cref="long"/> to the packet.
        /// </summary>
        public void WriteLong(long dat) => WriteExactly(BitConverter.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="int"/> to the packet.
        /// </summary>
        public void WriteInt(int dat) => WriteExactly(BitConverter.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="short"/> to the packet.
        /// </summary>
        public void WriteShort(short dat) => WriteExactly(BitConverter.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="byte"/> to the packet.
        /// </summary>
        public void WriteByte(byte dat) => WriteExactly( [ dat ] );

        /// <summary>
        /// Writes a <see cref="float"/> to the packet.
        /// </summary>
        public void WriteFloat(float dat) => WriteExactly(BitConverter.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="double"/> to the packet.
        /// </summary>
        public void WriteDouble(double dat) => WriteExactly(BitConverter.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="Half"/> to the packet.
        /// </summary>
        public void WriteHalf(Half dat) => WriteExactly(BitConverter.GetBytes(dat));

        #endregion
    }
}
