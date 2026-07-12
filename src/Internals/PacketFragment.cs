namespace OpeNetLib.Internals;

internal class PacketFragment
{
    public const int MIN_SIZE_BYTES = FragmentHeader.SIZE_BYTES;
    internal readonly struct FragmentHeader
    {
        public static FragmentHeader Default = new();

        const int START_CHECKSUM = 0;
        const int START_PACKET_ID = START_CHECKSUM + sizeof(long);
        const int START_FRAG_IDX = START_PACKET_ID + sizeof(uint);
        const int START_FRAG_CT = START_FRAG_IDX + sizeof(ushort);
        public const int SIZE_BYTES = START_FRAG_CT + sizeof(ushort);
        
        public readonly bool IsValid => _compChecksum == _intendedChecksum;
        readonly long _compChecksum;
        readonly long _intendedChecksum;

        public readonly uint PacketIdentifier;
        public readonly ushort FragmentIndex;
        public readonly ushort FragmentCount;

        internal FragmentHeader(byte[] fullPacketData)
        {
            // wow unreadable code!!!1!!!!1
            Span<byte> dat = fullPacketData.AsSpan();
            Console.WriteLine($"Constructing a header from {dat.Length} bytes!");
            _intendedChecksum = BitConverter.ToInt64(dat[START_CHECKSUM..START_PACKET_ID]);
            PacketIdentifier = BitConverter.ToUInt32(dat[START_PACKET_ID..START_FRAG_IDX]);
            FragmentIndex = BitConverter.ToUInt16(dat[START_FRAG_IDX..START_FRAG_CT]);
            FragmentCount = BitConverter.ToUInt16(dat[START_FRAG_CT..SIZE_BYTES]);
            _compChecksum = Checksum.Compute(dat[SIZE_BYTES..]);
        }

        internal FragmentHeader(uint packetId, ushort fragIdx, ushort fragCt, byte[] payload)
        {
            _intendedChecksum = Checksum.Compute(payload);
            _compChecksum = _intendedChecksum;
            PacketIdentifier = packetId;
            FragmentIndex = fragIdx;
            FragmentCount = fragCt;
        }

        internal readonly byte[] AsBytes()
        {
            byte[] asByte = new byte[SIZE_BYTES];
            BitConverter.GetBytes(_intendedChecksum).CopyTo(asByte, START_CHECKSUM);
            BitConverter.GetBytes(PacketIdentifier).CopyTo(asByte, START_PACKET_ID);
            BitConverter.GetBytes(FragmentIndex).CopyTo(asByte, START_FRAG_IDX);
            BitConverter.GetBytes(FragmentCount).CopyTo(asByte, START_FRAG_CT);
            return asByte;
        }

        internal readonly void AsBytes(byte[] copyTo, int startIndex)
        {
            BitConverter.GetBytes(_intendedChecksum).CopyTo(copyTo, START_CHECKSUM + startIndex);
            BitConverter.GetBytes(PacketIdentifier).CopyTo(copyTo, START_PACKET_ID + startIndex);
            BitConverter.GetBytes(FragmentIndex).CopyTo(copyTo, START_FRAG_IDX + startIndex);
            BitConverter.GetBytes(FragmentCount).CopyTo(copyTo, START_FRAG_CT + startIndex);
        }

        public FragmentHeader()
        {
            
        }
    }

    static readonly byte[] NO_DATA = [];
    
    public Span<byte> Payload => data.AsSpan()[FragmentHeader.SIZE_BYTES..];
    internal readonly byte[] data;
    public FragmentHeader Header => header;
    internal readonly FragmentHeader header;

    public PacketFragment()
    {
        data = NO_DATA;
        header = FragmentHeader.Default;
    }

    public PacketFragment(byte[] fragdat)
    {
        header = new(fragdat);
        data = fragdat;
    }

    public PacketFragment(FragmentHeader header, byte[] payload)
    {
        this.header = header;
        data = new byte[payload.Length + FragmentHeader.SIZE_BYTES];
        header.AsBytes(data, 0);
        payload.CopyTo(data, FragmentHeader.SIZE_BYTES);
    }
}