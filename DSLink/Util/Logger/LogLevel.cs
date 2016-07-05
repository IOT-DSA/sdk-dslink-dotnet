using System.Collections.Generic;

namespace DSLink.Util.Logger
{
    public class LogLevel
    {
        public static readonly LogLevel Error = new LogLevel(0, "ERROR");
        public static readonly LogLevel Warning = new LogLevel(1, "WARNING");
        public static readonly LogLevel Info = new LogLevel(2, "INFO");
        public static readonly LogLevel Debug = new LogLevel(3, "DEBUG");
        private static Dictionary<string, LogLevel> LogLevels = new Dictionary<string, LogLevel>
        {
            {Error.Name, Error},
            {Warning.Name, Warning},
            {Info.Name, Info},
            {Debug.Name, Debug}
        };

        /// <summary>
        /// Level of log message.
        /// </summary>
		public readonly int Level;

        /// <summary>
        /// Log level name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Util.Logger.LogLevel"/> class.
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="name">Name</param>
        private LogLevel(int level, string name)
        {
            Level = level;
            Name = name;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>The string</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Checks if the logger should print this message.
        /// </summary>
        /// <param name="toPrint">To print</param>
        /// <returns>Whether the logger should print</returns>
        public bool DoesPrint(LogLevel toPrint)
        {
            return toPrint.Level >= Level;
        }

        /// <summary>
        /// Parses the log level.
        /// </summary>
        /// <returns>The log level</returns>
        /// <param name="name">Name</param>
        public LogLevel ParseLogLevel(string name)
        {
            return LogLevels[name.ToUpper()];
        }
    }
}

