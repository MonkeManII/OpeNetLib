using OpeNetLib.Packet;
using System.Net;

namespace OpeNetLib.Internals
{
    internal sealed class UdpTwoWay : IDisposable
    {
        public void SetPacketCallback(PacketRecievedCallback callback)
        {
            OnPacketRecieved = callback;
        }

        public async void Dispose()
        {
            _reciever.Dispose();
            _sender?.Dispose();
        }

        /// <summary>
        /// The port the reciever listens to.
        /// </summary>
        public int RecieverPort => _reciever.Port;

        /// <summary>
        /// The callback called when a packet is recieved.
        /// </summary>
        private PacketRecievedCallback OnPacketRecieved;
        
        /// <summary>
        /// The reciever used to listen for packets.
        /// </summary>
        private readonly UdpReciever _reciever;

        /// <summary>
        /// The sender used to send packets.
        /// </summary>
        private UdpSender? _sender;

        /// <summary>
        /// Creates a new <see cref="UdpTwoWay"/> using the first available port.
        /// </summary>
        /// <param name="onPacketRecieved">The <see cref="PacketRecievedCallback"/> to call when a packet is recieved.</param>
        public UdpTwoWay(PacketRecievedCallback onPacketRecieved)
        {
            OnPacketRecieved = onPacketRecieved;
            _reciever = new();
        }

        public int ListenForPackets(Client? client, Server? server)
        {
            int packets = 0;

            PacketCallbackParam? recievedPacket;

            do
            {
                recievedPacket = _reciever.RecieveFirst();

                if (recievedPacket is not null)
                {
                    recievedPacket.Server ??= server;
                    recievedPacket.Client ??= client;

                    OnPacketRecieved(recievedPacket);
                    ++packets;
                }
            }
            while (recievedPacket is not null);

            return packets;
        }

        /// <summary>
        /// Sends a packet of data to the specified IPv4 address and port.
        /// </summary>
        /// <param name="bytes">The <seealso cref="byte"/>[] to send to the remote server.</param>
        /// <param name="ip">The IP address to send the packet to.</param>
        /// <param name="port">The port to send the packet to.</param>
        /// <returns>A <see cref="Task"/> that completes when the packet has sent.</returns>
        /// <remarks>
        ///     <para>
        ///         Using one <see cref="UdpTwoWay"/> to send to various addresses causes repeated connections and disconnections.
        ///     </para>
        ///     <para>
        ///         In general, this should be avoided - use multiple if sending to many IPs.
        ///     </para>
        /// </remarks>
        public async Task Send(byte[] bytes, string ip, int port)
        {
            if (_sender is null || !(_sender.SendIP == ip && _sender.SendPort == port))
            {
                _sender?.Dispose();
                _sender = new(ip, port);
            }
            await _sender.Send(bytes);
        }

        /// <summary>
        /// Sends a packet of data to the specified IPv4 address and port.
        /// </summary>
        /// <param name="bytes">The <seealso cref="byte"/>[] to send to the remote server.</param>
        /// <param name="endPoint">The endpoint to send the packet to.</param>
        /// <returns>A <see cref="Task"/> that completes when the packet has sent.</returns>
        /// <remarks>
        ///     <para>
        ///         Using one <see cref="UdpTwoWay"/> to send to various addresses causes repeated connections and disconnections.
        ///     </para>
        ///     <para>
        ///         In general, this should be avoided - use multiple if sending to many IPs.
        ///     </para>
        /// </remarks>
        public async Task Send(byte[] bytes, IPEndPoint endPoint)
        {
            if (_sender is null || !(_sender.EndPoint == endPoint))
            {
                _sender?.Dispose();
                _sender = new(endPoint);
            }
            await _sender.Send(bytes);
        }
    }
}
