using System.Net;
using OpeNetLib.Internals;

namespace OpeNetLib;

public class ClientReference : IEquatable<ClientReference>
{
    readonly byte[] brep;

    internal ClientReference(Connection c)
    {
        brep = GetBytes(c.RecieverEndpoint.Address);
    }

    public ClientReference(IPAddress c)
    {
        brep = GetBytes(c);
    }

    public ClientReference(IPEndPoint c)
    {
        brep = GetBytes(c.Address);
    }

    static byte[] GetBytes(IPAddress ep)
    {
        byte[] addr = ep.GetAddressBytes();

        byte[] r = new byte[addr.Length + 1];
        r[0] = (byte)addr.Length;
        addr.CopyTo(r, 1);

        return r;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not ClientReference i)
            return false;
        return Equals(i);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (int)Checksum.Compute(brep);
        }
    }

    public bool Equals(ClientReference? other)
    {
        if (other is null) return false;
        if (brep.Length != other.brep.Length) return false;
        for (int i = 0; i < brep.Length; ++i)
            if (brep[i] != other.brep[i]) return false;
        return true;
    }

    public static bool operator ==(ClientReference l, ClientReference r)
    {
        return l.Equals(r);
    }

    public static bool operator !=(ClientReference l, ClientReference r)
    {
        return !l.Equals(r);
    }
}