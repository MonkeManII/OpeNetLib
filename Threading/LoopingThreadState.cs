
namespace OpeNetLib.Threading
{
    /// <summary>
    /// Represents the state of a looping <see cref="Thread"/>.
    /// </summary>
    enum LoopingThreadState
    {
        /// <summary>
        /// The <see cref="Thread"/> is active and running.
        /// </summary>
        Running = 0b01,

        /// <summary>
        /// The <see cref="Thread"/> is marked for stopping, but has not stopped yet.
        /// </summary>
        WrappingUp = 0b10
    }
}
