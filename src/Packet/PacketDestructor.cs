
using System.Text;

namespace OpeNetLib.Packet
{
    /// <summary>
    /// A class used to destruct <see cref="byte[]"/> packets into objects.
    /// </summary>
    [Obsolete("OpeNetLib's binary serializer has been deprecated. Use an external library instead.")]
    public class PacketDestructor
    {
        /// <summary>
        /// The space remaining in this <see cref="byte[]"/> packet.
        /// </summary>
        public int RemainingBytes => packet.Length - position;

        /// <summary>
        /// The pointer into the packet array to read/write from.
        /// </summary>
        int position;

        /// <summary>
        /// The byte data of this packet.
        /// </summary>
        readonly byte[] packet;

        public PacketDestructor(byte[] bytes)
        {
            packet = bytes;

            // skip type byte
            position = 1;
        }

        /// <summary>
        /// Returns the type identifier of this packet.
        /// </summary>
        /// <returns>A <see cref="byte"/> representing the type of this packet.</returns>
        public byte PacketType()
        {
            return packet[0];
        }

        /// <summary>
        /// Reads the next <typeparamref name="T"/> from the packet, and advances <see cref="position"/> by the number of bytes read.
        /// </summary>
        /// <typeparam name="T">The type to read.</typeparam>
        /// <returns>A <typeparamref name="T"/> constructed from <see cref="IByteConvertable{TThis}.FromBytes(Span{byte})"/></returns>
        public T Read<T>() 
            where T : IByteConvertable<T>
        {
            return T.FromBytes(ReadBytes());
        }

        /// <summary>
        /// Reads the next <see cref="Span{byte}"/> from the packet.
        /// </summary>
        /// <returns>A <see cref="Span{byte}"/> read from the packet.</returns>
        public Span<byte> ReadBytes()
        {
            Span<byte> pkSpan = packet.AsSpan();

            int len = BitConverter.ToUInt16(pkSpan.Slice(position, 2));
            int oldPos = position + 2;
            position += 2 + len;

            return pkSpan.Slice(oldPos, len);
        }

        /// <summary>
        /// Reads the next <paramref name="count"/> bytes from the packet as a <see cref="Span{byte}"/>.
        /// </summary>
        /// <returns>A <see cref="Span{byte}"/> read from the packet.</returns>
        public Span<byte> ReadExactly(int count)
        {
            Span<byte> pkSpan = packet.AsSpan();

            int oldPos = position;
            position += count;

            return pkSpan.Slice(oldPos, count);
        }

        #region Specialized Read Functions

        /// <summary>
        /// Reads the next <see cref="Span{byte}"/> from the packet as a UTF8 string.
        /// </summary>
        /// <returns>A UTF8 <see cref="string"/> from the next <see cref="Span{byte}"/> of the packet.</returns>
        public string ReadUTF8() => Encoding.UTF8.GetString(ReadBytes());

        /// <summary>
        /// Reads the next <see cref="Span{byte}"/> from the packet as a UTF32 string.
        /// </summary>
        /// <returns>A UTF32 <see cref="string"/> from the next <see cref="Span{byte}"/> of the packet.</returns>
        public string ReadUTF32() => Encoding.UTF32.GetString(ReadBytes());

        /// <summary>
        /// Reads the next <see cref="Span{byte}"/> from the packet as an ASCII string.
        /// </summary>
        /// <returns>An ASCII <see cref="string"/> from the next <see cref="Span{byte}"/> of the packet.</returns>
        public string ReadASCII() => Encoding.ASCII.GetString(ReadBytes());

        /// <summary>
        /// Reads the next 8 bytes from the packet as a <see cref="ulong"/>.
        /// </summary>
        /// <returns>A <see cref="ulong"/> from the next 8 bytes of the packet.</returns>
        public ulong ReadULong() => BitConverter.ToUInt64(ReadExactly(sizeof(ulong)));

        /// <summary>
        /// Reads the next 4 bytes from the packet as a <see cref="uint"/>.
        /// </summary>
        /// <returns>A <see cref="uint"/> from the next 4 bytes of the packet.</returns>
        public uint ReadUInt() => BitConverter.ToUInt32(ReadExactly(sizeof(uint)));

        /// <summary>
        /// Reads the next 2 bytes from the packet as a <see cref="ushort"/>.
        /// </summary>
        /// <returns>A <see cref="ushort"/> from the next 2 bytes of the packet.</returns>
        public ushort ReadUShort() => BitConverter.ToUInt16(ReadExactly(sizeof(ushort)));

        /// <summary>
        /// Reads the next byte from the packet as a <see cref="sbyte"/>.
        /// </summary>
        /// <returns>A <see cref="sbyte"/> from the next byte of the packet.</returns>
        public sbyte ReadSByte() => (sbyte)ReadExactly(1)[0];

        /// <summary>
        /// Reads the next 8 bytes from the packet as a <see cref="long"/>.
        /// </summary>
        /// <returns>A <see cref="long"/> from the next 8 bytes of the packet.</returns>
        public long ReadLong() => BitConverter.ToInt64(ReadExactly(sizeof(long)));

        /// <summary>
        /// Reads the next 4 bytes from the packet as an <see cref="int"/>.
        /// </summary>
        /// <returns>An <see cref="int"/> from the next 4 bytes of the packet.</returns>
        public int ReadInt() => BitConverter.ToInt32(ReadExactly(sizeof(int)));

        /// <summary>
        /// Reads the next 2 bytes from the packet as a <see cref="short"/>.
        /// </summary>
        /// <returns>A <see cref="short"/> from the next 2 bytes of the packet.</returns>
        public short ReadShort() => BitConverter.ToInt16(ReadExactly(sizeof(short)));

        /// <summary>
        /// Reads the next <see cref="byte"/> from the packet.
        /// </summary>
        /// <returns>The next <see cref="byte"/> of the packet.</returns>
        public byte ReadByte() => ReadExactly(1)[0];

        /// <summary>
        /// Reads the next 4 bytes from the packet as a <see cref="float"/>.
        /// </summary>
        /// <returns>The next <see cref="float"/> of the packet.</returns>
        public float ReadFloat() => BitConverter.ToSingle(ReadExactly(sizeof(float)));

        /// <summary>
        /// Reads the next 8 bytes from the packet as a <see cref="double"/>.
        /// </summary>
        /// <returns>The next <see cref="double"/> of the packet.</returns>
        public double ReadDouble() => BitConverter.ToDouble(ReadExactly(sizeof(double)));

        /// <summary>
        /// Reads the next 2 bytes from the packet as a <see cref="Half"/>.
        /// </summary>
        /// <returns>The next <see cref="Half"/> of the packet.</returns>
        // WHY IS sizeof(Half) not a thing? Hello???
        // whatever, if .NET changes their Half implementation,
        // I (and the five other people that use Halfs [halves?]) are cooked.
        public Half ReadHalf() => BitConverter.ToHalf(ReadExactly(2));

        #endregion
    }
}
