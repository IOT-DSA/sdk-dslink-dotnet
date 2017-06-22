using DSLink.Connection;
using DSLink.Util.Logger;
using PCLStorage;
using System;
using System.Threading.Tasks;

namespace DSLink.Platform
{
    public abstract class BasePlatform
    {
        public static BasePlatform Current
        {
            get;
            private set;
        }
        
        public virtual void Init()
        {
        }

        public virtual Connector CreateConnector(DSLinkContainer container)
        {
            return new WebSocketBaseConnector(container);
        }

        public virtual IFolder GetPlatformStorageFolder()
        {
            return null;
        }

        public virtual string GetCommunicationFormat()
        {
            return string.Empty;
        }

        protected virtual Type GetLoggerType()
        {
            return typeof(DiagnosticsLogger);
        }

        public BaseLogger CreateLogger(string loggerName, LogLevel logLevel)
        {
            return (BaseLogger)Activator.CreateInstance(GetLoggerType(), loggerName, logLevel);
        }
        
        public async Task<IFolder> GetStorageFolder()
        {
            var platformFolder = GetPlatformStorageFolder();
            if (platformFolder == null)
            {
                return await FileSystem.Current.GetFolderFromPathAsync(".");
            }
            return platformFolder;
        }

        public static void SetPlatform(BasePlatform platform)
        {
            Current = platform;
            Current.Init();
        }
    }
}
