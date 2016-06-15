using DSLink.Connection;

namespace DSLink.NET
{
	public static class NETPlatform
	{
		public static void Initialize()
		{
			Websockets.Net.WebsocketConnection.Link();
			ConnectorManager.SetConnector(typeof(WebSocketBaseConnector));
		}
	}
}
