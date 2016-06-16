namespace DSLink.Universal
{
    public class UniversalPlatform
    {
        public static void Initiailize()
        {
            Websockets.Universal.WebsocketConnection.Link();
        }
    }
}
