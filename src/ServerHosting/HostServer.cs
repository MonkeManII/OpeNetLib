using OpeNetLib.Internals;
using OpeNetLib.Packet;
using OpeNetLib.Threading;
using System.Net;

namespace OpeNetLib.ServerHosting
{
    public sealed class Server : IDisposable
    {
        public int Port => ConnectionReciever.RecieverPort;

        readonly TwoWayPollThread PollingThread;
        readonly UdpTwoWay ConnectionReciever;
        readonly PacketRecievedCallback Callback;
        readonly Dictionary<ClientReference, Connection> ConnectedClients;

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

        public async Task Send(byte[] data, ClientReference connectionID)
        {
            if (!ConnectedClients.TryGetValue(connectionID, out Connection? connection))
            {
                return;
            }
            await connection.Send(data);
        }

        public async Task Send(byte[] data, IPEndPoint endpoint)
        {
            await Send(data, new ClientReference(endpoint));
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
            
            // Why was this using parse before??? The data's RIGHT THERE :(
            IPEndPoint client = new (packet.Origin.Address, clientReceptionPort);
            UdpTwoWay clientInteractor = new(Callback);
            Connection newConnection = new(client, clientInteractor, 1000);

            PollingThread.AddPoll(clientInteractor);
            ConnectedClients.Add(new ClientReference(newConnection), newConnection);

            byte[] port = BitConverter.GetBytes(clientInteractor.RecieverPort);
            byte[] outPacket = new byte[1 + port.Length];
            outPacket[0] = 0;
            port.CopyTo(outPacket, 1);

            Console.WriteLine("Directing client to port {0}", clientInteractor.RecieverPort);

            await clientInteractor.Send(
                outPacket,
                client
            );
        }
    }
}
