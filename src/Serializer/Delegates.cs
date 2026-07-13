namespace OpeNetLib.Serializer
{
    public delegate T Deserialize<T>(Span<byte> bytes);
    public delegate byte[] Serialize<T>(T obj);
}
