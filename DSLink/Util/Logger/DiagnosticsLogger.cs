using System;

namespace DSLink.Util.Logger
{
    /// <summary>
    /// Default logger for platforms without proper logging.
    /// </summary>
    public class ConsoleLogger : BaseLogger
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Util.Logger.DiagnosticsLogger"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="toPrint">To print</param>
        public ConsoleLogger(string name, LogLevel toPrint) : base(name, toPrint)
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
                Console.WriteLine(Format(logLevel, message));
            }
        }
    }
}

