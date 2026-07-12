namespace OpeNetLib.Internals;

internal class PacketSplitter
{
    static uint _curPackID = 0;
    static uint NextPacketID() => ++_curPackID;
    public int MaxFragmentSize => _maxFragSize;
    readonly int _maxFragSize;
    internal PacketSplitter(int maxPacketSize)
    {
        _maxFragSize = PacketFragment.MIN_SIZE_BYTES;

        // packets must be at least one byte (shocking)
        if (_maxFragSize <= 0)
            throw new InvalidOperationException($"Cannot create a {nameof(PacketSplitter)} with a {nameof(maxPacketSize)} < {PacketFragment.MIN_SIZE_BYTES + 1}!");
    }

    internal PacketFragment[] SplitPacket(byte[] packet)
    {
        // TODO maybe bad
        int fragCt = (int)MathF.Ceiling((float)packet.Length / _maxFragSize);

        Console.WriteLine($"Splitting packet len {packet.Length} into {fragCt} frags!");
        uint id = NextPacketID();
        PacketFragment[] ret = new PacketFragment[fragCt];

        for (int start = 0, i = 0; i < ret.Length; start += _maxFragSize, ++i)
        {
            Range range = new(start, Math.Min(start + _maxFragSize, packet.Length));
            Console.WriteLine($"First frag is from {range.Start.GetOffset(packet.Length)} to {range.End.GetOffset(packet.Length)}");
            ret[i] = new(new(id, (ushort)i, (ushort)ret.Length, packet), packet[start..]);
        }

        return ret;
    }
}