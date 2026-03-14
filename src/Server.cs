using OpeNetLib.Internals;
using OpeNetLib.Packet;
using OpeNetLib.Threading;
using System.Net;
using System.Text;

namespace OpeNetLib
{
    public sealed class Server : IDisposable
    {
        public int Port => ConnectionReciever.RecieverPort;

        readonly UdpListenThread PollingThread;
        readonly UdpTwoWay ConnectionReciever;

        public async void Dispose()
        {
            ConnectionReciever.Dispose();
        }

        internal Server()
        {
            PollingThread = new();
            ConnectionReciever = new UdpTwoWay(ConnectionCallback);
            PollingThread.AddPoll(ConnectionReciever);
            PollingThread.StartThread();
        }
        
        internal async void ConnectionCallback(OriginPacket packet)
        {
            IPEndPoint ep = packet.Origin;

            UdpTwoWay clientInteractor = new(PacketCallback);
            int clientReceptionPort = BitConverter.ToInt32(packet.Data);

            PollingThread.AddPoll(clientInteractor);

            Console.WriteLine($"Connection from {packet.Origin}!");
            Console.WriteLine($"Client reception port: {clientReceptionPort}.");
            Console.WriteLine($"Server (this) reception port: {clientInteractor.RecieverPort}. Directing client to port...");

            await clientInteractor.Send(
                BitConverter.GetBytes(clientInteractor.RecieverPort),
                packet.Origin.Address.ToString(),
                clientReceptionPort
            );
        }

        internal static void PacketCallback(OriginPacket packet)
        {
            Console.WriteLine(Encoding.UTF8.GetString(packet.Data));
        }
    }
}
