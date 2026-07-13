using OpeNetLib.Internals;
using OpeNetLib.Packet;
using OpeNetLib.Threading;
using System.Net;

namespace OpeNetLib
{
    /// <summary>
    /// Represents a UDP server that can connect to and negotiate with clients.
    /// </summary>
    public sealed class Server : IDisposable
    {
        /// <summary>
        /// The port used for negotiating with new clients.
        /// </summary>
        public int NegotiatorPort => ConnectionReciever.RecieverPort;

        /// <summary>
        /// The thread used for polling new packets.
        /// </summary>
        readonly TwoWayPollThread PollingThread;

        /// <summary>
        /// The reciever used for connecting with new clients.
        /// </summary>
        /// <remarks>
        /// This is what <see cref="Client.RequestConnect"/> expects.
        /// </remarks>
        readonly UdpTwoWay ConnectionReciever;

        /// <summary>
        /// The callback to use when a packet is recieved.
        /// </summary>
        readonly PacketRecievedCallback Callback;

        /// <summary> 
        /// Represents 
        /// </summary>
        readonly Dictionary<EndpointIdentifier, Connection> ConnectedClients;

        public async void Dispose()
        {
            ConnectionReciever.Dispose();
        }

        internal void UpdateTimeout(UdpMessageThreadParam _)
        {
            foreach (Connection con in ConnectedClients.Values)
            {
                con.UpdateTimeout();
            }
        }

        public Server(PacketRecievedCallback PacketInterpreter)
        {
            PollingThread = new(id: "Server Listen Thread", server: this, callback: UpdateTimeout);
            ConnectedClients = [];
            ConnectionReciever = new UdpTwoWay(ConnectionCallback);
            Callback = PacketInterpreter;
            PollingThread.AddPoll(ConnectionReciever);
            PollingThread.StartThread();
        }

        public async Task Send(byte[] data, EndpointIdentifier connectionID)
        {
            if (!ConnectedClients.TryGetValue(connectionID, out Connection? connection))
            {
                return;
            }
            await connection.Send(data);
        }

        public async Task Broadcast(byte[] data)
        {
            foreach (Connection client in ConnectedClients.Values)
            {
                await client.Send(data);
            }
        }

        internal async void ConnectionCallback(PacketCallbackParam packet)
        {
            PacketDestructor reader = new(packet.Data);
            
            // 0 is the negotiation packet
            if (reader.ReadByte() != 0) return;

            ushort clientReceptionPort = reader.ReadUShort();

            IPEndPoint client = new (packet.Origin.Address, clientReceptionPort);
            UdpTwoWay clientInteractor = new(Callback);
            Connection newConnection = new(client, clientInteractor, 1000);

            PollingThread.AddPoll(clientInteractor);
            ConnectedClients.Add(
                EndpointIdentifier.FromEndpoint(newConnection.RecieverEndpoint),
                newConnection
            );

            PacketConstructor affirmer = new(5);
            affirmer.WriteByte(0);
            affirmer.WriteUShort((ushort)clientInteractor.RecieverPort);

            await clientInteractor.Send(
                affirmer.ResultBytes(),
                client
            );
        }
    }
}
