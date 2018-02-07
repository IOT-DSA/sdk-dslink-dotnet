using System;

namespace DSLink.Logger
{
    /// <summary>
    /// Default logger for platforms that support using System.Console.WriteLine.
    /// </summary>
    public class ConsoleLogger : BaseLogger
    {
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

