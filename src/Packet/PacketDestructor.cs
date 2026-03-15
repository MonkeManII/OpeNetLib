
namespace OpeNetLib.Packet
{
    /// <summary>
    /// A class used to destruct <see cref="byte[]"/> packets into objects.
    /// </summary>
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
    }
}
