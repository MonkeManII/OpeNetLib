using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace OpeNetLib.Internals;

internal class PacketMerger
{
    class PartialPacket
    {
        readonly PacketFragment[] _fragments;
        readonly bool[] _isRegistered;
        int _registeredCount = 0;

        public PartialPacket(PacketFragment first)
        {
            _isRegistered = new bool[first.header.FragmentCount];
            _fragments = new PacketFragment[first.header.FragmentCount];
            Console.WriteLine($"Registering first fragment...\nTotal fragment count: {first.Header.FragmentCount}!");
            Register(first);
        }

        public void Register(PacketFragment frag)
        {
            Console.WriteLine($"Registering packet fragment at index {frag.Header.FragmentIndex}!");

            if (_isRegistered[frag.header.FragmentIndex])
                throw new InvalidOperationException("Registered packet fragment attempted to overwrite another fragment!");

            if (frag.header.FragmentCount != _fragments.Length)
                throw new InvalidOperationException("Packet fragment does not match expected packet length!");
            _fragments[frag.header.FragmentIndex] = frag;
            _isRegistered[frag.header.FragmentIndex] = true;
            ++_registeredCount;
        }

        public int ToBytes(in byte[] output)
        {
            int ptr = 0;

            foreach (PacketFragment frag in _fragments)
            {
                Span<byte> payload = frag.Payload;
                Span<byte> copySpan = output.AsSpan()[ptr..(ptr + frag.Payload.Length)];
                payload.CopyTo(copySpan);
                ptr += payload.Length;
            }

            return ptr;
        }

        public byte[] ToBytes()
        {
            int len = 0;
            foreach (PacketFragment frag in _fragments)
                len += frag.Payload.Length;
            
            byte[] output = new byte[len];
            ToBytes(in output);
            return output;
        }

        public bool IsComplete => _registeredCount >= _fragments.Length;
    }

    readonly Dictionary<uint, PartialPacket> _partials = [];

    internal PacketMerger() { }

    public bool OnFragmentRecieved(PacketFragment frag, [NotNullWhen(true)] out byte[]? packet)
    {
        if (_partials.TryGetValue(frag.header.PacketIdentifier, out PartialPacket? p))
        {
            p.Register(frag);
        } else
        {
            p = new(frag);
            _partials[frag.header.PacketIdentifier] = p;
        }

        if (p.IsComplete)
        {
            packet = p.ToBytes();
            Console.WriteLine("Packet complete!\nPacket data:\n\t{0}", Encoding.UTF8.GetString(packet));
            _partials.Remove(frag.header.PacketIdentifier);
            return true;
        } else
        {
            packet = null;
            return false;
        }
    }
    
    public bool OnFragmentRecieved(byte[] frag, [NotNullWhen(true)] out byte[]? packet) => OnFragmentRecieved(new PacketFragment(frag), out packet);
}