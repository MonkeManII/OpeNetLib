using OpeNetLib.Internals;
using OpeNetLib.Packet;
using OpeNetLib.Threading;
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
        readonly UdpPollThread PollingThread;

        /// <summary>
        /// The two-way connection for interacting with the server.
        /// </summary>
        readonly UdpTwoWay ServerInteractor;

        /// <summary>
        /// The callback to call when a packet is recieved.
        /// </summary>
        readonly PacketRecievedCallback Callback;

        /// <summary>
        /// The <see cref="IPEndPoint"/> of the server that this is connected to.
        /// </summary>
        public IPEndPoint? ServerEndPoint { get; private set; }

        /// <summary>
        /// Whether the client-server handshake has finished, and communications are complete.
        /// </summary>
        bool portConfirmed = false;

        public async void Dispose()
        {
            ServerInteractor.Dispose();
        }

        /// <summary>
        /// Creates a new <see cref="Client"/> with the specified callback.
        /// </summary>
        /// <param name="PacketInterpreter">The callback to call whenever a packet is recieved.</param>
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

        async void PacketCallback(PacketCallbackParam packet)
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
