using System;
using DSLink.Connection;
using DSLink.Platform;

namespace DSLink.NET
{
    public class NETPlatform : BasePlatform
    {
        public static void Initialize()
        {
            SetPlatform(new NETPlatform());
        }

        public override Connector CreateConnector(DSLinkContainer container)
        {
            return new WebSocketSharpConnector(container.Config, container.Logger);
        }

        protected override Type GetLoggerType()
        {
            return typeof(NETLogger);
        }
    }
}
