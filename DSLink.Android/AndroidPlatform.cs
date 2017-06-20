using System;
using DSLink.Platform;
using PCLStorage;

namespace DSLink.Android
{
    public class AndroidPlatform : BasePlatform
    {
        public static void Initialize()
        {
            SetPlatform(new AndroidPlatform());
            Websockets.Droid.WebsocketConnection.Link();
        }

        public override IFolder GetPlatformStorageFolder()
        {
            return FileSystem.Current.LocalStorage;
        }

        protected override Type GetLoggerType()
        {
            return typeof(AndroidLogger);
        }
    }
}
