using System;

namespace DSLink.Util.Logger
{
    public abstract class BaseLogger
    {
        public static Type Logger = typeof(DiagnosticsLogger);

        protected readonly string Name;
        protected readonly LogLevel ToPrint;

        protected BaseLogger(string name, LogLevel toPrint)
        {
            Name = name;
            ToPrint = toPrint;
        }

        public void Error(string message)
        {
            Print(LogLevel.Error, message);
        }

        public void Warning(string message)
        {
            Print(LogLevel.Warning, message);
        }

        public void Info(string message)
        {
            Print(LogLevel.Info, message);
        }

        public void Debug(string message)
        {
            Print(LogLevel.Debug, message);
        }

        public virtual string Format(LogLevel logLevel, string message)
        {
            return string.Format("[{0}][{1}][{2}] {3}", DateTime.Now.ToString("MM-dd HH:mm:ss.fff"), Name, logLevel, message);
        }

        public abstract void Print(LogLevel logLevel, string message);
    }
}

