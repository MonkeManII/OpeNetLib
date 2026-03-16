using OpeNetLib.Internals;

namespace OpeNetLib.Threading
{
    /// <summary>
    /// Represents the <see cref="Thread"/> parameter for a <see cref="TwoWayPollThread"/>.
    /// </summary>
    internal sealed class UdpMessageThreadParam
    {
        /// <summary>
        /// The <see cref="OpeNetLib.Client"/> that initiated this thread, if applicable.
        /// </summary>
        internal Client? Client;

        /// <summary>
        /// The <see cref="OpeNetLib.Server"/> that initiated this thread, if applicable.
        /// </summary>
        internal Server? Server;

        /// <summary>
        /// The callback called every time this <see cref="TwoWayPollThread"/> ticks.
        /// </summary>
        internal ThreadTickCallback? Callback;

        /// <summary>
        /// The time, in milliseconds, taken between ticks.
        /// </summary>
        internal int MsDelay;

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
        /// Marks this <see cref="TwoWayPollThread"/> as completely stopped.
        /// </summary>
        internal void MarkStopped()
        {
            SetFlag(LoopingThreadState.WrappingUp, false);
            SetFlag(LoopingThreadState.Running, false);
        }

        /// <summary>
        /// Marks this <see cref="TwoWayPollThread"/> as up and running.
        /// </summary>
        internal void MarkRunning()
        {
            SetFlag(LoopingThreadState.WrappingUp, false);
            SetFlag(LoopingThreadState.Running, true);
        }

        /// <summary>
        /// Tells this <see cref="TwoWayPollThread"/> to stop execution.
        /// </summary>
        internal void MarkStopping()
        {
            SetFlag(LoopingThreadState.WrappingUp, true);
        }

        /// <summary>
        /// Checks whether this <see cref="TwoWayPollThread"/> is running.
        /// </summary>
        /// <returns>Whether the thread is marked as running.</returns>
        internal bool IsRunning()
        {
            return State.HasFlag(LoopingThreadState.Running);
        }

        /// <summary>
        /// Checks whether this <see cref="TwoWayPollThread"/> is stopping, but not stopped.
        /// </summary>
        /// <returns>Whether the thread is wrapping up before stopping.</returns>
        internal bool IsStopping()
        {
            return State.HasFlag(LoopingThreadState.WrappingUp);
        }
    }
}
