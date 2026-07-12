namespace OpeNetLib.Internals;

public static class Checksum
{
    public static long Compute(ReadOnlySpan<byte> data)
    {
        unchecked
        {
            long r = long.MinValue;
            foreach (byte b in data)
                r += b;
            return r;   
        }
    }
}