using OpeNetLib.Serializer;
using System.Text;

namespace OpeNetLib.Packet
{
    /// <summary>
    /// A class used to construct <see cref="byte"/>[] packets to send over network.
    /// </summary>
    public class PacketConstructor
    {
        /// <summary>
        /// The <see cref="byte"/> size of each "length" (before a <seealso cref="Span{byte}"/>) in the packet.
        /// <para>
        /// Each "length" represents how large the <see cref="Span{byte}"/> is as a <see cref="ushort"/>.
        /// </para>
        /// </summary>
        internal const int SPAN_LENGTH_SIZE = sizeof(ushort);

        /// <summary>
        /// The space remaining in this <see cref="byte"/>[] packet.
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

        public PacketConstructor(int maxSize)
        {
            packet = new byte[maxSize];
            position = 0;
        }

        [Obsolete($"{nameof(PacketConstructor)}(byte, int) uses the old one-byte header system. Please define your own headers.")]
        public PacketConstructor(byte type, int maxSize)
        {
            packet = new byte[maxSize];
            position = 1;
            packet[0] = type;
        }

        [Obsolete($"{nameof(PacketType)} uses the old one-byte header system. Please define your own headers.")]
        public byte PacketType() => packet[0];

        #region Packet Result Functions

        /// <summary>
        /// Creates a <see cref="Span{T}"/> containing the bytes of this packet.
        /// </summary>
        /// <returns>A <see cref="Span{T}"/> containing only the used bytes in this packet.</returns>
        public Span<byte> ResultSpan()
        {
            return packet.AsSpan()[..position];
        }

        /// <summary>
        /// Creates a <see cref="byte"/>[] containing the bytes of this packet.
        /// </summary>
        /// <returns>A <see cref="byte"/>[] containing only the used bytes in this packet.</returns>
        public byte[] ResultBytes()
        {
            return packet[..position];
        }

        /// <summary>
        /// Gets all of the bytes in this packet, including unused ones.
        /// </summary>
        /// <returns>A <see cref="byte"/>[] containing all packet bytes.</returns>
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

        #endregion

        #region Basic Write Functions

        /// <summary>
        /// Writes a set of bytes to this packet, and advances <see cref="position"/> by bytes.Length.
        /// <para>
        /// It is worth noting that this saves two bytes compared to <see cref="Write(Span{byte})"/>, but only works if the reciever knows the exact length of this <see cref="Span{T}"/>.
        /// </para>
        /// <para>
        /// Size (<see cref="byte"/>): <paramref name="bytes"/> length.
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
        /// It is worth noting that this saves two bytes compared to <see cref="Write(byte[])"/>, but only works if the reciever knows the exact length of this <see cref="byte"/>[].
        /// </para>
        /// <para>
        /// Size (<see cref="byte"/>): <paramref name="bytes"/> length.
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
        /// <para>
        /// Size (<see cref="byte"/>): 2 + <paramref name="bytes"/> length.
        /// </para>
        /// </summary>
        /// <param name="bytes">The bytes to write to this packet.</param>
        public void Write(Span<byte> bytes)
        {
            if (bytes.Length + SPAN_LENGTH_SIZE > FreeBytes)
            {
                throw new PacketOverflowException((bytes.Length + SPAN_LENGTH_SIZE) - FreeBytes);
            }

            WriteUShort((ushort)bytes.Length);
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

            WriteUShort((ushort)bytes.Length);
            WriteExactly(bytes);
        }

        #endregion

        #region Specialized Write Functions

