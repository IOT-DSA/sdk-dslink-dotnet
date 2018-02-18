using System;

namespace DSLink.Logger
{
    /// <summary>
    /// Interface to implement a platform-specific Logger.
    /// </summary>
    public abstract class BaseLogger
    {
        /// <summary>
        /// Display name of the logger.
        /// </summary>
        protected readonly string Name;

        /// <summary>
        /// Highest level of log to print.
        /// </summary>
        public readonly LogLevel ToPrint;

        protected BaseLogger(string name, LogLevel toPrint)
        {
            Name = name;
            ToPrint = toPrint;
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">Message</param>
        public void Error(string message)
        {
            Print(LogLevel.Error, message);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">Message</param>
        public void Warning(string message)
        {
            Print(LogLevel.Warning, message);
        }

        /// <summary>
        /// Logs an info message.
        /// </summary>
        /// <param name="message">Message</param>
        public void Info(string message)
        {
            Print(LogLevel.Info, message);
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">Message</param>
        public void Debug(string message)
        {
            Print(LogLevel.Debug, message);
        }

        /// <summary>
        /// Formats a string for output to logs.
        /// </summary>
        /// <param name="logLevel">Log level.</param>
        /// <param name="message">Message.</param>
        public virtual string Format(LogLevel logLevel, string message)
        {
            return string.Format("[{0}][{1}][{2}] {3}", DateTime.Now.ToString("MM-dd HH:mm:ss.fff"), Name, logLevel, message);
        }

        /// <summary>
        /// Print the specified message.
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="message">Message</param>
        public abstract void Print(LogLevel logLevel, string message);
    }
}

