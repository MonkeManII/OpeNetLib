using OpeNetLib.Internals;
using OpeNetLib.Packet;
using OpeNetLib.Threading;
using System.Net;

namespace OpeNetLib
{
    public sealed class Client : IDisposable
    {
        readonly UdpPollThread PollingThread;
        readonly UdpTwoWay ServerInteractor;
        readonly PacketRecievedCallback Callback;
        public IPEndPoint? ServerEndPoint { get; private set; }
        bool portConfirmed = false;

        public async void Dispose()
        {
            ServerInteractor.Dispose();
        }

        public Client(PacketRecievedCallback PacketInterpreter)
        {
            ServerEndPoint = null;
            PollingThread = new("Client Listen Thread");
            ServerInteractor = new UdpTwoWay(PacketCallback);
            Callback = PacketInterpreter;
            PollingThread.AddPoll(ServerInteractor);
            PollingThread.StartThread();
        }

        public async Task<bool> RequestConnect(IPEndPoint endpoint, int timeout)
        {
            ServerEndPoint = endpoint;
            portConfirmed = false;

            byte[] recieverPort = BitConverter.GetBytes(ServerInteractor.RecieverPort);
            byte[] packet = new byte[recieverPort.Length + 1];
            packet[0] = 0;
            recieverPort.CopyTo(packet, 1);

            await ServerInteractor.Send(packet, endpoint);
            int ticker = 0;

            while (!portConfirmed && ticker < timeout)
            {
                ++ticker;
                await Task.Delay(1);
            }

            return portConfirmed;
        }

        public async void Send(byte[] data)
        {
            if (ServerEndPoint is null) return;
            await ServerInteractor.Send(data, ServerEndPoint);
        }

        async void PacketCallback(OriginPacket packet)
        {
            if (packet.Data[0] == 0)
            {
                if (ServerEndPoint is null) return;
                if (portConfirmed) return;

                int port = BitConverter.ToInt32(packet.Data.AsSpan()[1..5]);
                ServerEndPoint = IPEndPoint.Parse($"{ServerEndPoint.Address}:{port}");
                portConfirmed = true;
                ServerInteractor.SetPacketCallback(Callback);
            }
        }
    }
}
