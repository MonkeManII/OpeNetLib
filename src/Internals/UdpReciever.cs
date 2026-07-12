using OpeNetLib.Packet;
using System.Net;
using System.Net.Sockets;

namespace OpeNetLib.Internals
{
    /// <summary>
    /// Represents a port on this machine that can recieve UDP packets.
    /// </summary>
    internal sealed class UdpReciever : IDisposable
    {
        /// <summary>
        /// The <see cref="UdpClient"/> that runs all the details behind-the-scenes.
        /// </summary>
        private readonly UdpClient _udpClient;

        /// <summary>
        /// The port that this reciever is bound to.
        /// </summary>
        internal readonly int Port;

        /// <summary>
        /// The 
        /// </summary>
        readonly PacketMerger _fragManager;

        /// <summary>
        /// Creates a new UdpReciever bound to the specified port.
        /// </summary>
        /// <param name="port">The port to bind this reciever to, or null if first available.</param>
        internal UdpReciever(int? port = null)
        {
            // Port 0 means "first open port".
            int nonNullPort = port is null ? 0 : (int)port;
            _udpClient = new UdpClient(nonNullPort);

            // Because 0 means any, we have to now check manually instead of Port = nonNullPort.
            IPEndPoint? ep = (IPEndPoint?)_udpClient.Client.LocalEndPoint;
            if (ep is not null)
            {
                Port = ep.Port;
            } else
            {
                Port = 0;
            }

            _fragManager = new();
        }

        /// <summary>
        /// Waits until this reciever recieves a packet, and returns the contents of said packet.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{}>"/> that completes when a packet is recieved.
        /// </returns>
        internal async Task<PacketCallbackParam> ReceiveFirstAsync(int? msTimeout = null)
        {

            using CancellationTokenSource tokenSrc = new();
            if (msTimeout is not null)
                tokenSrc.CancelAfter((int)msTimeout);

            byte[]? result = null;
            UdpReceiveResult? sucRes = null;

            while (!tokenSrc.IsCancellationRequested)
            {
                sucRes = await _udpClient.ReceiveAsync(tokenSrc.Token);
                Console.WriteLine("Fragment recieved!");
                if (sucRes.HasValue && _fragManager.OnFragmentRecieved(sucRes.Value.Buffer, out result)) break;
                Console.WriteLine("Continuing...");
            }

            // TODO make cancellation tokens work :O
            if (sucRes == null || result == null)
                throw new Exception($"How did you get it to do this??? (Presumably you canceled your call to {nameof(ReceiveFirstAsync)}. Don't do that.)");

            return new PacketCallbackParam(sucRes.Value.RemoteEndPoint, result, null, null);
        }

        /// <summary>
        ///     Recieves the first <see cref="PacketCallbackParam"/> queued for recieving.
        ///     <para>
        ///         Will stop further code from running until the packet is recieved.
        ///     </para>
        /// </summary>
        /// <returns>The first <see cref="PacketCallbackParam"/> in reception, or null if none.</returns>
        internal PacketCallbackParam RecieveFirst()
        {
            // this can't be cancelled, we got all damn day
            IPEndPoint? _outEP = null;
            while (true)
            {
                byte[] bytes = _udpClient.Receive(ref _outEP);
                if (_outEP != null)
                {
                    if (_fragManager.OnFragmentRecieved(bytes, out byte[]? packet))
                    {
                        return new(_outEP, packet, null, null);
                    }
                }
                Thread.Sleep(0);
            }
        }

        public void Dispose()
        {
            _udpClient.Close();
            _udpClient.Dispose();
        }
    }
}
