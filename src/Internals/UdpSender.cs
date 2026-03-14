using System.Net;
using System.Net.Sockets;

namespace OpeNetLib.Internals
{
    /// <summary>
    /// Sends UDP packets to a remote device.
    /// </summary>
    public sealed class UdpSender : IDisposable
    {
        private readonly UdpClient _udpClient;
        public readonly string SendIP;
        public readonly int SendPort;
        public readonly IPEndPoint EndPoint;

        /// <summary>
        /// Creates a new <see cref="UdpSender"/> connected to a specified address.
        /// </summary>
        /// <param name="toIp">The IPv4 address to direct this sender's packets to.</param>
        /// <param name="toPort">The port to direct the packets to.</param>
        public UdpSender(string toIp, int toPort) : this(IPEndPoint.Parse($"{toIp}:{toPort}")) { }

        /// <summary>
        /// Creates a new <see cref="UdpSender"/> connected to a specified IP endpoint.
        /// </summary>
        /// <param name="address">The endpoint to direct this sender's packets to.</param>
        public UdpSender(IPEndPoint address)
        {
            SendIP = address.Address.ToString();
            SendPort = address.Port;
            EndPoint = address;

            _udpClient = new();
            _udpClient.Connect(address.Address, address.Port);
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
        public async Task<int> Send(byte[] data)
        {
            // is this an opcode?
            int opcode = await _udpClient.SendAsync(data, data.Length);
            return opcode;
        }
    }
}
