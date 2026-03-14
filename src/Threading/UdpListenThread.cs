
using OpeNetLib.Internals;
using System.Collections.Frozen;
using System.Threading.Tasks;

namespace OpeNetLib.Threading
{
    /// <summary>
    /// Represents a <see cref="Thread"/> that polls for UDP packets.
    /// </summary>
    internal class UdpListenThread
    {
        /// <summary>
        /// The internal thread that actually runs the loop.
        /// </summary>
        readonly Thread _thread;

        /// <summary>
        /// The thread parameters that allow for stopping the thread.
        /// </summary>
        readonly UdpListenThreadParam _param;

        /// <summary>
        /// The <see cref="HashSet{UdpTwoWay}"/> used to queue polls into <see cref="_param.Polls"/>
        /// </summary>
        HashSet<UdpTwoWay> QueuedAddPolls;

        /// <summary>
        /// Creates a new <see cref="UdpListenThread"/> with the specified ID.
        /// </summary>
        /// <param name="id">The <see cref="Thread"/> ID, for debugging purposes.</param>
        public UdpListenThread(string? id = null)
        {
            _thread = new(Listen);
            _param = new();

            if (id is not null)
            {
                _thread.Name = id;
            }
        }

        /// <summary>
        /// Starts this <see cref="Thread"/> if it wasn't already running.
        /// </summary>
        public void StartThread()
        {
            if (_param.IsStopping() || _param.IsRunning()) return;
            _param.MarkRunning();
            _thread.Start(_param);
        }

        /// <summary>
        /// Checks whether this <see cref="Thread"/> is listening for UDP packets.
        /// </summary>
        /// <returns>Whether the thread is actively listening for packets.</returns>
        public bool IsListening() => _param.IsRunning();

        /// <summary>
        /// Awaits the stopping of the polling <see cref="Thread"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> that completes when the <see cref="Thread"/> exits.</returns>
        public async Task AwaitStop()
        {
            while (IsListening())
            {
                await Task.Delay(1);
            }
        }

        /// <summary>
        /// Awaits the starting of the polling <see cref="Thread"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> that completes when the <see cref="Thread"/> begins executing code.</returns>
        public async Task AwaitStart()
        {
            while (!IsListening())
            {
                await Task.Delay(1);
            }
        }

        /// <summary>
        /// Stops the polling <see cref="Thread"/> and waits until it has finished wrapping up.
        /// </summary>
        /// <returns>A <see cref="Task"/> that completes when the <see cref="Thread"/> exits.</returns>
        public async Task StopAndWait()
        {
            _param.MarkStopping();
            await AwaitStop();
        }

        /// <summary>
        /// Adds a <see cref="UdpTwoWay"/> poll.
        /// </summary>
        /// <param name="poll">The poll to listen to.</param>
        /// <returns>Whether the poll was successfully added.</returns>
        public bool AddPoll(UdpTwoWay poll)
        {
            return _param.Polls.Add(poll);
        }

        /// <summary>
        /// Removes a <see cref="UdpTwoWay"/> poll.
        /// </summary>
        /// <param name="poll">The poll to remove.</param>
        /// <returns>Whether the poll was successfully removed.</returns>
        public bool RemovePoll(UdpTwoWay poll)
        {
            return _param.Polls.Remove(poll);
        }

        /// <summary>
        /// Removes all polls from this <see cref="UdpListenThread"/>.
        /// </summary>
        public void ClearPolls()
        {
            _param.Polls.Clear();
        }

        /// <summary>
        /// The main loop of the polling <see cref="Thread"/>.
        /// </summary>
        /// <param name="possibleParam">A <see cref="UdpListenThreadParam"/> controlling the state of the thread.</param>
        /// <exception cref="ArgumentException"></exception>
        void Listen(object? possibleParam)
        {
            if (possibleParam is not UdpListenThreadParam param)
            {
                throw new ArgumentException(
                    $"Argument to {nameof(Listen)} must be of type {nameof(UdpListenThreadParam)}!",
                    nameof(possibleParam)
                );
            }

            try
            {
                while (!param.IsStopping())
                {
                    // TODO make it not use toarray because apparently locking isnt enough
                    foreach (UdpTwoWay poll in param.Polls.ToArray())
                    {
                        poll.ListenForPackets();
                    }
                    Thread.Sleep(1);
                }
            } catch (Exception)
            {
                param.MarkStopped();
                throw;
            }
        }
    }
}
