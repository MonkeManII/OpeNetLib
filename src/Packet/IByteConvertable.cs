namespace OpeNetLib.Packet
{
    [Obsolete("OpeNetLib's binary serializer has been deprecated. Use an external library instead.")]
    public interface IByteConvertable<TThis>
        where TThis : IByteConvertable<TThis>
    {
        public abstract static TThis FromBytes(byte[] bytes);
        public abstract static TThis FromBytes(Span<byte> bytes);
        public byte[] ToBytes();
        public Span<byte> ToByteSpan();
    }
}
