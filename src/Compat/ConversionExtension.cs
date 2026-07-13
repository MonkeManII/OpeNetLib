namespace OpeNetLib.Compat
{
    /// <summary>
    /// A set of extensions to <see cref="BitConverter"/> that implements some post .NET 8 functionality.
    /// </summary>
    internal static class ConversionExtension
    {
        public static Int128 ToInt128(Span<byte> bytes)
        {
#if NET9_0_OR_GREATER
            return BitConverter.ToInt128(bytes);
#else
            return new Int128(BitConverter.ToUInt64(bytes[0..8]), BitConverter.ToUInt64(bytes[9..16]));
#endif
        }

        // keep "unsafe" here, because the implementation for under .NET 9.0 requires it.
        public static unsafe byte[] GetBytes(Int128 int128)
        {
#if NET9_0_OR_GREATER
            return BitConverter.GetBytes(int128);
#else
            // TODO test???
            byte[] numArray = new byte[16];
            fixed (byte* numPtr = numArray)
                *(Int128*)numPtr = int128;
            return numArray;
#endif
        }
    }
}
