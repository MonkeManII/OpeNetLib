using OpeNetLib.Internals;
using OpeNetLib.Packet;
using OpeNetLib.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace OpeNetLib
{
    /// <summary>
    /// Represents a UDP client that can connect to one server.
    /// </summary>
    public sealed class Client : IDisposable
    {
        /// <summary>
        /// The polling thread for packet polling.
        /// </summary>
        readonly TwoWayPollThread PollingThread;

        /// <summary>
        /// The permanent <see cref="Connection"/> to use for long-term communications.
        /// </summary>
        public Connection? PermanentConnection { get; private set; }

        /// <summary>
        /// The two-way connection for negotiating with the server.
        /// </summary>
        readonly UdpTwoWay Negotiator;

        /// <summary>
        /// The callback to call when a packet is recieved.
        /// </summary>
        readonly PacketRecievedCallback Callback;

        /// <summary>
        /// The IPAddress of the connected server.
        /// </summary>
        IPAddress? ServerAddress;

        /// <summary>
        /// The <see cref="IPEndPoint"/> of the server that this is connected to.
        /// </summary>
#pragma warning disable CS0618
        [Obsolete($"{nameof(ServerEndPoint)} is obsolete. Please use {nameof(PermanentConnection)}.{nameof(PermanentConnection.RecieverEndpoint)}")]
        public IPEndPoint? ServerEndPoint => PermanentConnection?.RecieverEndpoint;
#pragma warning restore CS0618

        /// <summary>
        /// Whether the client-server handshake has finished, and communications are complete.
        /// </summary>
        public bool NegotiationsFinished { get; private set; } = false;

        public async void Dispose()
        {
            Negotiator.Dispose();
        }

        /// <summary>
        /// Creates a new <see cref="Client"/> with the specified callback.
        /// </summary>
        /// <param name="PacketInterpreter">The callback to call whenever a packet is recieved.</param>
        public Client(PacketRecievedCallback PacketInterpreter)
        {
            PollingThread = new(id: "Client Listen Thread", client: this, callback: UpdateTimeout);
            Negotiator = new UdpTwoWay(NegotiatorCallback);
            Callback = PacketInterpreter;
            PollingThread.AddPoll(Negotiator);
            PollingThread.StartThread();
        }

        void UpdateTimeout(UdpMessageThreadParam _) => PermanentConnection?.UpdateTimeout();

        /// <summary>
        /// Requests a connection to a <see cref="Server"/>.
        /// </summary>
        /// <param name="endpoint">The <see cref="IPEndPoint"/> of the server.</param>
        /// <param name="timeout">The maximum time, in milliseconds, before this request times out.</param>
        /// <returns>Whether the request for connection was successful or not.</returns>
        [MemberNotNullWhen(true, nameof(PermanentConnection))]
        [MemberNotNull(nameof(ServerAddress))]
        public async Task<bool> RequestConnect(IPEndPoint endpoint, int timeout)
        {
            NegotiationsFinished = false;
            ServerAddress = endpoint.Address;

            PacketConstructor negotiator = new(5);
            negotiator.WriteByte(0);
            negotiator.WriteUShort((ushort)Negotiator.RecieverPort);

            await Negotiator.Send(negotiator.ResultBytes(), endpoint);
            int ticker = 0;

            while (!NegotiationsFinished && ticker < timeout)
            {
                ++ticker;
                await Task.Delay(1);
            }

            return NegotiationsFinished;
        }

        /// <summary>
        /// Sends a <see cref="byte"/>[] packet over the <see cref="PermanentConnection"/>.
        /// </summary>
        /// <param name="data">The packet to send.</param>
        public async void Send(byte[] data)
        {
            if (PermanentConnection is null) throw new InvalidOperationException($"Cannot call {nameof(Send)} when {nameof(PermanentConnection)} is null!");
            await PermanentConnection.Send(data);
        }

        async void NegotiatorCallback(PacketCallbackParam packet)
        {
            // Can't negotiate with nothing.
            if (ServerAddress is null) return;

            PacketDestructor destructor = new(packet.Data);

            // 0 is the negotiation packet
            if (destructor.ReadByte() != 0) return;

            // If we've already connected to a permanent connection point, ignore this request.
            if (PermanentConnection is not null) return;
            if (NegotiationsFinished) return;

            // Get the port as specified in the packet.
            ushort port = destructor.ReadUShort();
            NegotiationsFinished = true;
            
            // The negotiator is ready for normal packet callbacks.
            Negotiator.SetPacketCallback(Callback);

            // Repurpose the negotiator as a permanent communications two-way.
            PermanentConnection = new(new IPEndPoint(ServerAddress, port), Negotiator, 1000);
        }
    }
}
