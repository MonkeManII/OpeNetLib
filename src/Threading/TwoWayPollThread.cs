
using OpeNetLib.Internals;

namespace OpeNetLib.Threading
{
    /// <summary>
    /// Represents a <see cref="Thread"/> that polls for UDP packets.
    /// </summary>
    internal sealed class TwoWayPollThread
    {
        /// <summary>
        /// The internal thread that actually runs the loop.
        /// </summary>
        readonly Thread _thread;

        /// <summary>
        /// The thread parameters that allow for stopping the thread.
        /// </summary>
        readonly UdpMessageThreadParam _param;

        /// <summary>
        /// The <see cref="HashSet{UdpTwoWay}"/> used to queue polls into <see cref="_param.Polls"/>
        /// </summary>
        readonly HashSet<UdpTwoWay> QueuedAddPolls = [];

        bool pollsLocked = false;

        /// <summary>
        /// Creates a new <see cref="TwoWayPollThread"/> with the specified ID.
        /// </summary>
        /// <param name="id">The <see cref="Thread"/> ID, for debugging purposes.</param>
        public TwoWayPollThread(string? id = null, Client? client = null, Server? server = null, ThreadTickCallback? callback = null)
        {
            _thread = new(Listen)
            {
                IsBackground = false
            };

            _param = new()
            {
                Client = client,
                Server = server,
                Callback = callback,
                MsDelay = 5
            };

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
            pollsLocked = false;
            await AwaitStop();
        }

        /// <summary>
        /// Adds all polls in <see cref="QueuedAddPolls"/> to <see cref="_param.Polls"/>.
        /// </summary>
        void AddQueuedPolls()
        {
            foreach (UdpTwoWay poll in QueuedAddPolls)
            {
                _param.Polls.Add(poll);
            }
            QueuedAddPolls.Clear();
        }

        /// <summary>
        /// Adds a <see cref="UdpTwoWay"/> poll.
        /// </summary>
        /// <param name="poll">The poll to listen to.</param>
        /// <returns>Whether the poll was successfully added.</returns>
        public bool AddPoll(UdpTwoWay poll)
        {
            if (pollsLocked)
            {
                QueuedAddPolls.Add(poll);
                return !_param.Polls.Contains(poll);
            } else
            {
                return _param.Polls.Add(poll);
            }
        }

        /// <summary>
        /// The main loop of the polling <see cref="Thread"/>.
        /// </summary>
        /// <param name="possibleParam">A <see cref="UdpMessageThreadParam"/> controlling the state of the thread.</param>
        /// <exception cref="ArgumentException"></exception>
        async void Listen(object? possibleParam)
        {
            if (possibleParam is not UdpMessageThreadParam param)
            {
                throw new ArgumentException(
                    $"Argument to {nameof(Listen)} must be of type {nameof(UdpMessageThreadParam)}!",
                    nameof(possibleParam)
                );
            }

            try
            {
                while (!param.IsStopping())
                {
                    pollsLocked = true;
                    foreach (UdpTwoWay poll in param.Polls)
                    {
                        poll.ListenForPackets(param.Client, param.Server);
                    }
                    pollsLocked = false;
                    _param.Callback?.Invoke(_param);
                    AddQueuedPolls();
                    Thread.Sleep(_param.MsDelay);
                }
            } catch (Exception e)
            {
                param.MarkStopped();
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
