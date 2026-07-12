using System.Net;
using System.Net.Sockets;

namespace OpeNetLib.Internals
{
    /// <summary>
    /// Sends UDP packets to a remote device.
    /// </summary>
    internal sealed class UdpSender : IDisposable
    {
        const int MAX_SPLIT_PACKET_SIZE = 0b0000_1000_0000_0000;

        private readonly UdpClient _udpClient;
        internal readonly string SendIP;
        internal readonly int SendPort;
        internal readonly IPEndPoint EndPoint;
        readonly PacketSplitter _splitter;

        /// <summary>
        /// Creates a new <see cref="UdpSender"/> connected to a specified address.
        /// </summary>
        /// <param name="toIp">The IPv4 address to direct this sender's packets to.</param>
        /// <param name="toPort">The port to direct the packets to.</param>
        internal UdpSender(string toIp, int toPort) : this(IPEndPoint.Parse($"{toIp}:{toPort}")) { }

        /// <summary>
        /// Creates a new <see cref="UdpSender"/> connected to a specified IP endpoint.
        /// </summary>
        /// <param name="address">The endpoint to direct this sender's packets to.</param>
        internal UdpSender(IPEndPoint address)
        {
            SendIP = address.Address.ToString();
            SendPort = address.Port;
            EndPoint = address;

            _udpClient = new();
            _udpClient.Connect(address.Address, address.Port);

            _splitter = new(MAX_SPLIT_PACKET_SIZE);
        }

        public void Dispose()
        {
            _udpClient.Close();
            _udpClient.Dispose();
        }

        /// <summary>
        /// Sends a <seealso cref="byte"/>[] to the remote server.
        /// </summary>
        /// <param name="data">The data to send to the server.</param>
        /// <returns>
        ///     The <seealso cref="int"/> returned by
        ///     <see cref="UdpClient.SendAsync(byte[], int)"/>.
        ///     <para>
        ///         Possibly an opcode of sorts.
        ///     </para>
        /// </returns>
        /// <todo>Research more into what the int is.</todo>
        internal async Task<int> Send(byte[] data)
        {
            // is this an opcode? idk
            PacketFragment[] frags = _splitter.SplitPacket(data);
            int opcode = 0;
            foreach (PacketFragment f in frags)
            {
                opcode = await _udpClient.SendAsync(f.data, f.data.Length);
            }
            return opcode;
        }
    }
}
