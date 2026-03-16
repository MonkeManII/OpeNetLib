using OpeNetLib.Internals;
using OpeNetLib.Packet;
using OpeNetLib.Threading;
using System.Net;

namespace OpeNetLib
{
    public sealed class Server : IDisposable
    {
        public int Port => ConnectionReciever.RecieverPort;

        readonly TwoWayPollThread PollingThread;
        readonly UdpTwoWay ConnectionReciever;
        readonly PacketRecievedCallback Callback;
        readonly Dictionary<long, Connection> ConnectedClients;

        internal static long ConnectionID(Connection connection)
        {
            byte[] port = BitConverter.GetBytes((short)connection.RecieverEndpoint.Port);
            byte[] address = connection.RecieverEndpoint.Address.GetAddressBytes();
            
            byte[] bytes = new byte[8];
            port.CopyTo(bytes, 0);
            address.CopyTo(bytes, 2);

            return BitConverter.ToInt64(bytes);
        }

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

        public async Task Send(byte[] data, int connectionID)
        {
            if (!ConnectedClients.TryGetValue(connectionID, out Connection? connection))
            {
                // TODO debug log
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
            int clientReceptionPort = BitConverter.ToInt32(packet.Data, 1);
            
            IPEndPoint client = IPEndPoint.Parse($"{packet.Origin.Address}:{clientReceptionPort}");
            UdpTwoWay clientInteractor = new(Callback);
            Connection newConnection = new(client, clientInteractor, 1000);

            PollingThread.AddPoll(clientInteractor);
            ConnectedClients.Add(ConnectionID(newConnection), newConnection);

            byte[] port = BitConverter.GetBytes(clientInteractor.RecieverPort);
            byte[] outPacket = new byte[1 + port.Length];
            outPacket[0] = 0;
            port.CopyTo(outPacket, 1);

            await clientInteractor.Send(
                outPacket,
                client
            );
        }
    }
}
