using OpeNetLib.Internals;
using System.Net;

namespace OpeNetLib
{
    /// <summary>
    /// Holds data that a <see cref="Server"/> and a <see cref="Client"/> know about each other.
    /// </summary>
    public sealed class Connection
    {
        /// <summary>
        /// The <see cref="IPEndPoint"/> of the other's receiver port.
        /// </summary>
        public readonly IPEndPoint RecieverEndpoint;

        /// <summary>
        /// The <see cref="UdpTwoWay"/> used to contact the other.
        /// </summary>
        internal readonly UdpTwoWay Contact;

        /// <summary>
        /// The maximum timeout (in milliseconds) before this <see cref="Connection"/> terminates.
        /// </summary>
        public readonly int MaxTimeout;

        /// <summary>
        /// The current time remaining before timeout.
        /// </summary>
        internal double TimeoutMS;

        /// <summary>
        /// The time of the last timeout update, in UTC.
        /// </summary>
        DateTime? LastTimeoutUpdate = null;

        /// <summary>
        /// Creates a new <see cref="Connection"/>.
        /// </summary>
        /// <param name="RecieverEndpoint">The <see cref="IPEndPoint"/> of the other's receiver port.</param>
        /// <param name="Contact">The <see cref="UdpTwoWay"/> used to contact the other.</param>
        /// <param name="MaxTimeout">The maximum timeout (in milliseconds) before this <see cref="Connection"/> terminates.</param>
        /// <exception cref="ArgumentException"></exception>
        internal Connection(IPEndPoint RecieverEndpoint, UdpTwoWay Contact, int MaxTimeout)
        {
            if (MaxTimeout < 0)
            {
                throw new ArgumentException("Argument cannot be negative.", nameof(MaxTimeout));
            }

            this.RecieverEndpoint = RecieverEndpoint;
            this.Contact = Contact;
            this.MaxTimeout = MaxTimeout;
            
            TimeoutMS = MaxTimeout;
        }

        /// <summary>
        /// Resets the timeout timer of this <see cref="Connection"/>.
        /// </summary>
        internal void OnHeartbeat()
        {
            TimeoutMS = MaxTimeout;
            LastTimeoutUpdate = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the timeout ticker of this <see cref="Connection"/>.
        /// </summary>
        /// <returns>Whether the connection has timed out.</returns>
        internal bool UpdateTimeout()
        {
            if (LastTimeoutUpdate is null)
            {
                LastTimeoutUpdate = DateTime.UtcNow;
                return false;
            }

            TimeSpan passed = DateTime.UtcNow - LastTimeoutUpdate.Value;
            TimeoutMS -= passed.TotalMilliseconds;
            LastTimeoutUpdate = DateTime.UtcNow;

            return TimeoutMS <= 0;
        }

        /// <summary>
        /// Sends a block of <see cref="byte"/> data over this <see cref="Connection"/>.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <returns>A <see cref="Task"/> that completes when the data is sent.</returns>
        public async Task Send(byte[] data)
        {
            await Contact.Send(data, RecieverEndpoint);
        }
    }
}
