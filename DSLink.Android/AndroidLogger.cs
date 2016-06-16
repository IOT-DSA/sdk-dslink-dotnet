using System;
using Android.Util;
using DSLink.Util.Logger;
using Java.Lang;

namespace DSLink.Android
{
    public class AndroidLogger : BaseLogger
    {
        public AndroidLogger(string name, LogLevel toPrint) : base(name, toPrint)
        {
        }

        public override void Print(LogLevel logLevel, string message)
        {
            if (logLevel.DoesPrint(ToPrint))
            {
                JavaSystem.Out.Println(Format(logLevel, message));
            }
        }
    }
}

