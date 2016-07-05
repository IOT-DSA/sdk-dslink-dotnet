using DebugLog = System.Diagnostics.Debug;

namespace DSLink.Util.Logger
{
    /// <summary>
    /// Default logger for platforms without proper logging.
    /// </summary>
    public class DiagnosticsLogger : BaseLogger
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Util.Logger.DiagnosticsLogger"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="toPrint">To print</param>
        public DiagnosticsLogger(string name, LogLevel toPrint) : base(name, toPrint)
        {
        }

        /// <summary>
        /// Prints a message to the console.
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="message">Message</param>
        public override void Print(LogLevel logLevel, string message)
        {
            if (logLevel.DoesPrint(ToPrint))
            {
                DebugLog.WriteLine(Format(logLevel, message));
            }
        }
    }
}

