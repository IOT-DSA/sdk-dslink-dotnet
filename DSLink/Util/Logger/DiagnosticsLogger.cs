using DSLink.Util.Logger;
using DebugLog = System.Diagnostics.Debug;

namespace DSLink.Util.Logger
{
    public class DiagnosticsLogger : BaseLogger
    {
        public DiagnosticsLogger(string name, LogLevel toPrint) : base(name, toPrint)
        {
        }

        public override void Print(LogLevel logLevel, string message)
        {
            if (logLevel.DoesPrint(ToPrint))
            {
                DebugLog.WriteLine(Format(logLevel, message));
            }
        }
    }
}

