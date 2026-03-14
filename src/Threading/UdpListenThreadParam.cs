using OpeNetLib.Internals;

namespace OpeNetLib.Threading
{
    /// <summary>
    /// Represents the <see cref="Thread"/> parameter for a <see cref="UdpListenThread"/>.
    /// </summary>
    internal class UdpListenThreadParam
    {
        /// <summary>
        /// The state of the thread.
        /// </summary>
        LoopingThreadState State;

        void SetFlag(LoopingThreadState flag, bool value)
        {
            if (value)
                State |= flag;
            else
                State &= ~flag;
        }

        /// <summary>
        /// A <see cref="HashSet{UdpTwoWay}"/> containing all the two-ways to poll.
        /// </summary>
        internal readonly HashSet<UdpTwoWay> Polls = [];

        /// <summary>
        /// Marks this <see cref="UdpListenThread"/> as completely stopped.
        /// </summary>
        internal void MarkStopped()
        {
            SetFlag(LoopingThreadState.WrappingUp, false);
            SetFlag(LoopingThreadState.Running, false);
        }

        /// <summary>
        /// Marks this <see cref="UdpListenThread"/> as up and running.
        /// </summary>
        internal void MarkRunning()
        {
            SetFlag(LoopingThreadState.WrappingUp, false);
            SetFlag(LoopingThreadState.Running, true);
        }

        /// <summary>
        /// Tells this <see cref="UdpListenThread"/> to stop execution.
        /// </summary>
        internal void MarkStopping()
        {
            SetFlag(LoopingThreadState.WrappingUp, true);
        }

        /// <summary>
        /// Checks whether this <see cref="UdpListenThread"/> is running.
        /// </summary>
        /// <returns>Whether the thread is marked as running.</returns>
        internal bool IsRunning()
        {
            return State.HasFlag(LoopingThreadState.Running);
        }

        /// <summary>
        /// Checks whether this <see cref="UdpListenThread"/> is stopping, but not stopped.
        /// </summary>
        /// <returns>Whether the thread is wrapping up before stopping.</returns>
        internal bool IsStopping()
        {
            return State.HasFlag(LoopingThreadState.WrappingUp);
        }
    }
}
