using System;
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

		public readonly int Level;
        public readonly string Name;

        private LogLevel(int level, string name)
        {
            Level = level;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public bool DoesPrint(LogLevel toPrint)
        {
            return toPrint.Level >= Level;
        }

        public LogLevel ParseLogLevel(string name)
        {
            return LogLevels[name.ToUpper()];
        }
    }
}