        /// <summary>
        /// Writes a <see cref="string"/> to the packet in the specified format.
        /// <para>
        /// Size (<see cref="byte"/>): varies.
        /// </para>
        /// </summary>
        public void WriteString(string dat, Encoding encoding) => Write(encoding.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="string"/> to the packet in <see cref="Encoding.UTF8"/> format.
        /// <para>
        /// Size (<see cref="byte"/>): varies based on characters.
        /// </para>
        /// </summary>
        public void WriteUTF8(string dat) => Write(Encoding.UTF8.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="string"/> to the packet in <see cref="Encoding.UTF32"/> format.
        /// <para>
        /// Size (<see cref="byte"/>): 2 + 4 * <paramref name="dat"/> length.
        /// </para>
        /// </summary>
        public void WriteUTF32(string dat) => Write(Encoding.UTF32.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="string"/> to the packet in <see cref="Encoding.ASCII"/> format.
        /// <para>
        /// Size (<see cref="byte"/>): 2 + <paramref name="dat"/> length
        /// </para>
        /// </summary>
        public void WriteASCII(string dat) => Write(Encoding.ASCII.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="ulong"/> to the packet.
        /// <para>
        /// Size (<see cref="byte"/>): 8
        /// </para>
        /// </summary>
        public void WriteULong(ulong dat) => WriteExactly(BitConverter.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="uint"/> to the packet.
        /// <para>
        /// Size (<see cref="byte"/>): 4
        /// </para>
        /// </summary>
        public void WriteUInt(uint dat) => WriteExactly(BitConverter.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="ushort"/> to the packet.
        /// <para>
        /// Size (<see cref="byte"/>): 2
        /// </para>
        /// </summary>
        public void WriteUShort(ushort dat) => WriteExactly(BitConverter.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="sbyte"/> to the packet.
        /// <para>
        /// Size (<see cref="byte"/>): 1
        /// </para>
        /// </summary>
        public void WriteSByte(sbyte dat) => WriteExactly( [ (byte)dat ] );

        /// <summary>
        /// Writes a <see cref="long"/> to the packet.
        /// <para>
        /// Size (<see cref="byte"/>): 8
        /// </para>
        /// </summary>
        public void WriteLong(long dat) => WriteExactly(BitConverter.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="int"/> to the packet.
        /// <para>
        /// Size (<see cref="byte"/>): 4
        /// </para>
        /// </summary>
        public void WriteInt(int dat) => WriteExactly(BitConverter.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="short"/> to the packet.
        /// <para>
        /// Size (<see cref="byte"/>): 2
        /// </para>
        /// </summary>
        public void WriteShort(short dat) => WriteExactly(BitConverter.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="byte"/> to the packet.
        /// <para>
        /// Size (<see cref="byte"/>): 1
        /// </para>
        /// </summary>
        public void WriteByte(byte dat) => WriteExactly( [ dat ] );

        /// <summary>
        /// Writes a <see cref="float"/> to the packet.
        /// <para>
        /// Size (<see cref="byte"/>): 4
        /// </para>
        /// </summary>
        public void WriteFloat(float dat) => WriteExactly(BitConverter.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="double"/> to the packet.
        /// <para>
        /// Size (<see cref="byte"/>): 8
        /// </para>
        /// </summary>
        public void WriteDouble(double dat) => WriteExactly(BitConverter.GetBytes(dat));

        /// <summary>
        /// Writes a <see cref="Half"/> to the packet.
        /// <para>
        /// Size (<see cref="byte"/>): 2
        /// </para>
        /// </summary>
        public void WriteHalf(Half dat) => WriteExactly(BitConverter.GetBytes(dat));

        #endregion

        #region Generic Write Functions

        /// <summary>
        /// Writes an object to this packet using the specified <see cref="ObjectSerializer{T}"/>.
        /// <para>
        /// Size (<see cref="byte"/>): 2 + size of <paramref name="obj"/>.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of the object to write.</typeparam>
        /// <param name="obj">The object to write.</param>
        /// <exception cref="PacketOverflowException"></exception>
        public void Write<T>(T obj, ObjectSerializer<T> serializer)
        {
            Write(serializer.Serialize(obj));
        }

        /// <summary>
        /// Writes an object to this packet using the specified <see cref="ObjectSerializer{T}"/>.
        /// <para>
        /// <typeparamref name="T"/> must have a constant size.
        /// </para>
        /// <para>
        /// Size (<see cref="byte"/>): size of <paramref name="obj"/>.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of the object to write.</typeparam>
        /// <param name="obj">The object to write.</param>
        /// <exception cref="PacketOverflowException"></exception>
        public void WriteExactly<T>(T obj, ObjectSerializer<T> serializer)
        {
            WriteExactly(serializer.Serialize(obj));
        }

        #endregion
    }
}
