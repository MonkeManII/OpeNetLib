using OpeNetLib.Internals;
using OpeNetLib.Packet;
using OpeNetLib.Threading;
using System.Net;

namespace OpeNetLib
{
    public sealed class Server : IDisposable
    {
        public int Port => ConnectionReciever.RecieverPort;

        readonly UdpPollThread PollingThread;
        readonly UdpTwoWay ConnectionReciever;
        readonly PacketRecievedCallback Callback;
        readonly Dictionary<IPEndPoint, UdpTwoWay> ConnectedClients;

        public async void Dispose()
        {
            ConnectionReciever.Dispose();
        }

        public Server(PacketRecievedCallback PacketInterpreter)
        {
            PollingThread = new("Server Listen Thread");
            ConnectedClients = [];
            ConnectionReciever = new UdpTwoWay(ConnectionCallback);
            Callback = PacketInterpreter;
            PollingThread.AddPoll(ConnectionReciever);
            PollingThread.StartThread();
        }

        public async Task Send(byte[] data, IPEndPoint connection)
        {
            if (!ConnectedClients.TryGetValue(connection, out UdpTwoWay? messenger))
            {
                // TODO debug log
                return;
            }
            await messenger.Send(data, connection);
        }

        public async Task Broadcast(byte[] data)
        {
            foreach (KeyValuePair<IPEndPoint, UdpTwoWay> client in ConnectedClients)
            {
                await client.Value.Send(data, client.Key);
            }
        }

        internal async void ConnectionCallback(PacketCallbackParam packet)
        {
            int clientReceptionPort = BitConverter.ToInt32(packet.Data, 1);
            
            IPEndPoint client = IPEndPoint.Parse($"{packet.Origin.Address}:{clientReceptionPort}");
            UdpTwoWay clientInteractor = new(Callback);

            PollingThread.AddPoll(clientInteractor);
            ConnectedClients.Add(client, clientInteractor);

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
