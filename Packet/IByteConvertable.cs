namespace OpeNetLib.Packet
{
    public interface IByteConvertable<TThis>
        where TThis : IByteConvertable<TThis>
    {
        public abstract static TThis FromBytes(byte[] bytes);
        public byte[] ToBytes();
    }
}
