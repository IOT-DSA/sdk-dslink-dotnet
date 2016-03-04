using System;
using DSLink.Connection.Serializer;

namespace DSLink.Connection
{
    public class ConnectorManager
    {
        private static Type _connectorType;

        public static Connector Create(Configuration config, ISerializer serializer)
        {
            return (Connector) Activator.CreateInstance(_connectorType, config, serializer);
        }

        public static void SetConnector(Type type)
        {
            if (_connectorType == null)
            {
                _connectorType = type;
            }
        }
    }
}
