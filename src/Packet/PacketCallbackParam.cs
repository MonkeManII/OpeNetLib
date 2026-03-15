using System.Net;

namespace OpeNetLib.Packet
{
    /// <summary>
    /// Contains info regarding a UDP packet callback.
    /// </summary>
    /// <param name="Origin">The origin of the sender of this packet.</param>
    /// <param name="Data">The data stored in this packet.</param>
    /// <param name="Server">The <see cref="OpeNetLib.Server"/> that initiated this callback, if applicable.</param>
    /// <param name="Client">The <see cref="OpeNetLib.Client"/> that initiated this callback, if applicable.</param>
    /// <remarks>
    /// <see cref="Origin"/> is the <see cref="IPEndPoint"/> of the <i>sender</i>, not the reciever.
    /// <para>Therefore, for a "return to sender," you must know the port of the reciever.</para>
    /// </remarks>
    public class PacketCallbackParam(IPEndPoint Origin, byte[] Data, Server? Server, Client? Client)
    {
        /// <summary>
        /// The origin of the sender of this packet.
        /// </summary>
        public IPEndPoint Origin { get; init; } = Origin;
        /// <summary>
        /// The data stored in this packet.
        /// </summary>
        public byte[] Data { get; init; } = Data;
        /// <summary>
        /// The <see cref="OpeNetLib.Server"/> that initiated this callback, if applicable.
        /// </summary>
        public Server? Server { get; internal set; } = Server;
        /// <summary>
        /// The <see cref="OpeNetLib.Client"/> that initiated this callback, if applicable.
        /// </summary>
        public Client? Client { get; internal set; } = Client;
    }
}
