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
        }

        /// <summary>
        /// Waits until this reciever recieves a packet, and returns the contents of said packet.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{}>"/> that completes when a packet is recieved.
        /// </returns>
        internal async Task<PacketCallbackParam> ReceiveFirstAsync(int? msTimeout = null)
        {
            UdpReceiveResult result;

            using CancellationTokenSource tokenSrc = new();
            if (msTimeout is not null)
                tokenSrc.CancelAfter((int)msTimeout);
            result = await _udpClient.ReceiveAsync(tokenSrc.Token);

            return new PacketCallbackParam(result.RemoteEndPoint, result.Buffer, null, null);
        }

        /// <summary>
        ///     Recieves the first <see cref="PacketCallbackParam"/> queued for recieving.
        ///     <para>
        ///         If no packet is available, do not wait, and return null.
        ///     </para>
        /// </summary>
        /// <returns>The first <see cref="PacketCallbackParam"/> in reception, or null if none.</returns>
        internal PacketCallbackParam? RecieveFirst()
        {
            PacketCallbackParam? result = null;

            if (_udpClient.Available > 0)
            {
                IPEndPoint? _outEP = null;
                byte[] bytes = _udpClient.Receive(ref _outEP);

                if (_outEP is not null)
                {
                    result = new(_outEP, bytes, null, null);
                }
            }

            return result;
        }

        public void Dispose()
        {
            _udpClient.Close();
            _udpClient.Dispose();
        }
    }
}
